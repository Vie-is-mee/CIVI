@echo off
chcp 65001 >nul
setlocal
title CIVI
set "ROOT=%~dp0"
set "GAME=%ROOT%release\CIVI\CIVI.exe"

if not exist "%GAME%" (
    echo Chua co ban build. Dang build bang Unity, lan dau mat vai phut...
    powershell -NoProfile -ExecutionPolicy Bypass -File "%ROOT%tools\Build-CIVI.ps1"
    if not exist "%GAME%" (
        echo Khong build duoc. Xem huong dan trong README.md.
        pause
        exit /b 1
    )
)

rem May hai card (Intel + NVIDIA/AMD): xin Windows chay game tren card manh. Chi anh huong rieng game nay, khong can quyen admin.
reg add "HKCU\Software\Microsoft\DirectX\UserGpuPreferences" /v "%GAME%" /t REG_SZ /d "GpuPreference=2;" /f >nul 2>&1

rem Dem card that, bo qua card ao va driver co ban cua Windows.
set GPUS=0
for /f %%g in ('powershell -NoProfile -Command "@(Get-CimInstance Win32_VideoController | Where-Object { $_.Name -notmatch 'Basic|Remote|Virtual|Hyper-V|Parsec' }).Count"') do set GPUS=%%g

if "%GPUS%"=="0" goto cpu

"%GAME%"
if not errorlevel 1 exit /b 0
echo Game dung bat thuong tren GPU, thu lai bang CPU...

:cpu
rem Khong co GPU dung duoc: Windows ve bang CPU qua trinh ket xuat phan mem, game tu ha hinh anh de van choi duoc.
"%GAME%" -force-d3d11 -force-device-index %GPUS% -civi-cpu
exit /b %errorlevel%
