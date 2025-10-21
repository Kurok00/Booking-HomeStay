# ChillNest Development Setup Script
# Chạy script này để start backend và setup cho Android development

Write-Host "🚀 ChillNest Development Setup" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# 1. Lấy IP address của máy
Write-Host "📡 Đang lấy IP address của máy..." -ForegroundColor Yellow
$ipAddress = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.PrefixOrigin -eq 'Dhcp' -or $_.PrefixOrigin -eq 'Manual' } | Select-Object -First 1).IPAddress

if ($ipAddress) {
    Write-Host "✅ IP Address: $ipAddress" -ForegroundColor Green
} else {
    Write-Host "⚠️  Không tìm thấy IP, sử dụng localhost" -ForegroundColor Yellow
    $ipAddress = "localhost"
}

Write-Host ""

# 2. Hiển thị URL cho Android
Write-Host "📱 URL cho Android App:" -ForegroundColor Yellow
Write-Host "  • Emulator: http://10.0.2.2:5182/api" -ForegroundColor Cyan
Write-Host "  • Device thật (cùng WiFi): http://${ipAddress}:5182/api" -ForegroundColor Cyan
Write-Host ""

# 3. Kiểm tra Firewall
Write-Host "🔥 Checking Firewall..." -ForegroundColor Yellow
$firewallRule = Get-NetFirewallRule -DisplayName "ASP.NET Core ChillNest Dev" -ErrorAction SilentlyContinue

if (-not $firewallRule) {
    Write-Host "⚠️  Firewall rule chưa có. Đang tạo..." -ForegroundColor Yellow
    Write-Host "   (Có thể yêu cầu quyền Administrator)" -ForegroundColor Gray
    
    try {
        New-NetFirewallRule -DisplayName "ASP.NET Core ChillNest Dev" `
                           -Direction Inbound `
                           -LocalPort 5182 `
                           -Protocol TCP `
                           -Action Allow `
                           -ErrorAction Stop | Out-Null
        Write-Host "✅ Firewall rule đã được tạo" -ForegroundColor Green
    } catch {
        Write-Host "❌ Không thể tạo firewall rule. Chạy lại script as Administrator!" -ForegroundColor Red
        Write-Host "   Hoặc tự thêm rule: Port 5182 TCP Inbound" -ForegroundColor Yellow
    }
} else {
    Write-Host "✅ Firewall rule đã có sẵn" -ForegroundColor Green
}

Write-Host ""

# 4. Check Android devices
Write-Host "📱 Checking Android Devices..." -ForegroundColor Yellow
$adbPath = where.exe adb 2>$null

if ($adbPath) {
    $devices = adb devices | Select-String "device$"
    if ($devices) {
        Write-Host "✅ Android devices đã kết nối:" -ForegroundColor Green
        adb devices
    } else {
        Write-Host "⚠️  Không có device nào kết nối" -ForegroundColor Yellow
        Write-Host "   - Cắm điện thoại và bật USB Debugging" -ForegroundColor Gray
        Write-Host "   - Hoặc start Android Emulator" -ForegroundColor Gray
    }
} else {
    Write-Host "⚠️  ADB không tìm thấy. Cài Android SDK!" -ForegroundColor Yellow
}

Write-Host ""

# 5. Start Backend
Write-Host "🎯 Bạn muốn làm gì?" -ForegroundColor Cyan
Write-Host "  1. Start Backend với Hot Reload" -ForegroundColor White
Write-Host "  2. Start Backend + Mở browser" -ForegroundColor White
Write-Host "  3. Chỉ hiển thị thông tin (không start)" -ForegroundColor White
Write-Host "  4. Copy API URL vào clipboard" -ForegroundColor White
Write-Host ""

$choice = Read-Host "Chọn (1-4)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "🚀 Starting Backend với Hot Reload..." -ForegroundColor Green
        Write-Host "   Backend sẽ tự động reload khi bạn sửa code!" -ForegroundColor Gray
        Write-Host "   Nhấn Ctrl+C để stop" -ForegroundColor Gray
        Write-Host ""
        Set-Location "D:\ChillNestV2\Booking-HomeStay\BoookingHotels"
        dotnet watch run --urls "http://0.0.0.0:5182"
    }
    "2" {
        Write-Host ""
        Write-Host "🚀 Starting Backend và mở browser..." -ForegroundColor Green
        Set-Location "D:\ChillNestV2\Booking-HomeStay\BoookingHotels"
        Start-Process "http://localhost:5182"
        dotnet run --urls "http://0.0.0.0:5182"
    }
    "3" {
        Write-Host ""
        Write-Host "ℹ️  Thông tin đã hiển thị ở trên!" -ForegroundColor Blue
        Write-Host ""
        Write-Host "Để start backend, chạy:" -ForegroundColor Yellow
        Write-Host "  cd D:\ChillNestV2\Booking-HomeStay\BoookingHotels" -ForegroundColor Cyan
        Write-Host "  dotnet watch run --urls 'http://0.0.0.0:5182'" -ForegroundColor Cyan
    }
    "4" {
        $emulatorUrl = "http://10.0.2.2:5182/api"
        $deviceUrl = "http://${ipAddress}:5182/api"
        
        Write-Host ""
        Write-Host "📋 Chọn URL để copy:" -ForegroundColor Cyan
        Write-Host "  1. Emulator URL: $emulatorUrl" -ForegroundColor White
        Write-Host "  2. Device URL: $deviceUrl" -ForegroundColor White
        
        $urlChoice = Read-Host "Chọn (1-2)"
        
        if ($urlChoice -eq "1") {
            Set-Clipboard $emulatorUrl
            Write-Host "✅ Đã copy URL cho Emulator!" -ForegroundColor Green
        } elseif ($urlChoice -eq "2") {
            Set-Clipboard $deviceUrl
            Write-Host "✅ Đã copy URL cho Device!" -ForegroundColor Green
        }
        
        Write-Host ""
        Write-Host "Paste vào ApiService.cs:" -ForegroundColor Yellow
        Write-Host "  private const string BaseUrl = `"<URL_VỪA_COPY>`";" -ForegroundColor Cyan
    }
    default {
        Write-Host ""
        Write-Host "❌ Lựa chọn không hợp lệ!" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "📚 Xem thêm hướng dẫn:" -ForegroundColor Yellow
Write-Host "  • HOT_RELOAD_GUIDE.md - Hot Reload setup" -ForegroundColor Cyan
Write-Host "  • API_DOCUMENTATION.md - API docs" -ForegroundColor Cyan
Write-Host "  • MAUI_APP_GUIDE.md - MAUI app guide" -ForegroundColor Cyan
Write-Host ""
