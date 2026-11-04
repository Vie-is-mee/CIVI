@echo off
setlocal
cd /d "%~dp0release\Chapter1"
if not exist "CIVI_Chapter1_x86.exe" (
  echo [CIVI] Khong tim thay ban test Chapter 1 trong release\Chapter1.
  pause
  exit /b 1
)
start "CIVI - Chapter 1" "CIVI_Chapter1_x86.exe" -screen-width 1280 -screen-height 720 -screen-fullscreen 0
endlocal
