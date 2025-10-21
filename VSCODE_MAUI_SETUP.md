# VS Code Setup cho .NET MAUI Android Development với Hot Reload

## 📦 Bước 1: Cài đặt Extensions

### Extensions BẮT BUỘC:
```
1. .NET MAUI (ms-dotnettools.dotnet-maui)
   - Extension chính thức từ Microsoft
   - Hỗ trợ debug, IntelliSense, deployment

2. Android iOS Emulator (diemasmichiels.emulate)
   - Launch Android emulator trực tiếp từ VS Code
   - Không cần mở Android Studio

3. .NET Meteor (nromanov.dotnet-meteor) - Optional nhưng recommended
   - Run/Debug .NET MAUI apps
   - Hot reload support
```

### Cài đặt nhanh:
1. Mở VS Code
2. Nhấn `Ctrl+Shift+X` để mở Extensions
3. Search và install từng extension ở trên

---

## 🔧 Bước 2: Setup Android SDK và Emulator

### Check Android SDK:
```powershell
# Kiểm tra Android SDK đã cài chưa
$env:ANDROID_HOME
# Nếu rỗng, cần cài Android Studio hoặc command line tools
```

### Tạo Android Emulator (nếu chưa có):
```powershell
# List available system images
sdkmanager --list | Select-String "system-images"

# Tạo emulator mới (API 33 - Android 13)
avdmanager create avd -n Pixel_5_API_33 -k "system-images;android-33;google_apis;x86_64" -d "pixel_5"

# Hoặc dùng Android Studio > Device Manager > Create Device
```

---

## 🎮 Bước 3: Launch Android Emulator từ VS Code

### Method 1: Dùng Android iOS Emulator Extension
1. Nhấn `Ctrl+Shift+P`
2. Gõ: `Emulator: Start Android Emulator`
3. Chọn emulator từ list
4. Emulator sẽ mở trong cửa sổ riêng

### Method 2: Dùng Command
```powershell
# List emulators
emulator -list-avds

# Start emulator
emulator -avd Pixel_5_API_33
```

### Method 3: Tạo Task trong VS Code
Tạo file `.vscode/tasks.json`:
```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "Start Android Emulator",
            "type": "shell",
            "command": "emulator",
            "args": [
                "-avd",
                "Pixel_5_API_33"
            ],
            "isBackground": true,
            "problemMatcher": [],
            "presentation": {
                "reveal": "never",
                "panel": "new"
            }
        }
    ]
}
```
Sau đó: `Ctrl+Shift+P` → `Tasks: Run Task` → `Start Android Emulator`

---

## 🔥 Bước 4: Run MAUI App với Hot Reload

### Method 1: Dùng .NET MAUI Extension
1. Mở project MAUI trong VS Code
2. Nhấn `F5` hoặc `Ctrl+Shift+D` → chọn `.NET MAUI` configuration
3. Chọn target: Android Emulator
4. App sẽ build, deploy và chạy
5. Hot reload tự động khi edit XAML/C#

### Method 2: Dùng Terminal với Hot Reload
```powershell
# Chạy với hot reload
cd ChillNest.MobileApp
dotnet watch run --framework net8.0-android

# App sẽ chạy trên emulator và hot reload khi có thay đổi
```

### Method 3: Dùng .NET Meteor Extension
1. Install .NET Meteor extension
2. Mở project MAUI
3. Nhấn `Ctrl+Shift+P` → `Meteor: Build and Run`
4. Chọn Android target
5. Hot reload enabled

---

## 📱 Bước 5: Hot Reload Workflow - Code + Preview

### Workflow Tối Ưu:
```
┌─────────────────────┐         ┌─────────────────────┐
│   VS Code Editor    │         │  Android Emulator   │
│                     │         │                     │
│  ✏️  Edit XAML      │────────▶│  🔄 Auto Refresh    │
│  ✏️  Edit C#        │         │  📱 See Changes     │
│                     │         │                     │
│  💾 Save (Ctrl+S)   │         │  ⚡ Instant Update   │
└─────────────────────┘         └─────────────────────┘
```

### Setup 2 Cửa Sổ:
1. **Màn hình 1 (VS Code)**:
   - Editor với code XAML/C#
   - Terminal chạy `dotnet watch`
   
2. **Màn hình 2 (Emulator)**:
   - Android Emulator chạy app
   - Tự động reload khi có thay đổi

### Enable MAUI Hot Reload:
Trong file `.csproj` của MAUI project:
```xml
<PropertyGroup>
    <!-- Enable Hot Reload -->
    <HotReloadAgent>true</HotReloadAgent>
    <UseXamarinHotReload>true</UseXamarinHotReload>
</PropertyGroup>
```

---

## 🎯 Bước 6: Debug Workflow

