@echo off
setlocal
cd /d "%~dp0release\NewStoryCampaign"
if not exist "CIVI_NewStory_Test_x86.exe" (
  echo [CIVI] Khong tim thay ban test moi.
  pause
  exit /b 1
)
start "" "CIVI_NewStory_Test_x86.exe" -screen-width 1280 -screen-height 720 -screen-fullscreen 0
endlocal
