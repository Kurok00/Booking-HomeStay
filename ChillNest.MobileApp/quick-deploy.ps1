# Quick Deploy Script for MAUI Android
# Usage: .\quick-deploy.ps1

$ErrorActionPreference = "Stop"
$projectPath = "D:\ChillNestV2\Booking-HomeStay\ChillNest.MobileApp"
$apkPath = "$projectPath\bin\Debug\net9.0-android\com.companyname.chillnest.mobileapp-Signed.apk"
$packageName = "com.companyname.chillnest.mobileapp"
$activityName = "crc643486d2d6326be3c7.MainActivity"
$adbPath = "C:\Users\AnVnt\AppData\Local\Android\Sdk\platform-tools\adb.exe"

Write-Host "🔨 Building MAUI app..." -ForegroundColor Cyan
cd $projectPath
dotnet build -f net9.0-android

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build succeeded!" -ForegroundColor Green

Write-Host "📦 Deploying to emulator..." -ForegroundColor Cyan
& $adbPath install -r $apkPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Deploy failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Deploy succeeded!" -ForegroundColor Green

Write-Host "🔄 Restarting app..." -ForegroundColor Cyan
& $adbPath shell am force-stop $packageName
Start-Sleep -Milliseconds 500
& $adbPath shell am start -n "$packageName/$activityName"

Write-Host "✅ App restarted! Check your emulator." -ForegroundColor Green
Write-Host "⏱️  Total time: $((Get-Date) - $startTime)" -ForegroundColor Yellow
