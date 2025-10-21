# 🏨 ChillNest - Booking HomeStay System

Một hệ thống đặt phòng khách sạn và homestay hiện đại được xây dựng bằng ASP.NET Core 8.0, cho phép người dùng tìm kiếm, đặt phòng và quản lý các chuyến nghỉ dưỡng của mình một cách dễ dàng.

## ✨ Tính năng chính

### 👥 Cho Khách hàng
- **Tìm kiếm & Đặt phòng**: Tìm kiếm khách sạn theo địa điểm, ngày và số lượng khách
- **Xem chi tiết**: Thông tin chi tiết về khách sạn, phòng, tiện nghi và hình ảnh
- **Đánh giá & Nhận xét**: Xem và viết đánh giá về trải nghiệm lưu trú
- **Quản lý đặt phòng**: Theo dõi lịch sử đặt phòng và trạng thái
- **Hệ thống voucher**: Sử dụng mã giảm giá để tiết kiệm chi phí
- **Blog du lịch**: Đọc các bài viết hướng dẫn và kinh nghiệm du lịch

### 🏠 Cho Chủ nhà (Host)
- **Đăng ký làm host**: Tạo tài khoản để quản lý khách sạn/homestay
- **Quản lý khách sạn**: Thêm, sửa, xóa thông tin khách sạn
- **Quản lý phòng**: Tạo và cập nhật thông tin các phòng
- **Theo dõi đặt phòng**: Xem và quản lý các booking từ khách hàng

### 👨‍💼 Cho Quản trị viên
- **Dashboard tổng quan**: Thống kê doanh thu, booking, người dùng
- **Quản lý người dùng**: Tạo, sửa, xóa tài khoản và phân quyền
- **Duyệt khách sạn**: Phê duyệt khách sạn do host tạo
- **Quản lý voucher**: Tạo và quản lý các chương trình khuyến mãi
- **Quản lý blog**: Viết và quản lý nội dung blog
- **Báo cáo hoạt động**: Theo dõi log hoạt động của admin

## 🛠️ Công nghệ sử dụng

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server với Entity Framework Core 9.0
- **Authentication**: Cookie-based Authentication
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **PDF Generation**: QuestPDF
- **Password Hashing**: BCrypt.Net
- **Email Service**: SMTP Email Sender

## 📋 Yêu cầu hệ thống

- .NET 8.0 SDK
- SQL Server 2019 hoặc SQL Server LocalDB
- Visual Studio 2022 hoặc VS Code
- Git

## 🚀 Cài đặt và chạy dự án

### 1. Clone repository
```bash
git clone https://github.com/Kurok00/Booking-HomeStay.git
cd Booking-HomeStay
```

### 2. Cài đặt .NET 8.0 SDK
Tải và cài đặt từ: https://dotnet.microsoft.com/download/dotnet/8.0

### 3. Cấu hình Database
- Mở file `appsettings.json` trong thư mục `BoookingHotels`
- Cập nhật connection string theo cấu hình SQL Server của bạn:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=BoookingHotels;Integrated Security=true;TrustServerCertificate=True;"
  }
}
```

### 4. Tạo và cập nhật Database
```bash
cd BoookingHotels
dotnet ef database update
```

### 5. Restore packages và chạy ứng dụng
```bash
dotnet restore
dotnet run
```

### 6. Truy cập ứng dụng
Mở trình duyệt và truy cập: `https://localhost:5001` hoặc `http://localhost:5000`

## 🗂️ Cấu trúc dự án

```
BoookingHotels/
├── Controllers/          # Các controller xử lý logic
├── Models/              # Các entity models
├── Views/               # Razor views (UI)
├── Data/                # DbContext và seeding data
├── Service/             # Các service (Email, etc.)
├── wwwroot/             # Static files (CSS, JS, images)
├── Properties/          # Launch settings
└── appsettings.json     # Configuration
```

## 🔐 Tài khoản mặc định

Sau khi chạy ứng dụng lần đầu, bạn có thể tạo tài khoản admin thông qua trang đăng ký hoặc thêm trực tiếp vào database.

## 📊 Database Schema

Dự án sử dụng các bảng chính:
- `Users` - Thông tin người dùng
- `Hotels` - Thông tin khách sạn
- `Rooms` - Thông tin phòng
- `Bookings` - Thông tin đặt phòng
- `Reviews` - Đánh giá và nhận xét
- `Amenities` - Tiện nghi
- `Vouchers` - Mã giảm giá
- `Blogs` - Bài viết blog

## 🤝 Đóng góp

1. Fork dự án
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit thay đổi (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📝 License

Dự án này được phân phối dưới [MIT License](LICENSE).

## 📞 Liên hệ

- **Developer**: Kurok00
- **GitHub**: [https://github.com/Kurok00](https://github.com/Kurok00)
- **Repository**: [https://github.com/Kurok00/Booking-HomeStay](https://github.com/Kurok00/Booking-HomeStay)

## 🎯 Roadmap

- [ ] Tích hợp thanh toán online (VNPay, PayPal)
- [ ] Ứng dụng mobile (React Native/Flutter)
- [ ] Hệ thống chat real-time
- [ ] API REST cho third-party integration
- [ ] Hệ thống thông báo push
- [ ] Tích hợp bản đồ Google Maps
- [ ] Hỗ trợ đa ngôn ngữ

---

⭐ Nếu dự án này hữu ích, hãy star repository để ủng hộ nhé!