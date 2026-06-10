@echo off
echo ========================================
echo Publishing Kursachzhoska Application
echo ========================================
echo.

REM Очистка предыдущей публикации
if exist "publish" rmdir /s /q "publish"

echo Publishing application...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -o publish

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Publish completed successfully!
    echo Output directory: publish\
    echo ========================================
    echo.
    echo Files are ready in the 'publish' folder
    echo You can now run setup-builder.bat to create installer
) else (
    echo.
    echo ========================================
    echo Publish failed!
    echo ========================================
    pause
    exit /b 1
)

pause

