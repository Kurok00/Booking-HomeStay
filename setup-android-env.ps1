# Setup Android Environment Variables cho Windows
Write-Host "=== Setup Android Environment ===" -ForegroundColor Cyan

$androidSdk = "C:\Users\AnVnt\AppData\Local\Android\Sdk"
$emulatorPath = "$androidSdk\emulator"
$platformTools = "$androidSdk\platform-tools"
$cmdlineTools = "$androidSdk\cmdline-tools\latest\bin"

# Kiểm tra SDK tồn tại
if (!(Test-Path $androidSdk)) {
    Write-Host "❌ Android SDK không tìm thấy tại: $androidSdk" -ForegroundColor Red
    Write-Host "   Vui lòng cài Android Studio hoặc Android SDK" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Android SDK found: $androidSdk" -ForegroundColor Green

# Set ANDROID_HOME cho session hiện tại
$env:ANDROID_HOME = $androidSdk
Write-Host "✅ ANDROID_HOME = $androidSdk" -ForegroundColor Green

# Add to PATH cho session hiện tại
$env:PATH = "$emulatorPath;$platformTools;$cmdlineTools;$env:PATH"
Write-Host "Added to PATH: emulator, adb, sdkmanager" -ForegroundColor Green

# Check emulator
Write-Host "`n=== Available Emulators ===" -ForegroundColor Cyan
& "$emulatorPath\emulator.exe" -list-avds

# Check ADB
Write-Host "`n=== ADB Devices ===" -ForegroundColor Cyan
& "$platformTools\adb.exe" devices

Write-Host "`n=== Setup Complete! ===" -ForegroundColor Green
Write-Host "Environment variables set for this PowerShell session." -ForegroundColor Yellow
Write-Host "`nTo set permanently, run:" -ForegroundColor Cyan
Write-Host '  [Environment]::SetEnvironmentVariable("ANDROID_HOME", "' + $androidSdk + '", "User")' -ForegroundColor White

Write-Host "`n=== Quick Commands ===" -ForegroundColor Cyan
Write-Host "Start emulator:" -ForegroundColor Yellow
Write-Host '  emulator -avd Medium_Phone_API_36.0' -ForegroundColor White
Write-Host "`nList devices:" -ForegroundColor Yellow
Write-Host '  adb devices' -ForegroundColor White
Write-Host "`nKill ADB server:" -ForegroundColor Yellow
Write-Host '  adb kill-server; adb start-server' -ForegroundColor White
