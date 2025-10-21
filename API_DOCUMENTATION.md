# ChillNest Mobile API Documentation

## Base URL
```
http://localhost:5182/api
```

## Authentication
Hầu hết các API yêu cầu authentication. Sau khi login, lưu thông tin user và sử dụng Cookie authentication.

---

## 📱 API Endpoints

### 1. Authentication APIs

#### 1.1 Login
```http
POST /api/AuthApi/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response Success (200):**
```json
{
  "userId": 1,
  "userName": "john_doe",
  "email": "user@example.com",
  "fullName": "John Doe",
  "phone": "0123456789",
  "avatarUrl": "/Image/avatars/avatar_1.jpg",
  "roles": ["User"],
  "message": "Login successful"
}
```

#### 1.2 Register
```http
POST /api/AuthApi/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "phone": "0987654321",
  "password": "password123"
}
```

**Response Success (200):**
```json
{
  "message": "OTP sent to email",
  "email": "newuser@example.com",
  "otpForTesting": "123456",
  "tempUserToken": "base64_encoded_token"
}
```

#### 1.3 Verify OTP
```http
POST /api/AuthApi/verify-otp
Content-Type: application/json

{
  "tempUserToken": "base64_encoded_token_from_register",
  "otp": "123456"
}
```

#### 1.4 Get Profile
```http
GET /api/AuthApi/profile
Authorization: Required (Cookie)
```

#### 1.5 Update Profile
```http
PUT /api/AuthApi/profile
Authorization: Required (Cookie)
Content-Type: application/json

{
  "userName": "new_username",
  "fullName": "New Full Name",
  "phone": "0912345678"
}
```

---

### 2. Hotels APIs

#### 2.1 Get All Hotels (với phân trang và filter)
```http
GET /api/HotelsApi?search=Hanoi&city=Hanoi&checkIn=2025-12-01&checkOut=2025-12-05&page=1&pageSize=10
```

**Query Parameters:**
- `search` (optional): Tìm kiếm theo tên, địa chỉ, thành phố
- `city` (optional): Lọc theo thành phố
- `checkIn` (optional): Ngày nhận phòng (YYYY-MM-DD)
- `checkOut` (optional): Ngày trả phòng (YYYY-MM-DD)
- `page` (default: 1): Trang hiện tại
- `pageSize` (default: 10): Số item mỗi trang

**Response Success (200):**
```json
{
  "totalCount": 150,
  "page": 1,
  "pageSize": 10,
  "totalPages": 15,
  "data": [
    {
      "hotelId": 1,
      "name": "Grand Hotel Hanoi",
      "address": "123 Main Street",
      "city": "Hanoi",
      "country": "Vietnam",
      "description": "Luxury hotel in city center",
      "latitude": 21.0285,
      "longitude": 105.8542,
      "minPrice": 500000,
      "mainPhoto": "https://example.com/hotel1.jpg",
      "photos": ["url1", "url2"],
      "roomCount": 50,
      "avgRating": 4.5
    }
  ]
}
```

#### 2.2 Get Hotel Details
```http
GET /api/HotelsApi/122
```

**Response Success (200):**
```json
{
  "hotelId": 122,
  "name": "Grand Hotel",
  "address": "123 Main St",
  "city": "Hanoi",
  "country": "Vietnam",
  "description": "Luxury hotel...",
  "latitude": 21.0285,
  "longitude": 105.8542,
  "status": true,
  "createdAt": "2025-01-01T00:00:00",
  "photos": [
    { "photoId": 1, "url": "https://..." }
  ],
  "rooms": [
    {
      "roomId": 1,
      "name": "Deluxe Room",
      "capacity": 2,
      "bedType": "King",
      "size": 30,
      "price": 1000000,
      "mainPhoto": "https://...",
      "photos": ["url1", "url2"],
      "amenities": [
        {
          "amenityId": 1,
          "name": "WiFi",
          "icon": "bi-wifi"
        }
      ],
      "avgRating": 4.8,
      "reviewCount": 25
    }
  ],
  "reviews": [
    {
      "reviewId": 1,
      "rating": 5,
      "comment": "Great hotel!",
      "createdAt": "2025-01-15T10:00:00",
      "user": {
        "userId": 2,
        "userName": "john_doe",
        "fullName": "John Doe",
        "avatarUrl": "/Image/avatars/user.jpg"
      },
      "roomName": "Deluxe Room"
    }
  ],
  "avgRating": 4.7,
  "totalReviews": 150
}
```

#### 2.3 Get Top Cities
```http
GET /api/HotelsApi/cities
```

**Response Success (200):**
```json
[
  {
    "city": "Hanoi",
    "hotelCount": 250,
    "photo": "https://..."
  },
  {
    "city": "Ho Chi Minh",
    "hotelCount": 300,
    "photo": "https://..."
  }
]
```

#### 2.4 Get Nearby Hotels
```http
GET /api/HotelsApi/nearby?lat=21.0285&lng=105.8542&radius=5
```

**Query Parameters:**
- `lat` (required): Latitude
- `lng` (required): Longitude
- `radius` (optional, default: 5): Bán kính tìm kiếm (km)

---

### 3. Bookings APIs

#### 3.1 Get User Bookings
```http
GET /api/BookingsApi
Authorization: Required (Cookie)
```

**Response Success (200):**
```json
[
  {
    "bookingId": 1,
    "checkIn": "2025-12-01T14:00:00",
    "checkOut": "2025-12-05T12:00:00",
    "totalPrice": 4000000,
    "status": "Confirmed",
    "createdAt": "2025-11-20T10:00:00",
    "room": {
      "roomId": 1,
      "name": "Deluxe Room",
      "price": 1000000,
      "mainPhoto": "https://..."
    },
    "hotel": {
      "hotelId": 1,
      "name": "Grand Hotel",
      "address": "123 Main St",
      "city": "Hanoi"
    }
  }
]
```

#### 3.2 Get Booking Details
```http
GET /api/BookingsApi/1
Authorization: Required (Cookie)
```

#### 3.3 Create Booking
```http
POST /api/BookingsApi
Authorization: Required (Cookie)
Content-Type: application/json

