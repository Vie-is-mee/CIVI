@echo off
setlocal
chcp 65001 >nul
set "CIVI_S00=%~dp0release\S00-TV3-Windows\CIVI-S00.exe"
if not exist "%CIVI_S00%" (
  echo Chưa có bản chạy S00. Nhận cả thư mục release\S00-TV3-Windows từ người bàn giao.
  echo Người chơi không cần cài Unity.
  pause
  exit /b 1
)
start "" "%CIVI_S00%"
exit /b 0
