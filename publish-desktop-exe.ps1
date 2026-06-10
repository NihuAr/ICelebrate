# iCelebrate Desktop Publisher (single-file exe)
# Публикует WPF-приложение в самодостаточный .exe без архива

param(
    [string]$Version = "1.0.0",
    [string]$OutputDir = ".\releases"
)

Write-Host "=== iCelebrate Desktop Publisher (EXE) ===" -ForegroundColor Cyan
Write-Host "Версия: $Version" -ForegroundColor Yellow

# Проверка .NET SDK
$dotnetVersion = dotnet --version
Write-Host "Обнаружена версия .NET: $dotnetVersion" -ForegroundColor Green

# Очистка старых публикаций
Write-Host "Очистка старых файлов..." -ForegroundColor Yellow
if (Test-Path "Kursachzhoska\bin\Release") {
    Remove-Item "Kursachzhoska\bin\Release" -Recurse -Force
}
if (Test-Path "publish-temp-exe") {
    Remove-Item "publish-temp-exe" -Recurse -Force
}

# Публикация single-file exe
Write-Host "Публикация приложения..." -ForegroundColor Yellow
Push-Location Kursachzhoska
# Настройки в csproj уже включают PublishSingleFile и SelfContained
# Добавляем параметры для надежности
$publishArgs = @(
    'publish',
    '-c', 'Release',
    '-r', 'win-x64',
    '--self-contained',
    '/p:PublishSingleFile=true',
    '/p:IncludeAllContentForSelfExtract=true',
    '/p:PublishTrimmed=false',
    '/p:EnableCompressionInSingleFile=true',
    '-o', '..\publish-temp-exe'
)
& dotnet $publishArgs
$publishExit = $LASTEXITCODE
Pop-Location

if ($publishExit -ne 0) {
    Write-Host "Ошибка при публикации!" -ForegroundColor Red
    exit 1
}

# Создание папки для релизов
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

# Поиск exe
$exePath = Get-ChildItem "publish-temp-exe" -Filter "*.exe" | Select-Object -First 1
if (-not $exePath) {
    Write-Host "Не найден exe в publish-temp-exe" -ForegroundColor Red
    exit 1
}

$destExe = Join-Path $OutputDir "iCelebrate-v$Version.exe"
Copy-Item $exePath.FullName $destExe -Force

# Дополнительно копируем в frontend/public для скачивания с сайта
$publicDir = "frontend\public"
if (-not (Test-Path $publicDir)) {
    New-Item -ItemType Directory -Path $publicDir | Out-Null
}
Copy-Item $destExe (Join-Path $publicDir "iCelebrate.exe") -Force

# Очистка временной папки
Remove-Item "publish-temp-exe" -Recurse -Force

Write-Host "✓ Успешно собрано!" -ForegroundColor Green
Write-Host "Installer: $destExe" -ForegroundColor Green
Write-Host "Размер: $([math]::Round((Get-Item $destExe).Length / 1MB,2)) MB" -ForegroundColor Green

Write-Host "" 
Write-Host "=== Инструкция для распространения ===" -ForegroundColor Cyan
Write-Host "1. Отправьте файл $destExe пользователям" 
Write-Host "2. На сайте можно отдать frontend/public/iCelebrate.exe" 
Write-Host "3. Пользователь скачивает и запускает iCelebrate.exe (установка не требуется)" 
