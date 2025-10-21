# Hướng dẫn Hot Reload trên Android cho ChillNest

## 🔥 Phương pháp 1: .NET MAUI Hot Reload (Khuyên dùng nhất)

### Bước 1: Kết nối thiết bị Android

**Option A: Dùng thiết bị Android thật:**
```powershell
# Bật USB Debugging trên điện thoại:
# Settings → About Phone → Tap Build Number 7 lần → Developer Options → USB Debugging ON

# Cắm điện thoại vào máy tính
# Kiểm tra kết nối:
adb devices
```

**Option B: Dùng Android Emulator:**
```powershell
# Mở Android Studio → AVD Manager → Start emulator
# Hoặc từ command line:
emulator -avd Pixel_5_API_33
```

### Bước 2: Chạy app với Hot Reload

```powershell
cd d:\ChillNestV2\Booking-HomeStay\ChillNest.MobileApp

# Deploy và chạy với Hot Reload
dotnet watch run --framework net8.0-android

# Hoặc với device cụ thể:
dotnet watch run --framework net8.0-android --device <device-id>
```

### Bước 3: Sửa code và xem thay đổi ngay!

- **Sửa XAML**: Thay đổi hiển thị ngay lập tức (XAML Hot Reload)
- **Sửa C# code**: Hot Reload sẽ update (hầu hết trường hợp)
- **Thêm method mới**: Cần rebuild (Ctrl+Shift+B)

### Trong Visual Studio 2022:

1. Mở solution
2. Chọn device Android (emulator hoặc thiết bị thật)
3. Nhấn F5 để Debug
4. Sửa code → Lưu → Thấy ngay trên thiết bị! ⚡

**Hot Reload toolbar:**
- 🔥 Hot Reload: `Alt+F10`
- 🔄 Rebuild: `Ctrl+Shift+B`

---

## 🌐 Phương pháp 2: Backend API + Expose qua Network

Để mobile app trên Android device/emulator call được API backend trên máy tính:

### Setup Backend cho Android:

**Bước 1: Cấu hình Backend listen trên network**

Mở `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://0.0.0.0:5182",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Bước 2: Allow Firewall**

```powershell
# Chạy PowerShell as Administrator
New-NetFirewallRule -DisplayName "ASP.NET Core Dev" -Direction Inbound -LocalPort 5182 -Protocol TCP -Action Allow
```

**Bước 3: Lấy IP máy tính**

```powershell
# Windows
ipconfig
# Tìm IPv4 Address, ví dụ: 192.168.1.100
```

**Bước 4: Update API URL trong MAUI app**

Trong `Services/ApiService.cs`:

```csharp
// Android Emulator
private const string BaseUrl = "http://10.0.2.2:5182/api";

// Android Device thật (cùng WiFi)
private const string BaseUrl = "http://192.168.1.100:5182/api";
```

**IP đặc biệt cho Android:**
- `10.0.2.2` = localhost của máy tính (khi dùng emulator)
- `192.168.x.x` = IP thật của máy (khi dùng device thật cùng WiFi)

---

## 🚀 Phương pháp 3: Expose Backend qua Ngrok (Không cần cùng mạng)

Nếu bạn muốn test trên thiết bị không cùng WiFi:

**Bước 1: Cài Ngrok**
```powershell
# Download từ https://ngrok.com/download
# Hoặc dùng winget:
winget install ngrok
```

**Bước 2: Expose backend**
```powershell
# Backend đang chạy ở port 5182
ngrok http 5182
```

**Output:**
```
Forwarding: https://abc123.ngrok.io -> http://localhost:5182
```

**Bước 3: Update URL trong MAUI app**
```csharp
private const string BaseUrl = "https://abc123.ngrok.io/api";
```

Giờ bạn có thể test từ bất kỳ đâu! 🌍

---

## ⚡ Workflow Phát Triển Tối Ưu:

### Setup một lần:

```powershell
# Terminal 1: Chạy Backend với watch
cd d:\ChillNestV2\Booking-HomeStay\BoookingHotels
dotnet watch run --urls "http://0.0.0.0:5182"

# Terminal 2: Chạy MAUI App với watch
cd d:\ChillNestV2\Booking-HomeStay\ChillNest.MobileApp
dotnet watch run --framework net8.0-android
```

### Khi code:

1. **Sửa Backend** (Controllers, Models, Services):
   - Lưu file → Backend tự động reload
   - MAUI app tự động nhận API mới

2. **Sửa Frontend** (XAML, ViewModels):
   - Lưa file → MAUI Hot Reload update UI ngay
   - Không cần rebuild!

3. **Thêm feature mới** (method, class mới):
   - Cần rebuild: `Ctrl+Shift+B`
   - App tự động restart

---

## 📱 Tips & Tricks:

### 1. Live Preview cho XAML
Trong Visual Studio 2022:
- View → Other Windows → **XAML Live Preview**
- Sửa XAML → Thấy preview real-time

### 2. Debugging cùng lúc Backend + Mobile
- Visual Studio: F5 Backend
- Terminal riêng: `dotnet watch run` cho MAUI
- Breakpoint hoạt động ở cả 2!

### 3. Xem logs từ Android Device
```powershell
# Xem logs real-time
adb logcat | Select-String "ChillNest"

# Hoặc trong Visual Studio: View → Output → Android
```

### 4. Clear cache khi lỗi
```powershell
# Clean build
dotnet clean
dotnet build

# Clear Android cache
adb shell pm clear <your.app.package.name>
```

---

## 🎯 Demo Workflow:

```powershell
# 1. Start Backend (Terminal 1)
cd BoookingHotels
dotnet watch run --urls "http://0.0.0.0:5182"

# 2. Check your IP
ipconfig  # Ví dụ: 192.168.1.100

# 3. Update MAUI ApiService.cs
# BaseUrl = "http://192.168.1.100:5182/api"  (device thật)
# BaseUrl = "http://10.0.2.2:5182/api"        (emulator)

# 4. Start MAUI App (Terminal 2)
cd ChillNest.MobileApp
dotnet watch run --framework net8.0-android

# 5. Code và xem thay đổi real-time! 🔥
# - Sửa XAML → Hot reload UI
# - Sửa API → Backend auto reload → App tự fetch data mới
# - Sửa ViewModel → Hot reload (hầu hết cases)
```

---

## ⚠️ Lưu ý:

1. **Android 9+ yêu cầu HTTPS**: Nếu gặp lỗi "Cleartext HTTP traffic not permitted":
   - Thêm vào `Platforms/Android/AndroidManifest.xml`:
   ```xml
   <application android:usesCleartextTraffic="true">
   ```

2. **Hot Reload không work cho:**
   - Thay đổi cấu trúc class (thêm property)
   - Thay đổi namespace
   - Thêm NuGet package mới
   → Cần rebuild: `Ctrl+Shift+B`

3. **Performance**:
   - Debug build chậm hơn Release
   - Test performance trên Release build
   - Hot Reload làm app chậm hơn 1 chút

---

## 🎉 Kết luận:

**YES! Bạn hoàn toàn có thể:**
- ✅ Code XAML → Xem ngay trên Android (Hot Reload)
- ✅ Code C# → Xem ngay (Hot Reload)
- ✅ Sửa Backend API → Mobile tự fetch data mới
- ✅ Debug cùng lúc Backend + Mobile
- ✅ Không cần rebuild liên tục

**Recommended setup:**
1. Visual Studio 2022 để code
2. Android Emulator hoặc device thật
3. 2 terminals: Backend + MAUI watch mode
4. Enjoy coding! 🚀
