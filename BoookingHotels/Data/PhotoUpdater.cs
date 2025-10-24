using Microsoft.EntityFrameworkCore;
using BoookingHotels.Models;

namespace BoookingHotels.Data
{
    public static class PhotoUpdater
    {
        public static async Task UpdateHotelPhotosAsync(ApplicationDbContext context)
        {
            try
            {
                // Chỉ update nếu có hotel thiếu ảnh
                var hotels = await context.Hotels.ToListAsync();
                var hotelsWithFewPhotos = hotels.Where(h => context.Photoss.Count(p => p.HotelId == h.HotelId) < 3).ToList();
                if (!hotelsWithFewPhotos.Any())
                {
                    Console.WriteLine("✅ Tất cả hotels đã có đủ ảnh!");
                    return;
                }

                // Xóa photos cũ cho các hotel thiếu ảnh
                var hotelIdsToUpdate = hotelsWithFewPhotos.Select(h => h.HotelId).ToList();
                var existingPhotos = await context.Photoss.Where(p => p.HotelId != null && hotelIdsToUpdate.Contains((int)p.HotelId)).ToListAsync();
                if (existingPhotos.Any())
                {
                    context.Photoss.RemoveRange(existingPhotos);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"🗑️  Đã xóa {existingPhotos.Count} ảnh cũ cho hotels thiếu ảnh");
                }

                Console.WriteLine($"🔄 Đang thêm ảnh cho {hotelsWithFewPhotos.Count} khách sạn...");

                // URLs ảnh thực tế cho từng thành phố
                // 200+ unique Unsplash hotel images
                var allHotelPhotos = new List<string>();
                for (int i = 1; i <= 200; i++)
                {
                    allHotelPhotos.Add($"https://source.unsplash.com/800x600/?hotel,building,room,view,city,landscape&sig={i}");
                }
                var hotelPhotos = new Dictionary<string, List<string>>
                {
                    ["Đà Lạt"] = allHotelPhotos,
                    ["Vũng Tàu"] = allHotelPhotos,
                    ["Phú Quốc"] = allHotelPhotos,
                    ["Đà Nẵng"] = allHotelPhotos,
                    ["Nha Trang"] = allHotelPhotos
                };

                var photosToAdd = new List<Photos>();
                var random = new Random();

                foreach (var hotel in hotels)
                {
                    var city = hotel.City;

                    // Lấy danh sách ảnh cho thành phố
                    List<string> cityPhotos;
                    if (hotelPhotos.ContainsKey(city))
                    {
                        cityPhotos = hotelPhotos[city];
                    }
                    else
                    {
                        // Nếu không có ảnh cho thành phố, dùng ảnh chung
                        cityPhotos = hotelPhotos["Đà Lạt"];
                    }

                    // Thêm 3-5 ảnh cho mỗi hotel
                    var photoCount = random.Next(3, 6);
                    var usedPhotos = new HashSet<string>();

                    for (int i = 0; i < photoCount; i++)
                    {
                        string photoUrl;
                        do
                        {
                            photoUrl = cityPhotos[random.Next(cityPhotos.Count)];
                        } while (usedPhotos.Contains(photoUrl) && usedPhotos.Count < cityPhotos.Count);

                        usedPhotos.Add(photoUrl);

                        photosToAdd.Add(new Photos
                        {
                            HotelId = hotel.HotelId,
                            RoomId = null,
                            Url = photoUrl,
                            SortOrder = i + 1
                        });
                    }
                }

                if (photosToAdd.Any())
                {
                    context.Photoss.AddRange(photosToAdd);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"📸 Đã thêm {photosToAdd.Count} ảnh cho hotels!");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi cập nhật ảnh hotel: {ex.Message}");
            }
        }

        public static async Task UpdateRoomPhotosAsync(ApplicationDbContext context)
        {
            try
            {
                // Chỉ update nếu có room thiếu ảnh
                var rooms = await context.Rooms.Include(r => r.Hotel).Where(r => r.Hotel != null).ToListAsync();
                var roomsWithFewPhotos = rooms.Where(r => context.Photoss.Count(p => p.RoomId == r.RoomId) < 2).ToList();
                if (!roomsWithFewPhotos.Any())
                {
                    Console.WriteLine("✅ Tất cả rooms đã có đủ ảnh!");
                    return;
                }

                // Xóa room photos cũ cho các room thiếu ảnh
                var roomIdsToUpdate = roomsWithFewPhotos.Select(r => r.RoomId).ToList();
                var existingRoomPhotos = await context.Photoss.Where(p => p.RoomId != null && roomIdsToUpdate.Contains((int)p.RoomId)).ToListAsync();
                if (existingRoomPhotos.Any())
                {
                    context.Photoss.RemoveRange(existingRoomPhotos);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"🗑️  Đã xóa {existingRoomPhotos.Count} ảnh phòng cũ cho rooms thiếu ảnh");
                }

                Console.WriteLine($"🔄 Đang thêm ảnh cho {roomsWithFewPhotos.Count} phòng...");

                // URLs ảnh phòng theo loại
                // 200+ unique Unsplash room images
                var allRoomPhotos = new List<string>();
                for (int i = 1; i <= 200; i++)
                {
                    allRoomPhotos.Add($"https://source.unsplash.com/800x600/?room,bed,interior,apartment,suite,villa&sig={i}");
                }
                var roomPhotos = new Dictionary<string, List<string>>
                {
                    ["Suite"] = allRoomPhotos,
                    ["Villa"] = allRoomPhotos,
                    ["Room"] = allRoomPhotos,
                    ["Apartment"] = allRoomPhotos
                };

                var photosToAdd = new List<Photos>();
                var random = new Random();

                foreach (var room in rooms)
                {
                    // Xác định loại phòng
                    string roomType = "Room";
                    if (room.Name.Contains("Suite", StringComparison.OrdinalIgnoreCase))
                        roomType = "Suite";
                    else if (room.Name.Contains("Villa", StringComparison.OrdinalIgnoreCase))
                        roomType = "Villa";
                    else if (room.Name.Contains("Apartment", StringComparison.OrdinalIgnoreCase) ||
                             room.Name.Contains("Studio", StringComparison.OrdinalIgnoreCase))
                        roomType = "Apartment";

                    var typePhotos = roomPhotos[roomType];

                    // Thêm 2-3 ảnh cho mỗi room
                    var photoCount = random.Next(2, 4);
                    var usedPhotos = new HashSet<string>();

                    for (int i = 0; i < photoCount; i++)
                    {
                        string photoUrl;
                        do
                        {
                            photoUrl = typePhotos[random.Next(typePhotos.Count)];
                        } while (usedPhotos.Contains(photoUrl) && usedPhotos.Count < typePhotos.Count);

                        usedPhotos.Add(photoUrl);

                        photosToAdd.Add(new Photos
                        {
                            HotelId = null,
                            RoomId = room.RoomId,
                            Url = photoUrl,
                            SortOrder = i + 1
                        });
                    }
                }

                if (photosToAdd.Any())
                {
                    context.Photoss.AddRange(photosToAdd);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"📸 Đã thêm {photosToAdd.Count} ảnh cho rooms!");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi cập nhật ảnh room: {ex.Message}");
            }
        }

        public static async Task UpdateAllPhotosAsync(ApplicationDbContext context)
        {
            Console.WriteLine("📸 === BẮT ĐẦU CẬP NHẬT HÌNH ẢNH ===");
            await UpdateHotelPhotosAsync(context);
            await UpdateRoomPhotosAsync(context);
            Console.WriteLine("📸 === HOÀN THÀNH CẬP NHẬT HÌNH ẢNH ===");
        }
    }
}