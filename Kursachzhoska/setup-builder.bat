@echo off
echo ========================================
echo Building Kursachzhoska Installer
echo ========================================
echo.

REM Проверка наличия Inno Setup
set "INNO_SETUP=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not exist "%INNO_SETUP%" (
    echo Inno Setup not found at: %INNO_SETUP%
    echo.
    echo Please install Inno Setup 6 from: https://jrsoftware.org/isinfo.php
    echo Or update INNO_SETUP path in this script
    echo.
    echo Alternative: You can share the 'publish' folder directly as a portable app
    pause
    exit /b 1
)

REM Проверка наличия опубликованных файлов
if not exist "publish\Kursachzhoska.exe" (
    echo Published files not found!
    echo Please run publish.bat first
    pause
    exit /b 1
)

echo Building installer with Inno Setup...
"%INNO_SETUP%" "installer-script.iss"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Installer created successfully!
    echo ========================================
    echo.
    echo Installer location: installer-output\Kursachzhoska-Setup.exe
    echo.
    echo You can now share this installer file!
    start explorer "installer-output"
) else (
    echo.
    echo ========================================
    echo Installer build failed!
    echo ========================================
)

pause

