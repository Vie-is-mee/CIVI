@echo off
setlocal
cd /d "%~dp0release\SchoolWorldV2"
if not exist "CIVI_SchoolWorld_V2_x86.exe" (
  echo [CIVI] Khong tim thay ban Windows School Visual V2.
  pause
  exit /b 1
)
start "CIVI School Visual V2" "CIVI_SchoolWorld_V2_x86.exe" -screen-width 1280 -screen-height 720 -screen-fullscreen 0
endlocal
