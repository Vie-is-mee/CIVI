@echo off
setlocal
cd /d "%~dp0release\SchoolWorld"
if not exist "CIVI_SchoolWorld_x86.exe" (
  echo [CIVI] Khong tim thay ban School World. Kiem tra thu muc release.
  pause
  exit /b 1
)
start "CIVI - School World" "CIVI_SchoolWorld_x86.exe" -screen-width 1280 -screen-height 720 -screen-fullscreen 0
endlocal
