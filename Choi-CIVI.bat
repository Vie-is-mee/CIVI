@echo off
setlocal
set "CIVI_LAUNCHER_SELF=%~f0"
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoLogo -NoProfile -Command "$ErrorActionPreference='Stop'; try { $text=[IO.File]::ReadAllText($env:CIVI_LAUNCHER_SELF); $parts=$text -split '(?m)^#<CIVI-POWERSHELL>\r?$',2; if($parts.Length -ne 2){throw 'Invalid CIVI launcher'}; & ([scriptblock]::Create($parts[1])) } catch { Write-Host ('CIVI could not start: '+$_.Exception.Message) -ForegroundColor Red; exit 1 }"
set "CIVI_RESULT=%ERRORLEVEL%"
if not "%CIVI_RESULT%"=="0" (
    echo Please check your connection, free disk space, or send this message to the CIVI team.
    pause
)
exit /b %CIVI_RESULT%
#<CIVI-POWERSHELL>
# Windows PowerShell 5.1. Functions only: the BAT generator adds the entry point.
Set-StrictMode -Version Latest

function Get-CiviSha256([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') }
    finally { $sha.Dispose(); $stream.Dispose() }
}

function Assert-CiviChildPath([string]$Root, [string]$Path) {
    $parent = [IO.Path]::GetFullPath($Root).TrimEnd('\') + '\'
    $child = [IO.Path]::GetFullPath($Path)
    if (-not $child.StartsWith($parent, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path outside CIVI installation: $child"
    }
    $current = $child
    while ($current -and $current.Length -ge $parent.TrimEnd('\').Length) {
        if (Test-Path -LiteralPath $current) {
            $item = Get-Item -LiteralPath $current -Force
            $linkType = $item.PSObject.Properties['LinkType']
            # OneDrive placeholders are also reparse points; only actual links redirect paths.
            if ($linkType -and $linkType.Value -in @('Junction','SymbolicLink')) {
                throw "CIVI will not modify a junction or symbolic link: $current"
            }
        }
        $current = [IO.Path]::GetDirectoryName($current)
    }
}

function Remove-CiviTemporary([string]$Root, [string]$Path) {
    Assert-CiviChildPath $Root $Path
    if (Test-Path -LiteralPath $Path) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Test-CiviPayload($Config, [string]$Directory) {
    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) { return $false }
    foreach ($file in $Config.Files) {
        $path = Join-Path $Directory $file.Path
        Assert-CiviChildPath $Directory $path
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { return $false }
        if ((Get-Item -LiteralPath $path).Length -ne [long]$file.Size) { return $false }
        if ((Get-CiviSha256 $path) -ne $file.Sha256) { return $false }
    }
    return $true
}

function Test-CiviInstallation($Config, [string]$Directory) {
    $marker = Join-Path $Directory 'civi-install.json'
    if (-not (Test-Path -LiteralPath $marker -PathType Leaf)) { return $false }
    try {
        $saved = Get-Content -LiteralPath $marker -Raw | ConvertFrom-Json
        if ($saved.Version -ne $Config.Version -or $saved.ArchiveSha256 -ne $Config.ArchiveSha256) { return $false }
    } catch { return $false }
    return Test-CiviPayload $Config $Directory
}

function Assert-CiviArchive($Config, [string]$Archive) {
    if ((Get-CiviSha256 $Archive) -ne $Config.ArchiveSha256) {
        throw 'The downloaded ZIP is incomplete or does not match this launcher. Nothing was installed. Download the launcher and ZIP from the same release.'
    }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($Archive)
    try {
        $seen = @{}
        foreach ($entry in $zip.Entries) {
            $name = $entry.FullName
            if ($name.Contains('\') -or $name.Contains(':') -or $name -match '(^|/)\.\.?(/|$)' -or
                -not $name.StartsWith('CIVI-Trang17/', [StringComparison]::Ordinal)) {
                throw 'Unsafe path in release archive.'
            }
            if ($entry.Name -eq '') { continue }
            if ($seen.ContainsKey($name)) { throw 'Duplicate path in release archive.' }
            $seen[$name] = $true
            if ((($entry.ExternalAttributes -shr 16) -band 0xF000) -eq 0xA000) { throw 'Symbolic links are not supported in release archives.' }
        }
        if ($seen.Count -ne $Config.Files.Count) { throw 'Unexpected file count in release archive.' }
        foreach ($file in $Config.Files) {
            if (-not $seen.ContainsKey('CIVI-Trang17/' + $file.Path.Replace('\','/'))) { throw 'Release archive is missing a file.' }
        }
    } finally { $zip.Dispose() }
}

function Install-CiviPayload($Config, [string]$Root, [string]$Destination, [string]$LocalArchive) {
    # Keep staging shallow: Windows PowerShell 5.1 uses legacy MAX_PATH APIs.
    $stage = Join-Path $Root ('.install-' + [guid]::NewGuid().ToString('N').Substring(0,16))
    $backup = Join-Path $Root ('.previous-' + [guid]::NewGuid().ToString('N'))
    Assert-CiviChildPath $Root $stage
    Assert-CiviChildPath $Root $backup
    Assert-CiviChildPath $Root $Destination
    New-Item -ItemType Directory -Path $stage | Out-Null
    try {
        $archive = $LocalArchive
        if (-not $archive -or -not (Test-Path -LiteralPath $archive -PathType Leaf)) {
            if ($Config.Repository -notmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$') {
                throw 'No GitHub release configured. Place the release ZIP next to this BAT, or use a launcher from a published release.'
            }
            $uri = 'https://github.com/' + $Config.Repository + '/releases/download/' + $Config.Tag + '/' + $Config.ArchiveName
            $archive = Join-Path $stage 'download.zip'
            Write-Host ('Downloading CIVI ' + $Config.Version + ' from ' + $Config.Repository + ' ...')
            [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
            $oldProgress = $ProgressPreference
            try {
                $ProgressPreference = 'SilentlyContinue'
                Invoke-WebRequest -Uri $uri -UseBasicParsing -OutFile $archive -TimeoutSec 180 -MaximumRedirection 5
            } finally { $ProgressPreference = $oldProgress }
        } else { Write-Host 'Using the release ZIP next to the launcher.' }
        Write-Host 'Checking download ...'
        Assert-CiviArchive $Config $archive
        $unpack = $stage
        [IO.Compression.ZipFile]::ExtractToDirectory($archive, $unpack)
        $payload = Join-Path $unpack 'CIVI-Trang17'
        if (-not (Test-CiviPayload $Config $payload)) { throw 'Extracted files failed verification. Existing installation was kept.' }
        @{ Version=$Config.Version; ArchiveSha256=$Config.ArchiveSha256 } | ConvertTo-Json |
            Set-Content -LiteralPath (Join-Path $payload 'civi-install.json') -Encoding UTF8
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Destination) | Out-Null
        $movedPrevious = $false
        if (Test-Path -LiteralPath $Destination) {
            Move-Item -LiteralPath $Destination -Destination $backup
            $movedPrevious = $true
        }
        try { Move-Item -LiteralPath $payload -Destination $Destination }
        catch {
            if ($movedPrevious -and -not (Test-Path -LiteralPath $Destination)) {
                Move-Item -LiteralPath $backup -Destination $Destination
            }
            throw
        }
        # Only a verified replacement may discard the previous copy of this version.
        if ($movedPrevious) {
            try { Remove-CiviTemporary $Root $backup }
            catch { Write-Warning ('Installed successfully; previous copy could not be removed: ' + $backup) }
        }
        Write-Host ('Installed CIVI ' + $Config.Version + '.')
    } finally {
        try { Remove-CiviTemporary $Root $stage }
        catch { Write-Warning ('Could not clean temporary installer files: ' + $stage) }
    }
}

function Initialize-CiviShortcut {
    if ('CiviShortcut' -as [type]) { return }
    # IShellLinkW keeps Vietnamese profile paths intact, unlike some WSH versions.
    Add-Type -TypeDefinition @'
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
[ComImport, Guid("00021401-0000-0000-C000-000000000046")]
internal class CiviShellLink {}
[ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICiviShellLinkW {
    void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int size, IntPtr findData, uint flags);
    void GetIDList(out IntPtr list);
    void SetIDList(IntPtr list);
    void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder text, int size);
    void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string text);
    void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int size);
    void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string path);
    void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder text, int size);
    void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string text);
    void GetHotkey(out short hotkey);
    void SetHotkey(short hotkey);
    void GetShowCmd(out int command);
    void SetShowCmd(int command);
    void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder path, int size, out int index);
    void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string path, int index);
    void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string path, uint reserved);
    void Resolve(IntPtr window, uint flags);
    void SetPath([MarshalAs(UnmanagedType.LPWStr)] string path);
}
public static class CiviShortcut {
    public static void Write(string file, string target, string directory) {
        object obj = new CiviShellLink();
        try {
            var link = (ICiviShellLinkW)obj;
            link.SetPath(target); link.SetWorkingDirectory(directory);
            link.SetDescription("CIVI - Trang 17"); link.SetIconLocation(target, 0);
            ((IPersistFile)obj).Save(file, true);
        } finally { Marshal.FinalReleaseComObject(obj); }
    }
    public static string Read(string file) {
        object obj = new CiviShellLink();
        try {
            ((IPersistFile)obj).Load(file, 0);
            var text = new StringBuilder(32768);
            ((ICiviShellLinkW)obj).GetPath(text, text.Capacity, IntPtr.Zero, 4);
            return text.ToString();
        } finally { Marshal.FinalReleaseComObject(obj); }
    }
}
'@
}