{
  "roomId": 1,
  "checkIn": "2025-12-01T14:00:00",
  "checkOut": "2025-12-05T12:00:00",
  "guestName": "John Doe",
  "guestPhone": "0123456789"
}
```

**Response Success (201):**
```json
{
  "bookingId": 1,
  "checkIn": "2025-12-01T14:00:00",
  "checkOut": "2025-12-05T12:00:00",
  "totalPrice": 4000000,
  "status": "Pending",
  "message": "Booking created successfully",
  "hotelName": "Grand Hotel",
  "roomName": "Deluxe Room",
  "nights": 4
}
```

#### 3.4 Cancel Booking
```http
PUT /api/BookingsApi/1/cancel
Authorization: Required (Cookie)
```

**Response Success (200):**
```json
{
  "message": "Booking cancelled successfully",
  "bookingId": 1,
  "status": "Canceled"
}
```

#### 3.5 Check Room Availability
```http
POST /api/BookingsApi/check-availability
Content-Type: application/json

{
  "roomId": 1,
  "checkIn": "2025-12-01T14:00:00",
  "checkOut": "2025-12-05T12:00:00"
}
```

**Response Success (200):**
```json
{
  "available": true,
  "message": "Room is available"
}
```

---

## 🔒 Error Responses

### 400 Bad Request
```json
{
  "message": "Invalid request data"
}
```

### 401 Unauthorized
```json
{
  "message": "Unauthorized access"
}
```

### 404 Not Found
```json
{
  "message": "Resource not found"
}
```

### 500 Internal Server Error
```json
{
  "message": "An error occurred"
}
```

---

## 📌 Notes

1. **Authentication**: Sử dụng Cookie-based authentication. Sau khi login thành công, cookie sẽ tự động được lưu.

2. **Date Format**: Tất cả dates phải theo format ISO 8601: `YYYY-MM-DDTHH:mm:ss`

3. **Pagination**: Các API list thường có pagination với `page` và `pageSize`

4. **CORS**: API đã enable CORS cho mobile app

5. **Testing**: 
   - Test API bằng Postman hoặc Bruno
   - Base URL khi test local: `http://localhost:5182/api`
   - OTP sẽ được trả về trong response khi testing (cần remove trong production)

---

## 🚀 Ví dụ Flow đặt phòng

1. **Login**: POST `/api/AuthApi/login`
2. **Xem danh sách hotels**: GET `/api/HotelsApi?city=Hanoi`
3. **Xem chi tiết hotel**: GET `/api/HotelsApi/122`
4. **Check phòng trống**: POST `/api/BookingsApi/check-availability`
5. **Đặt phòng**: POST `/api/BookingsApi`
6. **Xem bookings của mình**: GET `/api/BookingsApi`
7. **Hủy booking (nếu cần)**: PUT `/api/BookingsApi/1/cancel`

---

## 📱 Ready for .NET MAUI Integration!

API này đã sẵn sàng để integrate với .NET MAUI mobile app. Các bước tiếp theo:

1. Tạo .NET MAUI project
2. Tạo Models tương ứng với API responses
3. Tạo ApiService để call các endpoints
4. Implement UI với XAML
5. Test trên Android emulator/device

Giờ bạn có thể bắt đầu làm mobile app rồi! 🎉
