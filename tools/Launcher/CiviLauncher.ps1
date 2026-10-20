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
