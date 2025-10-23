using BoookingHotels.Models;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Data
{
    public static class HostDataSeeder
    {
        public static async Task SeedHostHotelsAndRoomsAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🏨 Starting Host Hotels & Rooms seeding...");

            // Lấy danh sách host users (role Host)
            var hostRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Host");
            if (hostRole == null)
            {
                Console.WriteLine("❌ Host role not found! Please seed roles first.");
                return;
            }

            var hostUserIds = await context.UserRoles
                .Where(ur => ur.RoleId == hostRole.RoleId)
                .Select(ur => ur.UserId)
                .ToListAsync();

            if (!hostUserIds.Any())
            {
                Console.WriteLine("❌ No host users found! Please seed users first.");
                return;
            }

            Console.WriteLine($"📋 Found {hostUserIds.Count} host users");

            var random = new Random();
            var cities = new[] { "Hà Nội", "TP.HCM", "Đà Nẵng", "Hội An", "Nha Trang", "Phú Quốc", "Sa Pa", "Hạ Long", "Cần Thơ", "Huế", "Vũng Tàu", "Đà Lạt" };
            var hotelTypes = new[] { "Resort", "Hotel", "Homestay", "Villa", "Apartment", "Boutique Hotel", "Hostel", "Motel" };
            var roomTypes = new[] { "Phòng đơn", "Phòng đôi", "Phòng gia đình", "Suite", "Deluxe", "Standard", "Premium", "VIP", "Executive", "Junior Suite" };
            var bedTypes = new[] { "Single bed", "Queen bed", "King bed", "Twin beds", "King bed + Sofa bed", "2 Queen beds" };

            var hotelNames = new[]
            {
                "Ocean View", "Mountain Peak", "City Center", "Sunset Paradise", "Golden Gate",
                "Blue Lagoon", "Silver Star", "Diamond Palace", "Royal Garden", "Green Valley",
                "Lotus Flower", "Pearl Harbor", "Crystal Bay", "Emerald Isle", "Ruby Tower",
                "Sapphire Beach", "Amber Court", "Jade Garden", "Ivory Palace", "Opal Residence"
            };

            foreach (var hostUserId in hostUserIds)
            {
                var hostUser = await context.Users.FindAsync(hostUserId);
                Console.WriteLine($"\n👤 Processing host: {hostUser?.UserName} (ID: {hostUserId})");

                // Kiểm tra xem host này đã có hotels chưa
                var existingHotelsCount = await context.Hotels
                    .Where(h => h.CreatedBy == hostUserId && h.IsUserHostCreated == true)
                    .CountAsync();

                if (existingHotelsCount >= 20)
                {
                    Console.WriteLine($"⚠️  Host {hostUser?.UserName} already has {existingHotelsCount} hotels. Skipping...");
                    continue;
                }

                // Tạo 20 hotels cho mỗi host
                var hotelsToAdd = 20 - existingHotelsCount;
                Console.WriteLine($"📝 Creating {hotelsToAdd} hotels for host {hostUser?.UserName}...");

                for (int i = 1; i <= hotelsToAdd; i++)
                {
                    var city = cities[random.Next(cities.Length)];
                    var type = hotelTypes[random.Next(hotelTypes.Length)];
                    var hotelName = hotelNames[random.Next(hotelNames.Length)];

                    var hotel = new Hotel
                    {
                        Name = $"{hotelName} {type} - {city}",
                        Description = $"Khách sạn {type} cao cấp tại {city} với đầy đủ tiện nghi hiện đại. " +
                                     $"Vị trí thuận lợi, gần trung tâm thành phố và các điểm tham quan nổi tiếng. " +
                                     $"Phục vụ chuyên nghiệp 24/7, đảm bảo trải nghiệm tuyệt vời cho quý khách.",
                        Address = $"Số {random.Next(1, 999)}, Đường {(char)random.Next(65, 91)}{(char)random.Next(65, 91)}, Quận {random.Next(1, 13)}, {city}",
                        City = city,
                        Country = "Việt Nam",
                        Status = true,
                        CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365)),
                        CreatedBy = hostUserId,
                        IsUserHostCreated = true,
                        IsApproved = random.Next(100) < 80, // 80% được approved
                        Latitude = 10.0 + random.NextDouble() * 12.0, // Random Vietnam coordinates
                        Longitude = 102.0 + random.NextDouble() * 8.0
                    };

                    await context.Hotels.AddAsync(hotel);
                    await context.SaveChangesAsync(); // Save để lấy HotelId

                    Console.WriteLine($"  ✅ Created hotel #{i}: {hotel.Name} (ID: {hotel.HotelId})");

                    // Tạo 3-5 ảnh cho mỗi hotel
                    var hotelPhotoCount = random.Next(3, 6);
                    var hotelPhotos = new List<Photos>();
                    for (int p = 1; p <= hotelPhotoCount; p++)
                    {
                        hotelPhotos.Add(new Photos
                        {
                            HotelId = hotel.HotelId,
                            Url = $"/Image/{random.Next(1, 10)}.jpg",
                            SortOrder = p
                        });
                    }
                    await context.Photoss.AddRangeAsync(hotelPhotos);

                    // Tạo 10 rooms cho mỗi hotel
                    var rooms = new List<Room>();
                    for (int j = 1; j <= 10; j++)
                    {
                        var roomType = roomTypes[random.Next(roomTypes.Length)];
                        var bedType = bedTypes[random.Next(bedTypes.Length)];
                        var capacity = roomType.Contains("gia đình") ? random.Next(4, 7) :
                                      roomType == "Suite" || roomType == "Junior Suite" ? random.Next(2, 5) :
                                      roomType == "Phòng đơn" || roomType == "Standard" ? 1 : 2;

                        var basePrice = roomType switch
                        {
                            "Suite" or "VIP" or "Executive" => random.Next(2000000, 4000000),
                            "Deluxe" or "Premium" or "Junior Suite" => random.Next(1200000, 2500000),
                            "Phòng gia đình" => random.Next(1500000, 2800000),
                            _ => random.Next(500000, 1500000)
                        };

                        var room = new Room
                        {
                            HotelId = hotel.HotelId,
                            Name = $"{roomType} {j:D2}",
                            Capacity = capacity,
                            Price = (decimal)basePrice,
                            Size = random.Next(20, 60),
                            BedType = bedType,
                            Status = true
                        };

                        rooms.Add(room);
                    }

                    await context.Rooms.AddRangeAsync(rooms);
                    await context.SaveChangesAsync(); // Save để lấy RoomId

                    Console.WriteLine($"    ✅ Created 10 rooms for hotel {hotel.Name}");

                    // Thêm amenities cho từng room (5-12 amenities ngẫu nhiên)
                    var allAmenityIds = await context.Amenities.Select(a => a.AmenityId).ToListAsync();
                    var roomAmenities = new List<RoomAmenitie>();

                    foreach (var room in rooms)
                    {
                        var amenityCount = random.Next(5, 13);
                        var selectedAmenities = allAmenityIds.OrderBy(x => random.Next()).Take(amenityCount);

                        foreach (var amenityId in selectedAmenities)
                        {
                            roomAmenities.Add(new RoomAmenitie
                            {
                                RoomId = room.RoomId,
                                AmenityId = amenityId
                            });
                        }
                    }

                    await context.RoomAmenities.AddRangeAsync(roomAmenities);

                    // Thêm ảnh cho từng room (2-4 ảnh)
                    var roomPhotos = new List<Photos>();
                    foreach (var room in rooms)
                    {
                        var photoCount = random.Next(2, 5);
                        for (int p = 1; p <= photoCount; p++)
                        {
                            roomPhotos.Add(new Photos
                            {
                                RoomId = room.RoomId,
                                Url = $"/Image/{random.Next(1, 10)}.jpg",
                                SortOrder = p
                            });
                        }
                    }

                    await context.Photoss.AddRangeAsync(roomPhotos);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"    ✅ Added amenities and photos for all rooms");
                }

                var totalHotels = await context.Hotels
                    .Where(h => h.CreatedBy == hostUserId && h.IsUserHostCreated == true)
                    .CountAsync();

                Console.WriteLine($"✅ Completed! Host {hostUser?.UserName} now has {totalHotels} hotels");
            }

            Console.WriteLine("\n🎉 Host Hotels & Rooms seeding completed!");
        }
    }
}
