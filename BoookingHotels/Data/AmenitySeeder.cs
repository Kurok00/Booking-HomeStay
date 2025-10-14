using BoookingHotels.Models;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Data
{
    public static class AmenitySeeder
    {
        public static async Task SeedAmenitiesAndRoomAmenitiesAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🏨 === BẮT ĐẦU SEED AMENITIES ===");

            // 1. Seed Amenities nếu chưa có
            if (!await context.Amenities.AnyAsync())
            {
                Console.WriteLine("📝 Đang thêm Amenities...");
                var amenities = new[]
                {
                    new Amenities { Name = "Wi-Fi miễn phí", Icon = "bi-wifi" },
                    new Amenities { Name = "Điều hòa", Icon = "bi-snow" },
                    new Amenities { Name = "TV", Icon = "bi-tv" },
                    new Amenities { Name = "Tủ lạnh", Icon = "bi-snow2" },
                    new Amenities { Name = "Ban công", Icon = "bi-house-door" },
                    new Amenities { Name = "Bãi đậu xe", Icon = "bi-car-front" },
                    new Amenities { Name = "Hồ bơi", Icon = "bi-water" },
                    new Amenities { Name = "Gym", Icon = "bi-heart-pulse" },
                    new Amenities { Name = "Spa", Icon = "bi-person-hearts" },
                    new Amenities { Name = "Nhà hàng", Icon = "bi-cup-hot" },
                    new Amenities { Name = "Bar", Icon = "bi-cup-straw" },
                    new Amenities { Name = "Phòng họp", Icon = "bi-people" },
                    new Amenities { Name = "Máy sấy tóc", Icon = "bi-wind" },
                    new Amenities { Name = "Két an toàn", Icon = "bi-shield-lock" },
                    new Amenities { Name = "Phục vụ phòng 24/7", Icon = "bi-clock" },
                    new Amenities { Name = "Máy giặt", Icon = "bi-droplet" },
                    new Amenities { Name = "Bàn làm việc", Icon = "bi-laptop" },
                    new Amenities { Name = "Minibar", Icon = "bi-cup" },
                    new Amenities { Name = "Máy pha cà phê", Icon = "bi-cup-hot-fill" },
                    new Amenities { Name = "View biển", Icon = "bi-water" }
                };

                await context.Amenities.AddRangeAsync(amenities);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Đã thêm {amenities.Length} amenities");
            }
            else
            {
                Console.WriteLine("✅ Amenities đã tồn tại");
            }

            // 2. Seed RoomAmenities cho các rooms chưa có
            var roomsWithoutAmenities = await context.Rooms
                .Include(r => r.RoomAmenities)
                .Where(r => r.RoomAmenities == null || !r.RoomAmenities.Any())
                .ToListAsync();

            if (roomsWithoutAmenities.Any())
            {
                Console.WriteLine($"📝 Đang thêm amenities cho {roomsWithoutAmenities.Count} phòng...");
                
                var random = new Random();
                var roomAmenities = new List<RoomAmenitie>();
                var totalAmenities = await context.Amenities.CountAsync();

                foreach (var room in roomsWithoutAmenities)
                {
                    // Mỗi phòng có 5-10 tiện nghi ngẫu nhiên
                    var amenityCount = random.Next(5, 11);
                    var amenityIds = Enumerable.Range(1, totalAmenities)
                        .OrderBy(x => random.Next())
                        .Take(amenityCount)
                        .ToList();

                    foreach (var amenityId in amenityIds)
                    {
                        roomAmenities.Add(new RoomAmenitie
                        {
                            RoomId = room.RoomId,
                            AmenityId = amenityId
                        });
                    }
                }

                await context.RoomAmenities.AddRangeAsync(roomAmenities);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Đã thêm {roomAmenities.Count} room-amenity mappings");
            }
            else
            {
                Console.WriteLine("✅ Tất cả rooms đều đã có amenities");
            }

            Console.WriteLine("🏨 === HOÀN THÀNH SEED AMENITIES ===");
        }
    }
}
