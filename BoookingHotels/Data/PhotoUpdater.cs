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
                // Xóa photos cũ để thay bằng ảnh thực tế
                var existingPhotos = await context.Photoss.Where(p => p.HotelId != null).ToListAsync();
                if (existingPhotos.Any())
                {
                    context.Photoss.RemoveRange(existingPhotos);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"🗑️  Đã xóa {existingPhotos.Count} ảnh cũ");
                }

                // Lấy danh sách hotels
                var hotels = await context.Hotels.ToListAsync();
                
                if (!hotels.Any())
                {
                    Console.WriteLine("❌ Không có hotel nào để thêm ảnh!");
                    return;
                }

                Console.WriteLine($"🔄 Đang thêm ảnh cho {hotels.Count} khách sạn...");

                // URLs ảnh thực tế cho từng thành phố
                var hotelPhotos = new Dictionary<string, List<string>>
                {
                    ["Đà Lạt"] = new List<string>
                    {
                        "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
                        "https://images.unsplash.com/photo-1578884436442-d1d68d6175a4?w=800",
                        "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=800",
                        "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
                        "https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800",
                        "https://images.unsplash.com/photo-1581739725066-3bb95b516f87?w=800",
                        "https://images.unsplash.com/photo-1596436889106-be35e843f974?w=800",
                        "https://images.unsplash.com/photo-1561501900-3701fa6a0864?w=800"
                    },
                    ["Vũng Tàu"] = new List<string>
                    {
                        "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
                        "https://images.unsplash.com/photo-1571003123894-1f0594d2b5d9?w=800",
                        "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
                        "https://images.unsplash.com/photo-1540541338287-41700207dee6?w=800",
                        "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
                        "https://images.unsplash.com/photo-1578645510447-e20b4311e3ce?w=800",
                        "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800",
                        "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800"
                    },
                    ["Phú Quốc"] = new List<string>
                    {
                        "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
                        "https://images.unsplash.com/photo-1582880121574-da8cb1d08346?w=800",
                        "https://images.unsplash.com/photo-1578884436442-d1d68d6175a4?w=800",
                        "https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800",
                        "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
                        "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
                        "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
                        "https://images.unsplash.com/photo-1540541338287-41700207dee6?w=800"
                    },
                    ["Đà Nẵng"] = new List<string>
                    {
                        "https://images.unsplash.com/photo-1578884436442-d1d68d6175a4?w=800",
                        "https://images.unsplash.com/photo-1571003123894-1f0594d2b5d9?w=800",
                        "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
                        "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
                        "https://images.unsplash.com/photo-1596436889106-be35e843f974?w=800",
                        "https://images.unsplash.com/photo-1561501900-3701fa6a0864?w=800",
                        "https://images.unsplash.com/photo-1578662996442-48f60103fc96?w=800",
                        "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800"
                    },
                    ["Nha Trang"] = new List<string>
                    {
                        "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
                        "https://images.unsplash.com/photo-1582880121574-da8cb1d08346?w=800",
                        "https://images.unsplash.com/photo-1540541338287-41700207dee6?w=800",
                        "https://images.unsplash.com/photo-1578645510447-e20b4311e3ce?w=800",
                        "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
                        "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
                        "https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800",
                        "https://images.unsplash.com/photo-1578884436442-d1d68d6175a4?w=800"
                    }
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
                // Xóa room photos cũ
                var existingRoomPhotos = await context.Photoss.Where(p => p.RoomId != null).ToListAsync();
                if (existingRoomPhotos.Any())
                {
                    context.Photoss.RemoveRange(existingRoomPhotos);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"🗑️  Đã xóa {existingRoomPhotos.Count} ảnh phòng cũ");
                }

                // Lấy rooms với hotel
                var rooms = await context.Rooms
                    .Include(r => r.Hotel)
                    .Where(r => r.Hotel != null)
                    .ToListAsync();

                if (!rooms.Any())
                {
                    Console.WriteLine("❌ Không có room nào để thêm ảnh!");
                    return;
                }

                Console.WriteLine($"🔄 Đang thêm ảnh cho {rooms.Count} phòng...");

                // URLs ảnh phòng theo loại
                var roomPhotos = new Dictionary<string, List<string>>
                {
                    ["Suite"] = new List<string>
                    {
                        "https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800",
                        "https://images.unsplash.com/photo-1571624436279-b272aff752b5?w=800",
                        "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800",
                        "https://images.unsplash.com/photo-1578898886161-c4dd46c6c948?w=800"
                    },
                    ["Villa"] = new List<string>
                    {
                        "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
                        "https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800",
                        "https://images.unsplash.com/photo-1578884436442-d1d68d6175a4?w=800",
                        "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800"
                    },
                    ["Room"] = new List<string>
                    {
                        "https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800",
                        "https://images.unsplash.com/photo-1571624436279-b272aff752b5?w=800",
                        "https://images.unsplash.com/photo-1578898886161-c4dd46c6c948?w=800",
                        "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800"
                    },
                    ["Apartment"] = new List<string>
                    {
                        "https://images.unsplash.com/photo-1560184897-ae75f418493e?w=800",
                        "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800",
                        "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
                        "https://images.unsplash.com/photo-1574180045827-681f8a1a9622?w=800"
                    }
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