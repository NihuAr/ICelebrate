# iCelebrate Desktop Publisher (Simplified)
# Простой скрипт для публикации WPF приложения

param(
    [string]$Version = "1.0.0",
    [string]$OutputDir = ".\releases"
)

Write-Host "=== iCelebrate Desktop Publisher ===" -ForegroundColor Cyan
Write-Host "Версия: $Version" -ForegroundColor Yellow

# Проверка .NET SDK
$dotnetVersion = dotnet --version
Write-Host "Обнаружена версия .NET: $dotnetVersion" -ForegroundColor Green

# Очистка старых публикаций
Write-Host "Очистка старых файлов..." -ForegroundColor Yellow
if (Test-Path "Kursachzhoska\bin\Release") {
    Remove-Item "Kursachzhoska\bin\Release" -Recurse -Force
}

# Публикация
Write-Host "Публикация приложения..." -ForegroundColor Yellow
cd Kursachzhoska
dotnet publish -c Release -o "..\publish-temp" --self-contained -r win-x64

if ($LASTEXITCODE -ne 0) {
    Write-Host "Ошибка при публикации!" -ForegroundColor Red
    exit 1
}

cd ..

# Создание папки для релизов
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

# Архивирование с помощью встроенного Windows компрессора
$zipName = "$OutputDir\iCelebrate-v$Version-portable.zip"
Write-Host "Создание архива: $zipName" -ForegroundColor Yellow

if (Test-Path $zipName) {
    Remove-Item $zipName
}

# Используем встроенный Windows компрессор (более надежный)
# Важно: архивируем содержимое папки, а не саму папку
Push-Location "publish-temp"
Compress-Archive -Path "*" -DestinationPath "..\$zipName" -CompressionLevel Optimal -Force
Pop-Location

# Очистка временной папки
Remove-Item "publish-temp" -Recurse -Force

# Копирование в frontend/public для скачивания с лэндинга
Write-Host "Копирование установщика в frontend/public..." -ForegroundColor Yellow
if (-not (Test-Path "frontend\public")) {
    New-Item -ItemType Directory -Path "frontend\public" | Out-Null
}
Copy-Item $zipName "frontend\public\iCelebrate-portable.zip" -Force
Write-Host "✓ Установщик скопирован в frontend/public" -ForegroundColor Green

Write-Host "✓ Успешно опубликовано!" -ForegroundColor Green
Write-Host "Файл: $zipName" -ForegroundColor Green
Write-Host "Размер: $([math]::Round((Get-Item $zipName).Length / 1MB, 2))MB" -ForegroundColor Green

Write-Host ""
Write-Host "=== Инструкция для распространения ===" -ForegroundColor Cyan
Write-Host "1. Загрузите $zipName на GitHub Releases / Google Drive"
Write-Host "2. Поделитесь ссылкой для скачивания"
Write-Host "3. Пользователи распаковывают и запускают iCelebrate.exe"
