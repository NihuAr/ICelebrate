# iCelebrate Desktop Publisher
# Скрипт для публикации WPF приложения в портативный формат

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

# Архивирование
$zipName = "$OutputDir\iCelebrate-v$Version-portable.zip"
Write-Host "Создание архива: $zipName" -ForegroundColor Yellow

if (Test-Path $zipName) {
    Remove-Item $zipName
}

# Используем 7-Zip или встроенный компрессор
try {
    # Пытаемся использовать 7-Zip если установлен
    $7zipPath = "C:\Program Files\7-Zip\7z.exe"
    if (Test-Path $7zipPath) {
        & $7zipPath a -tzip $zipName "publish-temp\*" | Out-Null
        Write-Host "Архив создан с помощью 7-Zip" -ForegroundColor Green
    } else {
        throw "7-Zip not found"
    }
} catch {
    # Fallback на встроенный компрессор
    Write-Host "Используется встроенный компрессор..." -ForegroundColor Yellow
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory((Resolve-Path "publish-temp").Path, (Resolve-Path $OutputDir).Path + "\iCelebrate-v$Version-portable.zip", [System.IO.Compression.CompressionLevel]::Optimal, $false)
}

# Очистка временной папки
Remove-Item "publish-temp" -Recurse -Force

Write-Host "✓ Успешно опубликовано!" -ForegroundColor Green
Write-Host "Файл: $zipName" -ForegroundColor Green
Write-Host "Размер: $((Get-Item $zipName).Length / 1MB)MB" -ForegroundColor Green

Write-Host ""
Write-Host "=== Инструкция для распространения ===" -ForegroundColor Cyan
Write-Host "1. Загрузите $zipName на GitHub Releases / Google Drive"
Write-Host "2. Поделитесь ссылкой для скачивания"
Write-Host "3. Пользователи распаковывают и запускают iCelebrate.exe"
