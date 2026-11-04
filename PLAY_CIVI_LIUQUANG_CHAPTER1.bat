@echo off
setlocal
cd /d "%~dp0"
if not exist "%~dp0release\LiuQuangChapter1\CIVI_LiuQuang_C1.exe" (
  echo Khong tim thay ban game. Xem CHAPTER1_LIUQUANG.md.
  pause
  exit /b 1
)
start "" /D "%~dp0release\LiuQuangChapter1" "%~dp0release\LiuQuangChapter1\CIVI_LiuQuang_C1.exe" -screen-fullscreen 0
endlocal
