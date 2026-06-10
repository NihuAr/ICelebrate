@echo off
chcp 65001 >nul
echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║     Kursachzhoska - Build and Package Tool                ║
echo ╚════════════════════════════════════════════════════════════╝
echo.
echo Выберите действие:
echo.
echo [1] Полная сборка (Публикация + Установщик + Портативная версия)
echo [2] Только публикация приложения
echo [3] Только портативная версия (ZIP)
echo [4] Только установщик (требуется Inno Setup)
echo [0] Выход
echo.
set /p choice="Введите номер: "

if "%choice%"=="1" goto full
if "%choice%"=="2" goto publish_only
if "%choice%"=="3" goto portable_only
if "%choice%"=="4" goto installer_only
if "%choice%"=="0" goto end
goto invalid

:full
echo.
echo ════════════════════════════════════════════════════════════
echo ШАГИ: Публикация → Портативная версия → Установщик
echo ════════════════════════════════════════════════════════════
echo.
call publish.bat
if %ERRORLEVEL% NEQ 0 goto error
call create-portable.bat
if %ERRORLEVEL% NEQ 0 goto error
call setup-builder.bat
goto success

:publish_only
echo.
echo ════════════════════════════════════════════════════════════
echo Публикация приложения...
echo ════════════════════════════════════════════════════════════
echo.
call publish.bat
goto check_result

:portable_only
echo.
echo ════════════════════════════════════════════════════════════
echo Создание портативной версии...
echo ════════════════════════════════════════════════════════════
echo.
if not exist "publish\Kursachzhoska.exe" (
    echo Сначала необходима публикация!
    call publish.bat
    if %ERRORLEVEL% NEQ 0 goto error
)
call create-portable.bat
goto check_result

:installer_only
echo.
echo ════════════════════════════════════════════════════════════
echo Создание установщика...
echo ════════════════════════════════════════════════════════════
echo.
if not exist "publish\Kursachzhoska.exe" (
    echo Сначала необходима публикация!
    call publish.bat
    if %ERRORLEVEL% NEQ 0 goto error
)
call setup-builder.bat
goto check_result

:check_result
if %ERRORLEVEL% EQU 0 goto success
goto error

:success
echo.
echo ════════════════════════════════════════════════════════════
echo ✓ ВСЕ ГОТОВО!
echo ════════════════════════════════════════════════════════════
echo.
echo Созданные файлы:
if exist "publish\" echo   • publish\ - опубликованное приложение
if exist "Kursachzhoska-Portable.zip" echo   • Kursachzhoska-Portable.zip - портативная версия
if exist "installer-output\Kursachzhoska-Setup.exe" echo   • installer-output\Kursachzhoska-Setup.exe - установщик
echo.
echo Вы можете делиться любым из этих файлов!
echo.
pause
exit /b 0

:error
echo.
echo ════════════════════════════════════════════════════════════
echo ✗ ОШИБКА!
echo ════════════════════════════════════════════════════════════
echo.
echo Проверьте вывод выше для деталей
echo.
pause
exit /b 1

:invalid
echo.
echo Неверный выбор! Попробуйте снова.
timeout /t 2 >nul
cls
goto :eof

:end
echo До свидания!
exit /b 0

