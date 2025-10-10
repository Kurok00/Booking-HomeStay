using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🌱 Starting data seeding...");

            // Seed Roles (kiểm tra riêng biệt)
            await SeedRoles(context);
            
            // Seed Users (kiểm tra riêng biệt)
            await SeedUsers(context);
            
            // Seed UserRoles
            await SeedUserRoles(context);
            
            // Seed Amenities
            await SeedAmenities(context);
            
            // Seed Hotels
            await SeedHotels(context);
            
            // Seed Rooms
            await SeedRooms(context);
            
            // Seed Room Amenities
            await SeedRoomAmenities(context);
            
            // Seed Photos
            await SeedPhotos(context);
            
            // Seed Vouchers
            await SeedVouchers(context);
            
            // Seed Bookings
            await SeedBookings(context);
            
            // Seed Reviews
            await SeedReviews(context);
            
            // Seed Blogs
            await SeedBlogs(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRoles(ApplicationDbContext context)
        {
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "User" },
                    new Role { RoleName = "Host" }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Seeded 3 Roles");
            }
            else
            {
                Console.WriteLine("⚠️ Roles already exist, skipping...");
            }
        }

        private static async Task SeedUsers(ApplicationDbContext context)
        {
            var currentUserCount = await context.Users.CountAsync();
            if (currentUserCount < 31) // Cần 31 users (1 admin + 10 hosts + 20 users)
            {
                var users = new List<User>();
                
                // Admin user (chỉ thêm nếu chưa có admin)
                var adminExists = await context.Users.AnyAsync(u => u.UserName == "admin");
                if (!adminExists)
                {
                    users.Add(new User
                    {
                        UserName = "admin",
                        Email = "admin@booking.com", 
                        Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        FullName = "Administrator",
                        Phone = $"012345{DateTime.Now.Millisecond:D4}", // Unique phone
                        Status = true,
                        CreatedAt = DateTime.Now,
                        AvatarUrl = "/Image/admin-avatar.jpg"
                    });
                }

                // Host users (chỉ thêm những host chưa có)
                for (int i = 1; i <= 10; i++)
                {
                    var hostExists = await context.Users.AnyAsync(u => u.UserName == $"host{i}");
                    if (!hostExists)
                    {
                        users.Add(new User
                        {
                            UserName = $"host{i}",
                            Email = $"host{i}@booking.com",
                            Password = BCrypt.Net.BCrypt.HashPassword("host123"),
                            FullName = $"Host Number {i}",
                            Phone = $"0901234{i:D3}", // Unique phone pattern
                            Status = true,
                            CreatedAt = DateTime.Now.AddDays(-new Random().Next(1, 365)),
                            AvatarUrl = $"/Image/host{i}-avatar.jpg"
                        });
                    }
                }

                // Regular users (chỉ thêm những user chưa có)
                for (int i = 1; i <= 20; i++)
                {
                    var userExists = await context.Users.AnyAsync(u => u.UserName == $"user{i}");
                    if (!userExists)
                    {
                        users.Add(new User
                        {
                            UserName = $"user{i}",
                            Email = $"user{i}@gmail.com",
                            Password = BCrypt.Net.BCrypt.HashPassword("user123"),
                            FullName = $"User Number {i}",
                            Phone = $"0987654{i:D3}", // Unique phone pattern
                            Status = true,
                            CreatedAt = DateTime.Now.AddDays(-new Random().Next(1, 365)),
                            AvatarUrl = $"/Image/user{i}-avatar.jpg"
                        });
                    }
                }

                if (users.Count > 0)
                {
                    await context.Users.AddRangeAsync(users);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Seeded {users.Count} Users");
                }
                else
                {
                    Console.WriteLine("⚠️ All required users already exist");
                }
            }
            else
            {
                Console.WriteLine("⚠️ Users already exist, skipping...");
            }
        }

        private static async Task SeedUserRoles(ApplicationDbContext context)
        {
            if (await context.UserRoles.AnyAsync())
            {
                Console.WriteLine("⚠️ UserRoles already exist, skipping...");
                return;
            }

            var userRoles = new List<UserRole>();
            
            // Lấy actual User IDs và Role IDs
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            var hostRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Host");
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");

            if (adminUser != null && adminRole != null)
            {
                userRoles.Add(new UserRole { UserId = adminUser.UserId, RoleId = adminRole.RoleId });
            }

            // Host roles
            for (int i = 1; i <= 10; i++)
            {
                var hostUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == $"host{i}");
                if (hostUser != null && hostRole != null)
                {
                    userRoles.Add(new UserRole { UserId = hostUser.UserId, RoleId = hostRole.RoleId });
                }
            }

            // Regular user roles
            for (int i = 1; i <= 20; i++)
            {
                var regularUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == $"user{i}");
                if (regularUser != null && userRole != null)
                {
                    userRoles.Add(new UserRole { UserId = regularUser.UserId, RoleId = userRole.RoleId });
                }
            }

            if (userRoles.Count > 0)
            {
                await context.UserRoles.AddRangeAsync(userRoles);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Seeded {userRoles.Count} UserRoles");
            }
        }

        private static async Task SeedAmenities(ApplicationDbContext context)
        {
            var amenities = new[]
            {
                new Amenities { Name = "Wi-Fi miễn phí", Icon = "fas fa-wifi" },
                new Amenities { Name = "Điều hòa", Icon = "fas fa-snowflake" },
                new Amenities { Name = "TV", Icon = "fas fa-tv" },
                new Amenities { Name = "Tủ lạnh", Icon = "fas fa-cube" },
                new Amenities { Name = "Ban công", Icon = "fas fa-home" },
                new Amenities { Name = "Bãi đậu xe", Icon = "fas fa-car" },
                new Amenities { Name = "Hồ bơi", Icon = "fas fa-swimming-pool" },
                new Amenities { Name = "Gym", Icon = "fas fa-dumbbell" },
                new Amenities { Name = "Spa", Icon = "fas fa-spa" },
                new Amenities { Name = "Nhà hàng", Icon = "fas fa-utensils" },
                new Amenities { Name = "Bar", Icon = "fas fa-cocktail" },
                new Amenities { Name = "Phòng họp", Icon = "fas fa-handshake" },
                new Amenities { Name = "Máy sấy tóc", Icon = "fas fa-wind" },
                new Amenities { Name = "Két an toàn", Icon = "fas fa-shield-alt" },
                new Amenities { Name = "Phục vụ phòng 24/7", Icon = "fas fa-clock" },
                new Amenities { Name = "Máy giặt", Icon = "fas fa-tshirt" },
                new Amenities { Name = "Bàn làm việc", Icon = "fas fa-desk" },
                new Amenities { Name = "Minibar", Icon = "fas fa-glass-whiskey" },
                new Amenities { Name = "Máy pha cà phê", Icon = "fas fa-coffee" },
                new Amenities { Name = "View biển", Icon = "fas fa-water" }
            };

            await context.Amenities.AddRangeAsync(amenities);
            await context.SaveChangesAsync();
        }

        private static async Task SeedHotels(ApplicationDbContext context)
        {
            var random = new Random();
            var hotels = new List<Hotel>();
            var cities = new[] { "Hà Nội", "TP.HCM", "Đà Nẵng", "Hội An", "Nha Trang", "Phú Quốc", "Sa Pa", "Hạ Long", "Cần Thơ", "Huế" };
            var hotelTypes = new[] { "Resort", "Hotel", "Homestay", "Villa", "Apartment" };

            for (int i = 1; i <= 30; i++)
            {
                var city = cities[random.Next(cities.Length)];
                var type = hotelTypes[random.Next(hotelTypes.Length)];
                
                hotels.Add(new Hotel
                {
                    Name = $"{type} {city} {i}",
                    Description = $"Khách sạn tuyệt vời tại {city} với đầy đủ tiện nghi hiện đại. Vị trí thuận lợi, phục vụ chuyên nghiệp.",
                    Address = $"Số {i * 10}, Đường ABC, Quận {random.Next(1, 13)}, {city}",
                    City = city,
                    Country = "Việt Nam",
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-random.Next(30, 365)),
                    CreatedBy = random.Next(2, 12), // Host users (2-11)
                    IsApproved = true,
                    Latitude = 21.0285 + random.NextDouble() * 0.1, // Random around Hanoi
                    Longitude = 105.8542 + random.NextDouble() * 0.1
                });
            }

            await context.Hotels.AddRangeAsync(hotels);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRooms(ApplicationDbContext context)
        {
            var random = new Random();
            var rooms = new List<Room>();
            var roomTypes = new[] { "Phòng đơn", "Phòng đôi", "Phòng gia đình", "Suite", "Deluxe", "Standard" };

            // Mỗi hotel có 3-5 phòng
            for (int hotelId = 1; hotelId <= 30; hotelId++)
            {
                var roomCount = random.Next(3, 6);
                for (int j = 1; j <= roomCount; j++)
                {
                    var roomType = roomTypes[random.Next(roomTypes.Length)];
                    var capacity = roomType == "Phòng gia đình" ? random.Next(4, 7) : 
                                  roomType == "Suite" ? random.Next(2, 5) : 
                                  roomType == "Phòng đơn" ? 1 : 2;

                    rooms.Add(new Room
                    {
                        HotelId = hotelId,
                        Name = $"{roomType} {j:D2}",
                        Capacity = capacity,
                        Price = (decimal)(roomType == "Suite" ? random.Next(1500000, 3000000) : 
                                        roomType == "Deluxe" ? random.Next(1000000, 2000000) : 
                                        random.Next(500000, 1500000)),
                        Size = random.Next(20, 50),
                        BedType = capacity > 2 ? "King bed + Sofa bed" : capacity == 2 ? "Queen bed" : "Single bed",
                        Status = true
                    });
                }
            }

            await context.Rooms.AddRangeAsync(rooms);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRoomAmenities(ApplicationDbContext context)
        {
            var random = new Random();
            var roomAmenities = new List<RoomAmenitie>();
            var roomCount = await context.Rooms.CountAsync();

            // Mỗi phòng có 5-10 tiện nghi ngẫu nhiên
            for (int roomId = 1; roomId <= roomCount; roomId++)
            {
                var amenityIds = Enumerable.Range(1, 20).OrderBy(x => random.Next()).Take(random.Next(5, 11)).ToList();
                
                foreach (var amenityId in amenityIds)
                {
                    roomAmenities.Add(new RoomAmenitie
                    {
                        RoomId = roomId,
                        AmenityId = amenityId
                    });
                }
            }

            await context.RoomAmenities.AddRangeAsync(roomAmenities);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPhotos(ApplicationDbContext context)
        {
            var random = new Random();
            var photos = new List<Photos>();

            // Ảnh cho hotels
            for (int hotelId = 1; hotelId <= 30; hotelId++)
            {
                var photoCount = random.Next(3, 6);
                for (int j = 1; j <= photoCount; j++)
                {
                    photos.Add(new Photos
                    {
                        HotelId = hotelId,
                        Url = $"/Image/{j}.jpg", // Sử dụng ảnh có sẵn
                        SortOrder = j
                    });
                }
            }

            // Ảnh cho rooms
            var roomCount = await context.Rooms.CountAsync();
            for (int roomId = 1; roomId <= roomCount; roomId++)
            {
                var photoCount = random.Next(2, 4);
                for (int j = 1; j <= photoCount; j++)
                {
                    photos.Add(new Photos
                    {
                        RoomId = roomId,
                        Url = $"/Image/{j + 3}.jpg", // Sử dụng ảnh khác
                        SortOrder = j
                    });
                }
            }

            await context.Photoss.AddRangeAsync(photos);
            await context.SaveChangesAsync();
        }

        private static async Task SeedVouchers(ApplicationDbContext context)
        {
            var random = new Random();
            var vouchers = new List<Voucher>();

            for (int i = 1; i <= 20; i++)
            {
                var discountType = random.Next(2) == 0 ? "Percentage" : "FixedAmount";
                var discountValue = discountType == "Percentage" ? random.Next(5, 31) : random.Next(50000, 500000);

                vouchers.Add(new Voucher
                {
                    Code = $"VOUCHER{i:D3}",
                    Description = $"Giảm giá {(discountType == "Percentage" ? $"{discountValue}%" : $"{discountValue:N0} VNĐ")} cho đặt phòng",
                    DiscountType = discountType,
                    DiscountValue = discountValue,
                    MinOrderValue = discountType == "Percentage" ? random.Next(500000, 2000000) : 0,
                    Quantity = random.Next(10, 101),
                    ExpiryDate = DateTime.Now.AddDays(random.Next(30, 180)),
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 60))
                });
            }

            await context.Vouchers.AddRangeAsync(vouchers);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBookings(ApplicationDbContext context)
        {
            var random = new Random();
            var bookings = new List<Booking>();
            var statuses = new[] { BookingStatus.Pending, BookingStatus.Confirmed, BookingStatus.Paid, BookingStatus.Canceled };
            var roomCount = await context.Rooms.CountAsync();

            for (int i = 1; i <= 50; i++)
            {
                var checkIn = DateTime.Now.AddDays(random.Next(-30, 90));
                var checkOut = checkIn.AddDays(random.Next(1, 8));
                var roomId = random.Next(1, roomCount + 1);
                var room = await context.Rooms.FindAsync(roomId);
                var subtotal = room!.Price * (decimal)(checkOut - checkIn).Days;
                var discount = random.Next(0, 2) == 1 ? subtotal * 0.1m : 0;

                bookings.Add(new Booking
                {
                    UserId = random.Next(12, 32), // User IDs 12-31
                    HotelId = room.HotelId,
                    RoomId = roomId,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    GuestName = $"Khách hàng {i}",
                    GuestPhone = $"090{random.Next(10000000, 99999999)}",
                    SubTotal = subtotal,
                    Discount = discount,
                    Total = subtotal - discount,
                    Status = statuses[random.Next(statuses.Length)],
                    Currency = "VND",
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 60))
                });
            }

            await context.Bookings.AddRangeAsync(bookings);
            await context.SaveChangesAsync();
        }

        private static async Task SeedReviews(ApplicationDbContext context)
        {
            var random = new Random();
            var reviews = new List<Review>();
            var comments = new[]
            {
                "Khách sạn tuyệt vời, phòng sạch sẽ và thoải mái!",
                "Dịch vụ chuyên nghiệp, nhân viên thân thiện.",
                "Vị trí thuận lợi, gần nhiều địa điểm du lịch.",
                "Phòng rộng rãi, view đẹp, rất hài lòng.",
                "Giá cả hợp lý, chất lượng tốt.",
                "Sẽ quay lại lần sau, recommend cho mọi người.",
                "Bữa sáng ngon, nhiều món lựa chọn.",
                "Hồ bơi sạch sẽ, khu vực giải trí tốt.",
                "Check-in nhanh chóng, thuận tiện.",
                "Cần cải thiện thêm về âm thanh cách âm."
            };

            var roomCount = await context.Rooms.CountAsync();

            for (int i = 1; i <= 80; i++)
            {
                var roomId = random.Next(1, roomCount + 1);
                var room = await context.Rooms.Include(r => r.Hotel).FirstAsync(r => r.RoomId == roomId);

                reviews.Add(new Review
                {
                    UserId = random.Next(12, 32), // User IDs 12-31
                    HotelId = room.HotelId,
                    RoomId = roomId,
                    Rating = random.Next(3, 6), // 3-5 stars
                    Comment = comments[random.Next(comments.Length)],
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 90))
                });
            }

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBlogs(ApplicationDbContext context)
        {
            var blogs = new[]
            {
                new Blog
                {
                    Title = "Top 10 khách sạn tốt nhất tại Hà Nội",
                    Content = "Khám phá những khách sạn hàng đầu tại thủ đô với đầy đủ tiện nghi và dịch vụ chất lượng cao...",
                    ShortDescription = "Danh sách các khách sạn được đánh giá cao nhất tại Hà Nội",
                    Author = "Admin",
                    CreatedDate = DateTime.Now.AddDays(-30),
                    ReviewerId = 1 // Admin user
                },
                new Blog
                {
                    Title = "Kinh nghiệm du lịch Đà Nẵng - Hội An",
                    Content = "Hướng dẫn chi tiết về lịch trình, địa điểm tham quan và lựa chọn chỗ ở tại Đà Nẵng - Hội An...",
                    ShortDescription = "Kinh nghiệm và tips du lịch Đà Nẵng - Hội An",
                    Author = "Travel Expert",
                    CreatedDate = DateTime.Now.AddDays(-25),
                    ReviewerId = 1
                },
                new Blog
                {
                    Title = "Những resort sang trọng tại Phú Quốc",
                    Content = "Tổng hợp các resort 5 sao tại đảo ngọc Phú Quốc với view biển tuyệt đẹp và dịch vụ đẳng cấp...",
                    ShortDescription = "Review các resort cao cấp tại Phú Quốc",
                    Author = "Resort Reviewer",
                    CreatedDate = DateTime.Now.AddDays(-20),
                    ReviewerId = 1
                }
            };

            await context.Blogs.AddRangeAsync(blogs);
            await context.SaveChangesAsync();
        }
    }
}