function Set-CiviShortcut([string]$Root, [string]$Executable, [string]$Desktop) {
    if (-not $Desktop) { return }
    Initialize-CiviShortcut
    $link = Join-Path $Desktop 'CIVI - Trang 17.lnk'
    if (Test-Path -LiteralPath $link) {
        try { Assert-CiviChildPath $Root ([CiviShortcut]::Read($link)) }
        catch { Write-Warning 'A different shortcut with this name already exists. It was kept.'; return }
    }
    [CiviShortcut]::Write($link, $Executable, (Split-Path -Parent $Executable))
}

function Invoke-CiviLauncher {
    param($Config, [string]$LauncherPath, [string]$InstallRoot,
        [string]$Desktop, [switch]$NoLaunch, [switch]$NoShortcut)
    $ErrorActionPreference = 'Stop'
    if (-not [Environment]::Is64BitOperatingSystem) { throw 'This release needs 64-bit Windows.' }
    if ($PSVersionTable.PSVersion.Major -lt 5) { throw 'This launcher needs Windows PowerShell 5.1 or newer.' }
    if ($Config.Version -notmatch '^\d+\.\d+\.\d+$' -or $Config.Tag -notmatch '^v\d+\.\d+\.\d+$' -or
        $Config.ArchiveName -notmatch '^CIVI-[A-Za-z0-9.-]+\.zip$' -or $Config.ArchiveSha256 -notmatch '^[A-Fa-f0-9]{64}$') {
        throw 'Invalid release configuration.'
    }
    if (-not $InstallRoot) { $InstallRoot = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'CIVI\Trang17' }
    $InstallRoot = [IO.Path]::GetFullPath($InstallRoot)
    $destination = Join-Path $InstallRoot ('versions\' + $Config.Version)
    Assert-CiviChildPath $InstallRoot $destination
    New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null
    $lock = $null
    try {
        try { $lock = [IO.File]::Open((Join-Path $InstallRoot 'launcher.lock'), [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None) }
        catch { throw 'Another CIVI launcher is busy. Wait a moment, then try again.' }
        Write-Host 'CIVI / Trang 17 - checking installed files ...'
        if (-not (Test-CiviInstallation $Config $destination)) {
            $localArchive = Join-Path (Split-Path -Parent $LauncherPath) $Config.ArchiveName
            Install-CiviPayload $Config $InstallRoot $destination $localArchive
        } else { Write-Host 'Ready. No download needed.' }
        $executable = Join-Path $destination 'CIVI-Trang17.exe'
        if (-not $NoShortcut) {
            if (-not $Desktop) { $Desktop = [Environment]::GetFolderPath('DesktopDirectory') }
            try { Set-CiviShortcut $InstallRoot $executable $Desktop }
            catch { Write-Warning 'Could not create the Desktop shortcut. This BAT can still open the game.' }
        }
        if (-not $NoLaunch) { Start-Process -FilePath $executable -WorkingDirectory $destination | Out-Null }
        return $destination
    } finally { if ($lock) { $lock.Dispose() } }
}

$civiConfig = @'
{"Version":"0.41.1","Tag":"v0.41.1","Repository":"Vie-is-mee/CIVI","ArchiveName":"CIVI-Trang17-0.41.1-Windows.zip","ArchiveSha256":"30F5F5981BD1DD791F3BF1EEEDB7E6FC30C40C545234D1A39143A0942E3CC158","Files":[{"Path":"CIVI-Trang17.exe","Size":667136,"Sha256":"001AEEBFB92718F8B49F0C0E0A03E313C54669552B08E952B2FEE3DDD143683A"},{"Path":"UnityPlayer.dll","Size":38692784,"Sha256":"3BDC6E4CD1B5EB85E1562B1C6018949145CDEDF50CF7ACEAB0D1518143F9AA7C"},{"Path":"UnityCrashHandler64.exe","Size":1772456,"Sha256":"85096FC604D5516BE32E3AAF4840A1700B852DDC40988DFC67D5C0B583A166EB"},{"Path":"dstorage.dll","Size":206904,"Sha256":"CAE65787F61514BEC6F012F44546E68B19E39D8854A9AB52AF813370D44B272A"},{"Path":"dstoragecore.dll","Size":1482784,"Sha256":"CF85B19A92A8276F8BACA0DB9BBCD60735BAD50768DEB303D310E84D5F4FDD11"},{"Path":"CIVI-Trang17_Data/app.info","Size":28,"Sha256":"B2CD877414B1ABEAED6DEF96CF01F3AA757CA1EE112DE2A0A0EC92DE297816E0"},{"Path":"CIVI-Trang17_Data/boot.config","Size":156,"Sha256":"105840B98C8430CAEE5D78B8DF433B6EA5F807D0E704DB17C35B435C11DAA1F3"},{"Path":"CIVI-Trang17_Data/globalgamemanagers","Size":48532,"Sha256":"3316034CEDCC1F3CE31FE4B8A0F4CCCAA7823B19A3399FB27A3A7BFBE0FAC9F9"},{"Path":"CIVI-Trang17_Data/globalgamemanagers.assets","Size":82864,"Sha256":"A797A635ADF66FFE1B50A87601FEABAB38C789AF9487D4542267F23A8337E11C"},{"Path":"CIVI-Trang17_Data/globalgamemanagers.assets.resS","Size":2796240,"Sha256":"39DD38FCD8DD95D41D6D55FE16A448D29FE2C31D9A162C825C24C6D99A893D7A"},{"Path":"CIVI-Trang17_Data/level0","Size":764,"Sha256":"92686DEB3E3BB93686E7B7A13B8A98A673D20E62741D0F8C6741591063633766"},{"Path":"CIVI-Trang17_Data/resources.assets","Size":1116396,"Sha256":"599CFE293494D5B65981C85400EA5590DB379C6C39F2ABC62B961DB11DD33A50"},{"Path":"CIVI-Trang17_Data/resources.assets.resS","Size":7864320,"Sha256":"D7706C3C0B9CF29A89752F016400ABB6C27BE6B85F748BCDB6E985681CD274D2"},{"Path":"CIVI-Trang17_Data/resources.resource","Size":130912,"Sha256":"4AF34768E5DC484D16A3BF661785EA64425C60C5C7DF0FF09F37EAF5EC3168BF"},{"Path":"CIVI-Trang17_Data/RuntimeInitializeOnLoads.json","Size":1421,"Sha256":"B9A9036F8DA0FEC2A7FB9B4C50C1E387155C67CE8313804ECF4386CCF240CF7C"},{"Path":"CIVI-Trang17_Data/ScriptingAssemblies.json","Size":3531,"Sha256":"0BE430F1A28A94FB38AEA8566A01D2F1D755EF565372C5F5E95CB45B9BE270E8"},{"Path":"CIVI-Trang17_Data/sharedassets0.assets","Size":11104,"Sha256":"6071789D4BA7F759C6713D71E4EE6FC3B1DA07C6D36EE2D8D806C407A944BF27"},{"Path":"CIVI-Trang17_Data/sharedassets0.assets.resS","Size":524288,"Sha256":"393EFE6399536384AA91BDFC4E64F244B4D3E1FBB6A4FF145C483FFEE64CCB36"},{"Path":"CIVI-Trang17_Data/Managed/Assembly-CSharp.dll","Size":87552,"Sha256":"3AD3F123406D7DA98F86D65A19B2449D8A69045E0D1336F11F0A51D29FCF4AD2"},{"Path":"CIVI-Trang17_Data/Managed/Mono.Security.dll","Size":241152,"Sha256":"A7128A85C5CE748CFCC64BE97B7FAB4119F4636C917A0303BFA2582F86282ED8"},{"Path":"CIVI-Trang17_Data/Managed/mscorlib.dll","Size":4632064,"Sha256":"ACBFA8295DA5B5E5A2C68C7B2DBC9CD7A5829C0AE031AF22BE4F6A49FD626F8F"},{"Path":"CIVI-Trang17_Data/Managed/netstandard.dll","Size":90112,"Sha256":"6AE62E082DC494A2433984177F60CA4DB5FAE69B1F360A8B33754172B310B8C5"},{"Path":"CIVI-Trang17_Data/Managed/System.Buffers.dll","Size":4608,"Sha256":"762F8FDBE975E05B76BE5FE996C53CE7C75E4A2830F2F50B02A5948EF6BA0AEB"},{"Path":"CIVI-Trang17_Data/Managed/System.Collections.Immutable.dll","Size":252680,"Sha256":"5B1B1C83BA3D135C2FDFE425842FBE9C7432878B7E468623ACB554C69B4C130F"},{"Path":"CIVI-Trang17_Data/Managed/System.ComponentModel.Composition.dll","Size":257024,"Sha256":"D949BD6214C26B38A190E66FE976038E3845AEA6C6980D3EB58E78B75EDF98A5"},{"Path":"CIVI-Trang17_Data/Managed/System.Configuration.dll","Size":124928,"Sha256":"7170A8FA3363A8EBD59E8B9931509F595DF009F11E1885707A3C3BC0A3FF5648"},{"Path":"CIVI-Trang17_Data/Managed/System.Core.dll","Size":1113088,"Sha256":"15081EAC9F71FD7817A7D42A963651C89C666A4C21499F2076FB3AB7AACD9B9A"},{"Path":"CIVI-Trang17_Data/Managed/System.Data.DataSetExtensions.dll","Size":29696,"Sha256":"DA75CEEBBEF879968AAA721090DF162E7A6FE1C7BA32F4BB5D05523385C53E8C"},{"Path":"CIVI-Trang17_Data/Managed/System.Data.dll","Size":2125312,"Sha256":"557BD378CAA400C4DE52BD73225A948F1A1E41586C52E307AAA042F77B33D648"},{"Path":"CIVI-Trang17_Data/Managed/System.dll","Size":2641920,"Sha256":"0C49D192FC91F2EADE9C12C3960D8B90161560507D96EBCC9D878A2390A2CAC6"},{"Path":"CIVI-Trang17_Data/Managed/System.Drawing.dll","Size":489984,"Sha256":"1657E6EB247364CD232FC6C80DF1AB6804732B6644639F18C80E48AF1174FD63"},{"Path":"CIVI-Trang17_Data/Managed/System.EnterpriseServices.dll","Size":44544,"Sha256":"7FCC7078F244AB3A5E8549A8ACD2905B71AA8A31DDA917AB60817C5B13924D64"},{"Path":"CIVI-Trang17_Data/Managed/System.IO.Compression.dll","Size":114688,"Sha256":"35242DDC40F673D10767E3AB3381718864170CFFBC710501374E911DF700FDA0"},{"Path":"CIVI-Trang17_Data/Managed/System.IO.Compression.FileSystem.dll","Size":18432,"Sha256":"E7F2784C57088326F8DE52A79426102A2250C2F31922030E9FED0CB64740E242"},{"Path":"CIVI-Trang17_Data/Managed/System.Memory.dll","Size":4608,"Sha256":"C4F030A2CBA7DA7CDCF493257C24560E203D355904AEE490D645A935842F834A"},{"Path":"CIVI-Trang17_Data/Managed/System.Net.Http.dll","Size":122880,"Sha256":"A6227DE2834BC7019A10E69A7CA0BB0908D5092B8BC279ED428B27ECF583A681"},{"Path":"CIVI-Trang17_Data/Managed/System.Numerics.dll","Size":119296,"Sha256":"711616207139CFBF471F4F344A32ADE842077314F03C19A0846A98ECACE3FC5D"},{"Path":"CIVI-Trang17_Data/Managed/System.Reflection.Metadata.dll","Size":487584,"Sha256":"AFCFD6C166FE0D59F45CC6363082806F6A7EA8CEED9DC6C75EEC9FD79CCB5717"},{"Path":"CIVI-Trang17_Data/Managed/System.Runtime.CompilerServices.Unsafe.dll","Size":19256,"Sha256":"FD67ABC4C4AF10AFFBCD42FD68A2A7BC361B26714AFE6965D4D0D139459D8297"},{"Path":"CIVI-Trang17_Data/Managed/System.Runtime.Serialization.dll","Size":934400,"Sha256":"AA96283B3009B39C4ED8DFCDE146B0E0BCAFA09F9BE24A959D0C16DD6F54AE8A"},{"Path":"CIVI-Trang17_Data/Managed/System.Security.dll","Size":320000,"Sha256":"21C2CC73F1CECC8B5BFC42171A22412DFD8E04AC42978DE90C00ECB507583D62"},{"Path":"CIVI-Trang17_Data/Managed/System.ServiceModel.Internals.dll","Size":215040,"Sha256":"C90168F6F28B2355C6C11D38EE39FA18F44EA839C877CCCE8934BEE50E7FB479"},{"Path":"CIVI-Trang17_Data/Managed/System.Transactions.dll","Size":35328,"Sha256":"E5FF61CC9163810AA4C1FD9A00E929070E6733ECA6D6669B7ECE5169EC3739EF"},{"Path":"CIVI-Trang17_Data/Managed/System.Xml.dll","Size":3160064,"Sha256":"EE620F038A3A2589A118F0E7A030FE398B082015FBF83AC1474FB55B521D0BD7"},{"Path":"CIVI-Trang17_Data/Managed/System.Xml.Linq.dll","Size":136704,"Sha256":"638F8C5F44AAB1FD5755FDF2873683F3CA0F0FB0A3AAECFDBF1D2C2280C5983E"},{"Path":"CIVI-Trang17_Data/Managed/Unity.InputSystem.dll","Size":1089024,"Sha256":"D151FDD090ED48CC45CFA45A2C1B08244BA5123582462109EAA81A1D5BE6D4DB"},{"Path":"CIVI-Trang17_Data/Managed/Unity.InputSystem.ForUI.dll","Size":15360,"Sha256":"874C1696435E4DC611293632F4AE2191582B3B58256068B0FD8E3FD5327794D6"},{"Path":"CIVI-Trang17_Data/Managed/Unity.InternalAPIEngineBridge.004.dll","Size":4096,"Sha256":"D32F24F9929D7FB988F3DB379541C1F9BB014826A8AC2C3954CBAF7A26A7C059"},{"Path":"CIVI-Trang17_Data/Managed/Unity.Scripting.dll","Size":70568,"Sha256":"3A3706946F93113AEAFBED6058EB38D35C2531EC7DC1F5425C85B748B40D20E0"},{"Path":"CIVI-Trang17_Data/Managed/Unity.TextMeshPro.dll","Size":438784,"Sha256":"C938B67090BD6C47564F50E70A163C38060CB44954ABAD5271B14642CBCA057B"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.AccessibilityModule.dll","Size":49584,"Sha256":"4288D493BBF9E04A8FC28840AF2C98C51BF1A28D1CEEE4BBBAF967FA2888F2FB"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.AdaptivePerformanceModule.dll","Size":122288,"Sha256":"A25DCAEB076AC26321CFB374896ABCC888634090D775D86D48812AB42E58BDDD"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.AIModule.dll","Size":87472,"Sha256":"4F939F1D8EA83BA67324AFCEE2C7208A4EDD6CB6B585C3CD2D451EA9AD936455"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.AMDModule.dll","Size":21936,"Sha256":"E0162A0422C045C8605E00253D8207C5904FD74EEB31C0829E8F8F385FE7331A"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.AndroidJNIModule.dll","Size":121256,"Sha256":"99485C14AEFA2D23DA00252691C1AF8501F87F1EEAA5CBE7D6504706BEE7DCAC"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.AnimationModule.dll","Size":218024,"Sha256":"0302F993C0B09AE1F56583851B81016004EF53C4DB3A08735BFE9E70444C9F7B"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ARModule.dll","Size":27568,"Sha256":"C7CF9A4F750F8CC7F74E0F9CF3E1D8A8564869B1519A480D0ED963FC75F3B6C4"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.AssetBundleModule.dll","Size":42928,"Sha256":"47B3C3781358FE56E99006DAD55DC56EC465C7F8039B7D5C227FEDEB8E5827CB"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.AudioModule.dll","Size":142256,"Sha256":"9372DAD3A83E08562F28A7561EC6847167FCF6C8773E0CE61F4746BB6EC70BCC"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.BurstModule.dll","Size":337840,"Sha256":"6EFEA2B4288A2E5F7FE96F701A654FF8DA77F43E8AA6938AEF25EE20645EF397"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ClothModule.dll","Size":38320,"Sha256":"E7A4F1507A31CAC7141A3B905843E2E302E632B9978E74116795BACFBB2C3CC6"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ClusterInputModule.dll","Size":30128,"Sha256":"67943811446B8C098635F9FFBCA6698DDEA1AF13D4F4A737D56CE069ED3F26DD"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ClusterRendererModule.dll","Size":28592,"Sha256":"10BBC8E41A8AF3F39D76F1B9156094B8838697DF1416199DBF08C7FA25D19D1F"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ContentLoadModule.dll","Size":56232,"Sha256":"0BEB31AC67A588C3E55CEAE422A38F7E5964E750B5D828D0D1F92BD37E463F8D"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.CoreModule.dll","Size":2030504,"Sha256":"65F31F2076A46CEC35712898748D12FC1CD65ECDDF84DB88D5ECCB378292ECB9"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.CrashReportingModule.dll","Size":28584,"Sha256":"5FB0F91747797572EB6B9CC2056C7DF43061FF9AB3D366CD76C726AB3B7C7FB6"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.DirectorModule.dll","Size":42928,"Sha256":"257703447F26AA8D4332261591FBB1E8876D066FAC3A96294D9808A0BC5ECEEB"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.dll","Size":226224,"Sha256":"A29238E56CFC7FD59243DC0D6D02C8E55BA9687E7AE2EDFB976BF17DBAF7BE1A"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.DSPGraphModule.dll","Size":33704,"Sha256":"B2C43D1D6C5423EE7AA58DF531B5C2437ACD26F8184B5AD089F565E478F81A12"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.GameCenterModule.dll","Size":46512,"Sha256":"93DF0CAC6F6DA9C3EA1F7E2F059B1D7C415E877510537A9F88C068BED722C2E5"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.GIModule.dll","Size":26024,"Sha256":"6522B595AC749D06E987C8CA2EBC6270E11C82DF06E907C8B19B91B3BFA5AFE6"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.GraphicsStateCollectionSerializerModule.dll","Size":14256,"Sha256":"D3863C590CCFE0EE84AD8C5176FD18482792D1F45584D64DA41510DC3E5526BB"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.GridModule.dll","Size":32680,"Sha256":"1A2BDFEA96F1B57321C8C16827FD54E618253DE8AE55A667AAC6D7983D40503F"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.HierarchyCoreModule.dll","Size":113584,"Sha256":"F9660A2636395DF5B38C50B5C069076BA4ADA71149CE8AD1151E642BA46099E6"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.HierarchyModule.dll","Size":115120,"Sha256":"221689B7ABECAAA217B3DAF25C7D6CC83FB41419A0F5B0A2EEDE0507426E956F"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.HotReloadModule.dll","Size":26024,"Sha256":"0171D5192CCAEE9AB37A5F34339A8D304136FEC3E0781801837CFEBE8B8CEE4E"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.IdentifiersModule.dll","Size":16816,"Sha256":"9BD465E35EF75F9595E6ECEE9D30001DA51933FA0E3FF5C54B293383D00C8416"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ImageConversionModule.dll","Size":34216,"Sha256":"D8F1529F511848756D41737F8BCCAA4C0585A3C399EF9976ECDB6DCF2B574721"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.IMGUIModule.dll","Size":191920,"Sha256":"F0319A9316157D87AA3C085B6A3BF1141884CF3FDD07F95989E3FD79F7442ED4"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.InputForUIModule.dll","Size":56744,"Sha256":"358C70D4363E547A6EBEB589205647B2959279BD681E781A7196465501EF7A39"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.InputLegacyModule.dll","Size":42928,"Sha256":"03FDF9591B60072E58B3359DBE9B4F837D69219FBC526FA5FBB256359AFB9C52"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.InputModule.dll","Size":32688,"Sha256":"1DFB88C68D3C2A399A8CEE3BBB971ADA4E8238B2FCAAE0098C34E828F3D8DF16"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.InsightsModule.dll","Size":15792,"Sha256":"FC65F05AF151E36DB204CA1150D37C4AC8385C64CD0DBB3FDEFC73F4BB9C53BF"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.JSONSerializeModule.dll","Size":29096,"Sha256":"F45AB4F397B6E76822ACBEAC85C21A5AFCFF6D328223FEAFF55B20DCCE14F090"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.LocalizationModule.dll","Size":29096,"Sha256":"C442036FB4E48D27931214C4FF7005E9EF829BF68003C4B9415BFCA486564C6D"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ManagedKernelModule.dll","Size":14256,"Sha256":"3DD265D0E90B30B85A3E0BE70D1912F8742D997C852630B44132EDCB2E7F5C92"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.MarshallingModule.dll","Size":100784,"Sha256":"C6EE04460A8A0DFB1EE8CFB88EABCE3284A4B368B0BAA57D61AAC6318C4BD422"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.MathematicsModule.dll","Size":740776,"Sha256":"90FBB7F615FEACE08ED4E0D4B867E1FB2CF2F0282AD506A601C214E23E7C7663"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.MultiplayerModule.dll","Size":18344,"Sha256":"F8FB69A016F8F01589448CEF8024C0C1A9329D85AD3E641810DACF345E757B8C"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.NVIDIAModule.dll","Size":41384,"Sha256":"CCEED34C83031A20D2D5F3318AA726CB07F231C26B45612298190A6CAC5BDF05"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ParticleSystemModule.dll","Size":169392,"Sha256":"E405346366A594AB27D52EB75860174281ABCC9B58FF1F49252371E978DFE235"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.PerformanceReportingModule.dll","Size":27048,"Sha256":"441F88274973B15592789D82CED9711E768B7A2E6E485C2B101CA3CC0B52EBD5"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.Physics2DModule.dll","Size":222640,"Sha256":"ECC0FDD8B6077957F4CEE3E15991CD253A845AD8874919CEFD49D57DDFFD44E3"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.PhysicsBackendPhysXModule.dll","Size":14256,"Sha256":"291EF8AFF4A6C65298666936E64CD60026F8669BB36B0EF5B212FF1E564A20F4"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.PhysicsCore2DModule.dll","Size":482728,"Sha256":"D3916749AE229B80F4AE5DC2FA2AC497D90444B703AF1C0B0523EAB6DD23CB72"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.PhysicsModule.dll","Size":181680,"Sha256":"B4EFE48820D9D36BD71787B56FD4DADC69864E11C169253959164D411C78427B"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.PropertiesModule.dll","Size":124848,"Sha256":"428DD1419F4848E6ADDC8FCB6779D335EB1A733EA56FA3A1D5FFBB0834604BEB"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.RenderAs2DModule.dll","Size":27568,"Sha256":"C1D6E97A3817B550D207C9CB8F3B485B0200943C5E25DAEF678083FDE49CBE8D"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.RuntimeInitializeOnLoadManagerInitializerModule.dll","Size":26024,"Sha256":"4303986BD4E0E84EE35CF52617B20529D8933A4642FB9E287A0742B5F72A5005"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ScreenCaptureModule.dll","Size":28080,"Sha256":"DBFAC5EB3D098DB8223BE80AB706A448D5315C52A515E042F0510068D414C58E"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ScriptingModule.dll","Size":65968,"Sha256":"E61E45E836A128257159E98C1D165255149DDC0ABCD839571C697A8B6B1C11BB"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.ShaderVariantAnalyticsModule.dll","Size":26024,"Sha256":"706EE6F2D2F8CF27BD0FE26AA91BF788A80B29836827A2D6692E82980BD10A02"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.SpriteMaskModule.dll","Size":30632,"Sha256":"712DD50DCE3A0F4AEACA4827BE8A096DD3E61769F4293A54043E9A8CAE14D70F"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.SpriteShapeModule.dll","Size":34216,"Sha256":"3CDCE47AE7609630805BD5B72B4EDAE11F98B9A7E4F6C1AFEA79A6F88E977B7D"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.StreamingModule.dll","Size":28080,"Sha256":"E8A82A115BC37AE619CA5939159F3803B5BF200C1C712F6110BC4ADAFF55143F"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.SubstanceModule.dll","Size":31152,"Sha256":"5922EDF5C3968B50E641BD06BC31CE269DCDFD6210FC47E2A4537F0D0C47E30A"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.SubsystemsModule.dll","Size":42920,"Sha256":"192DA294E7430AA0B3FB25205DD940657E9E38526CE13253F1C97F306FD5ED2E"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.TerrainModule.dll","Size":132528,"Sha256":"24260B97586835A6489A2129DDE75F3D4256D5D4B36781639DE1EEAA27F33CDD"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.TerrainPhysicsModule.dll","Size":27568,"Sha256":"784060EBA0F234B0A3C300C0C7CF907787B1D63CC4CD38536075FD0789CE12CD"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.TetgenModule.dll","Size":26032,"Sha256":"198AD6A73DCD2A9E8089CA893BF00F6E912A4501D11A0B59CD666C1F38DE41DE"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.TextCoreFontEngineModule.dll","Size":86440,"Sha256":"C83E99CDB63B29B67FCE89E2440EAA80907F56DA3D436C23A4513B008C0DE483"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.TextCoreTextEngineModule.dll","Size":314792,"Sha256":"F3C43A7AA04573C8F00FF70806225E71378EB1C7E8ACA63EB96F742114CD7376"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.TextRenderingModule.dll","Size":39336,"Sha256":"48918AF4370C6D6F6A5062FA3DF10936D8FD7130EF6D22DC5B9092AD0B4A0515"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.TilemapModule.dll","Size":75688,"Sha256":"EA96F44F98E4BDF43BAA39CAD7F01CEF6677A0A557CCF9ACBD7FFD27BF30F593"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.TimelineFoundationModule.dll","Size":167336,"Sha256":"544B1EAB2D14B895D85604060A045344034FBE68F9507FE66A87192B113EF0E3"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.TLSModule.dll","Size":34224,"Sha256":"3B61D20B9971CCFAA563A449418C4139FF103A5D9985F504D1A28E1E4026C23A"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.U2DRuntimeModule.dll","Size":73648,"Sha256":"9B585A00D2A01C0DDE0047780BC356B2DE1586694DD23D0806AC6DFD7F893206"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UI.dll","Size":269312,"Sha256":"B0C8BF8CB20512D2CCA5E13BBA6DA454AE246770A939DD6DE6731937983B53F1"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UIElementsModule.dll","Size":2545064,"Sha256":"D9AF54209D4264F7696333A3FA5E65EAA10522999D7E197ECBF655E0CD01A90A"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UIModule.dll","Size":57256,"Sha256":"D5FFF86E52BDEB4B3BA44D1E3E35F095EF53A8AFA1CABB042049AD810815924E"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UmbraModule.dll","Size":26024,"Sha256":"9DA8BD35A22FD296F4ADBD122118128BCD09CAFE9329D21F1EA0796D103BEE1E"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityAnalyticsCommonModule.dll","Size":36776,"Sha256":"D3A791BFDD96EAD6F59EFC8DFD6F8FD76A54E3833A0A5B18A63C5B2DB07C0D1E"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityAnalyticsModule.dll","Size":61872,"Sha256":"1B37BC7EEC19A7E3760A6406143BE94A42A0EE21435AA77A1F82423078243414"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityConnectModule.dll","Size":29616,"Sha256":"4124E8A53D17EDCF12BB7BE7B5BAC7F5AB10CC213CC8D11BD331F03B143D2D13"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityConsentModule.dll","Size":16296,"Sha256":"82541B1999E87E5C1C93A97BB7091BF5B7DA16DC6C2B035C514574F26BAC178F"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityCurlModule.dll","Size":28080,"Sha256":"B78090C68E5DB9F3B41F90E863391821B21EE038122CE6ACEF05974CBFDAF630"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityWebRequestAssetBundleModule.dll","Size":30632,"Sha256":"FB594D87ECE1EA7854923C3FE6ADCC56E0AEFF2777277C063C1CA7F7E9EC251A"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityWebRequestAudioModule.dll","Size":30632,"Sha256":"4D05E355B3503F9DBDFE80132CC2C7BA8A79187F35800310EB5AC333EAA60154"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityWebRequestModule.dll","Size":81320,"Sha256":"AF264BC2656E3B3FF1D25741B8DCE6C70A0BDE27B858BCD4D0E6C8308F7DE839"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityWebRequestTextureModule.dll","Size":29616,"Sha256":"55A2C741287949B450F4A5E74259B9DEC6C45B80983B61C2B3840B135DD8CE14"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.UnityWebRequestWWWModule.dll","Size":35240,"Sha256":"1BED8E10C3518AD2D069B3105BE78E910271F2C5876883BC0234B4CD9E85699D"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.VectorGraphicsModule.dll","Size":231336,"Sha256":"B8920EB946E89F09A6FC0329A8C69DF54FB3665C757C25A42326681CDA5BF455"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.VehiclesModule.dll","Size":33200,"Sha256":"4CD42F969901539A6FDBA19032F334F6AF0E7007339E68644412E0822E808C9A"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.VFXModule.dll","Size":82352,"Sha256":"AE342249B5BE5B8A0A3B48723F4FF2F0D3559850C8A15E502748D7540A20F924"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.VideoModule.dll","Size":48552,"Sha256":"A33ADD02EA506E62B75A630D99FF81DC262B8B8D5FC3564DEAC6728751F51557"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.VirtualTexturingModule.dll","Size":43944,"Sha256":"FE2DA6222030123BF05C494E2F92542E831FC03374FB37947D693C72BA7F5800"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.WindModule.dll","Size":28584,"Sha256":"4D2CC53B917356115BAC69D3BC0F795DCA91A4A64981F6FEA092EA397D19562C"},{"Path":"CIVI-Trang17_Data/Managed/UnityEngine.XRModule.dll","Size":96168,"Sha256":"0E34A02864D6A9F7AB0D6478D0109333BF7F40A979293C1928C63A33B3C3FF0F"},{"Path":"CIVI-Trang17_Data/Resources/unity default resources","Size":5919584,"Sha256":"696B9AFAEE02B5D45978634F1450EF18423C19177933B997FD8EE364D70CDD61"},{"Path":"CIVI-Trang17_Data/Resources/unity_builtin_extra","Size":532308,"Sha256":"854F87449D3B5A164C74C61F4813CE179B6D6CFB610BA7179C2F8681C4404E2E"},{"Path":"MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll","Size":7834544,"Sha256":"E664912D883DF71FF2BBF347C20C47FF66B7CF8A0C4D22A90EDDC4F8DB513DED"},{"Path":"MonoBleedingEdge/EmbedRuntime/MonoPosixHelper.dll","Size":611240,"Sha256":"09D8A2BF781A9EB5C6EE2C3C733723A41ADDD709413EF49AD9ED1A06A018F6D1"},{"Path":"MonoBleedingEdge/etc/mono/browscap.ini","Size":311984,"Sha256":"4DDD50F31FB968F30BEDEFC253A46DC3F2890192D05CDAA9E0A64A056EEE807E"},{"Path":"MonoBleedingEdge/etc/mono/config","Size":3815,"Sha256":"5776169973A26A387B8B3E5C0F2301A7AB9A6DD7C7D3EFA22A96ABC47FBF8662"},{"Path":"MonoBleedingEdge/etc/mono/2.0/DefaultWsdlHelpGenerator.aspx","Size":60575,"Sha256":"751861040B69EA63A3827507B7C8DA9C7F549DC181C1C8AF4B7CA78CC97D710A"},{"Path":"MonoBleedingEdge/etc/mono/2.0/machine.config","Size":29066,"Sha256":"E91782A27FA39FC6C1D6EE8B08529F5D35052310D0006034B878EB04B8F2AF30"},{"Path":"MonoBleedingEdge/etc/mono/2.0/settings.map","Size":2622,"Sha256":"49C6160F9D54AF4270A3B4E997FC4A8301F79B9E2070118FA46DDBCBBC44F9A2"},{"Path":"MonoBleedingEdge/etc/mono/2.0/web.config","Size":11635,"Sha256":"CF2B54339E069B9E42F7D6795EECBF562C50C1011F4CA8F65F578FE52D97FF92"},{"Path":"MonoBleedingEdge/etc/mono/2.0/Browsers/Compat.browser","Size":1605,"Sha256":"8A1082057AC5681DCD4E9C227ED7FB8EB42AC1618963B5DE3B65739DD77E2741"},{"Path":"MonoBleedingEdge/etc/mono/4.0/DefaultWsdlHelpGenerator.aspx","Size":60575,"Sha256":"751861040B69EA63A3827507B7C8DA9C7F549DC181C1C8AF4B7CA78CC97D710A"},{"Path":"MonoBleedingEdge/etc/mono/4.0/machine.config","Size":33598,"Sha256":"E60AEC2C5115D65B3ACB3C55EA21576DBD770F579166C017125571E46AE560ED"},{"Path":"MonoBleedingEdge/etc/mono/4.0/settings.map","Size":2622,"Sha256":"CE1DB1AD8A9512073164E3ECCDC193F7EDA036E1A9733CAEC4635DE21B2865C8"},{"Path":"MonoBleedingEdge/etc/mono/4.0/web.config","Size":18802,"Sha256":"E38BB8CC68FE5B4EDECDFD288D094B9E8CED7629039B2A347682ABA0D8BD7492"},{"Path":"MonoBleedingEdge/etc/mono/4.0/Browsers/Compat.browser","Size":1605,"Sha256":"8A1082057AC5681DCD4E9C227ED7FB8EB42AC1618963B5DE3B65739DD77E2741"},{"Path":"MonoBleedingEdge/etc/mono/4.5/DefaultWsdlHelpGenerator.aspx","Size":60575,"Sha256":"751861040B69EA63A3827507B7C8DA9C7F549DC181C1C8AF4B7CA78CC97D710A"},{"Path":"MonoBleedingEdge/etc/mono/4.5/machine.config","Size":34056,"Sha256":"EE950004B576FB28DC85F4B0435ED04BF96612DE2E8B53BE84D07AFE85A0DE6C"},{"Path":"MonoBleedingEdge/etc/mono/4.5/settings.map","Size":2622,"Sha256":"CE1DB1AD8A9512073164E3ECCDC193F7EDA036E1A9733CAEC4635DE21B2865C8"},{"Path":"MonoBleedingEdge/etc/mono/4.5/web.config","Size":18811,"Sha256":"C4CBDDD4FD9347B58CC5A72B36DC4BA1AD2BB699E65869D05CD3FB9865F0D824"},{"Path":"MonoBleedingEdge/etc/mono/4.5/Browsers/Compat.browser","Size":1605,"Sha256":"8A1082057AC5681DCD4E9C227ED7FB8EB42AC1618963B5DE3B65739DD77E2741"},{"Path":"MonoBleedingEdge/etc/mono/mconfig/config.xml","Size":25817,"Sha256":"0C56E34C69124510FA8C19E7B4C2CA6C1C4FF460AE19F798DD0CA035809E396D"},{"Path":"D3D12/D3D12Core.dll","Size":4725304,"Sha256":"625BD40005B1C3708695D5256ECF230D278E6B8EDBA5A88AF70F3D91324F3052"},{"Path":"Licenses/FONT-NOTICE.txt","Size":229,"Sha256":"B791E8404EBE5FF88509148CE1398927354F64AC3F1320E084488246940E0AFB"},{"Path":"Licenses/LiberationSans-OFL.txt","Size":4469,"Sha256":"37F8552E9A874EC10710DC0EDE6A9ADF168E6609FBD02A507F35629373B85A48"},{"Path":"Licenses/lora-OFL.txt","Size":4423,"Sha256":"1D9A970809AC804B582A6CE7F0EBC4E7FEFCBFD7FF6299CAD35EE656A21BE716"},{"Path":"Licenses/patrickhand-OFL.txt","Size":4376,"Sha256":"377F4F9C19E935228552478EB68CC2ED82910988A60BA60E2AC73B09F32D02D1"},{"Path":"BAT-DAU.txt","Size":992,"Sha256":"C04F5566DBB1C8F827C5E4DD9EDF28E48317B1BE9D04DD517E976A74413B505F"}]}
'@ | ConvertFrom-Json
Invoke-CiviLauncher -Config $civiConfig -LauncherPath $env:CIVI_LAUNCHER_SELF | Out-Null