### Debug trong VS Code:
1. Set breakpoint trong C# code (click cột bên trái)
2. Nhấn `F5` để start debugging
3. App pause tại breakpoint
4. Inspect variables, step through code
5. Debug Console hiển thị logs

### Config Debug - `.vscode/launch.json`:
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET MAUI Android",
            "type": "maui",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/ChillNest.MobileApp/bin/Debug/net8.0-android/ChillNest.MobileApp.dll"
        }
    ]
}
```

---

## 💡 Tips & Tricks

### 1. **Scrcpy - Mirror Android Screen to PC**
Nếu muốn emulator nhỏ gọn hơn:
```powershell
# Install scrcpy
winget install Genymobile.scrcpy

# Run emulator
emulator -avd Pixel_5_API_33

# Mirror to smaller window
scrcpy --max-size 800 --window-title "ChillNest App"
```

### 2. **Multi-Monitor Setup**
- **Monitor 1**: VS Code fullscreen
- **Monitor 2**: Android Emulator + Backend API logs

### 3. **Fast Emulator Restart**
Tạo PowerShell alias:
```powershell
# Thêm vào $PROFILE
function Start-Emu { emulator -avd Pixel_5_API_33 -netdelay none -netspeed full }
Set-Alias -Name emu -Value Start-Emu
```
Sau đó chỉ cần gõ: `emu`

### 4. **Backend + MAUI cùng lúc**
```powershell
# Terminal 1: Backend với hot reload
cd BoookingHotels
dotnet watch run --urls "http://0.0.0.0:5182"

# Terminal 2: MAUI app với hot reload
cd ChillNest.MobileApp
dotnet watch run --framework net8.0-android
```

### 5. **VS Code Split Terminal**
- Nhấn `Ctrl+Shift+5` để split terminal
- Terminal 1: Backend
- Terminal 2: MAUI app
- Xem logs của cả 2 cùng lúc!

---

## 🐛 Troubleshooting

### ❌ "No Android devices found"
```powershell
# Check devices
adb devices

# Restart ADB
adb kill-server
adb start-server
```

### ❌ Emulator chậm
```powershell
# Enable hardware acceleration (HAXM for Intel, WHPX for Windows)
# Check in BIOS: Virtualization enabled

# Start emulator with more resources
emulator -avd Pixel_5_API_33 -memory 4096 -cores 4
```

### ❌ Hot Reload không hoạt động
1. Check `.csproj` có `<HotReloadAgent>true</HotReloadAgent>`
2. Restart với `dotnet watch` thay vì `dotnet run`
3. Xem logs: `dotnet watch --verbose`

### ❌ "Cannot connect to backend API"
```csharp
// Trong ApiService.cs, check BaseUrl:
// Emulator: http://10.0.2.2:5182/api
// Device:   http://[YOUR_IP]:5182/api

private const string BaseUrl = "http://10.0.2.2:5182/api";
```

---

## 📊 So sánh Workflow Options

| Method | Hot Reload | Debug | Emulator View | Setup |
|--------|-----------|-------|---------------|-------|
| .NET MAUI Extension | ✅ | ✅ | External | Easy |
| .NET Meteor | ✅ | ✅ | External | Easy |
| dotnet watch | ✅ | ❌ | External | Very Easy |
| F5 Debug | ❌ | ✅ | External | Easy |

**Khuyến nghị**: Dùng `.NET MAUI Extension` + `dotnet watch` cho tốc độ tối đa!

---

## 🎬 Quick Start Commands

```powershell
# 1. Start Backend
cd d:\ChillNestV2\Booking-HomeStay\BoookingHotels
dotnet watch run --urls "http://0.0.0.0:5182"

# 2. Start Emulator (Terminal mới)
emulator -avd Pixel_5_API_33

# 3. Run MAUI App với Hot Reload (Terminal mới)
cd d:\ChillNestV2\Booking-HomeStay\ChillNest.MobileApp
dotnet watch run --framework net8.0-android

# 4. Code trong VS Code → Save → Xem thay đổi trên Emulator ngay lập tức! 🚀
```

---

## 🌟 Bonus: VS Code Settings tối ưu cho MAUI

Thêm vào `.vscode/settings.json`:
```json
{
    "files.exclude": {
        "**/bin": true,
        "**/obj": true
    },
    "omnisharp.enableRoslynAnalyzers": true,
    "dotnet.defaultSolution": "BoookingHotels.sln",
    "[csharp]": {
        "editor.formatOnSave": true
    },
    "[xml]": {
        "editor.formatOnSave": true
    }
}
```

---

Với setup này, bạn có thể:
- ✅ Code trong VS Code
- ✅ Xem Android emulator cạnh bên
- ✅ Hot reload tự động khi save
- ✅ Debug với breakpoints
- ✅ Không cần Android Studio!

**Happy Coding! 🎉**
