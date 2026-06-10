@echo off
echo ========================================
echo Creating Portable Version
echo ========================================
echo.

REM Проверка наличия опубликованных файлов
if not exist "publish\Kursachzhoska.exe" (
    echo Published files not found!
    echo Please run publish.bat first
    pause
    exit /b 1
)

REM Создание портативной версии
set OUTPUT_DIR=Kursachzhoska-Portable
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

echo Copying files...
xcopy /E /I /Y "publish\*" "%OUTPUT_DIR%\"

REM Создание README
echo Kursachzhoska - Event Management System > "%OUTPUT_DIR%\README.txt"
echo. >> "%OUTPUT_DIR%\README.txt"
echo To run the application, double-click on Kursachzhoska.exe >> "%OUTPUT_DIR%\README.txt"
echo. >> "%OUTPUT_DIR%\README.txt"
echo This is a portable version - no installation required. >> "%OUTPUT_DIR%\README.txt"
echo You can copy this folder to any location or USB drive. >> "%OUTPUT_DIR%\README.txt"

REM Создание ZIP архива (если доступен PowerShell)
echo Creating ZIP archive...
powershell -command "Compress-Archive -Path '%OUTPUT_DIR%' -DestinationPath 'Kursachzhoska-Portable.zip' -Force"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Portable version created successfully!
    echo ========================================
    echo.
    echo Folder: %OUTPUT_DIR%\
    echo ZIP Archive: Kursachzhoska-Portable.zip
    echo.
    echo You can share the ZIP file or the folder!
    start explorer .
) else (
    echo.
    echo ZIP creation failed, but folder is ready: %OUTPUT_DIR%\
    start explorer "%OUTPUT_DIR%"
)

pause

