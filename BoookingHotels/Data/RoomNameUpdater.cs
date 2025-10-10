using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Data
{
    public static class RoomNameUpdater
    {
        public static async Task UpdateRoomNamesAsync(ApplicationDbContext context)
        {
            try
            {
                // Lấy danh sách rooms với hotel liên quan
                var rooms = await context.Rooms
                    .Include(r => r.Hotel)
                    .Where(r => r.Hotel != null)
                    .ToListAsync();
                
                if (!rooms.Any())
                {
                    Console.WriteLine("❌ Không có room nào trong database!");
                    return;
                }

                Console.WriteLine($"🔄 Đang cập nhật tên cho {rooms.Count} phòng...");

                // Danh sách tên phòng theo loại khách sạn
                var roomTypes = new Dictionary<string, List<string>>
                {
                    ["Resort"] = new List<string>
                    {
                        "Ocean View Suite", "Garden Villa", "Beach Front Villa", "Pool Access Room", 
                        "Presidential Suite", "Executive Suite", "Deluxe Ocean Room", "Premier Garden View",
                        "Infinity Pool Villa", "Spa Treatment Suite", "Royal Pavilion", "Sunset Terrace Suite"
                    },
                    ["Hotel"] = new List<string>
                    {
                        "Executive Suite", "Business Deluxe", "Premium King Room", "Corner Suite",
                        "City View Room", "Superior Twin", "Grand Executive", "Signature Suite",
                        "Panoramic Suite", "Club Level Room", "Ambassador Suite", "Presidential Floor"
                    },
                    ["Villa"] = new List<string>
                    {
                        "Private Villa", "Garden Villa", "Hillside Villa", "Valley View Villa",
                        "Mountain Lodge", "Romantic Cottage", "Family Villa", "Luxury Retreat",
                        "Secluded Hideaway", "Panoramic Villa", "Executive Villa", "Heritage Villa"
                    },
                    ["Homestay"] = new List<string>
                    {
                        "Cozy Family Room", "Traditional Room", "Garden House", "Local Experience Room",
                        "Authentic Stay", "Heritage House", "Cultural Room", "Countryside Villa",
                        "Local Family Suite", "Traditional Villa", "Rustic Charm Room", "Village House"
                    },
                    ["Apartment"] = new List<string>
                    {
                        "Studio Apartment", "One Bedroom Suite", "Executive Apartment", "Penthouse Suite",
                        "Serviced Apartment", "City Apartment", "Modern Loft", "Luxury Residence",
                        "Urban Suite", "Contemporary Flat", "Downtown Apartment", "Skyline Apartment"
                    }
                };

                var bedTypes = new[] { "Single", "Double", "Queen", "King", "Twin", "Bunk" };
                var random = new Random();
                int updatedCount = 0;

                // Nhóm rooms theo hotel
                var roomsByHotel = rooms.GroupBy(r => r.Hotel);

                foreach (var hotelGroup in roomsByHotel)
                {
                    var hotel = hotelGroup.Key;
                    var hotelRooms = hotelGroup.ToList();
                    
                    // Xác định loại khách sạn từ tên
                    string hotelType = "Hotel"; // Default
                    foreach (var type in roomTypes.Keys)
                    {
                        if (hotel!.Name.Contains(type, StringComparison.OrdinalIgnoreCase))
                        {
                            hotelType = type;
                            break;
                        }
                    }

                    // Nếu không tìm thấy type trong tên, thử đoán từ tên hotel
                    if (hotelType == "Hotel")
                    {
                        if (hotel!.Name.Contains("Resort", StringComparison.OrdinalIgnoreCase) ||
                            hotel.Name.Contains("Spa", StringComparison.OrdinalIgnoreCase))
                            hotelType = "Resort";
                        else if (hotel.Name.Contains("Villa", StringComparison.OrdinalIgnoreCase))
                            hotelType = "Villa";
                        else if (hotel.Name.Contains("Homestay", StringComparison.OrdinalIgnoreCase) ||
                                hotel.Name.Contains("House", StringComparison.OrdinalIgnoreCase))
                            hotelType = "Homestay";
                        else if (hotel.Name.Contains("Apartment", StringComparison.OrdinalIgnoreCase) ||
                                hotel.Name.Contains("Residence", StringComparison.OrdinalIgnoreCase))
                            hotelType = "Apartment";
                    }

                    var availableRoomNames = new List<string>(roomTypes[hotelType]);

                    // Cập nhật từng room trong hotel
                    foreach (var room in hotelRooms)
                    {
                        string newRoomName;
                        string newBedType;

                        // Chọn tên room từ danh sách
                        if (availableRoomNames.Any())
                        {
                            var nameIndex = random.Next(0, availableRoomNames.Count);
                            newRoomName = availableRoomNames[nameIndex];
                            availableRoomNames.RemoveAt(nameIndex); // Tránh trùng lặp
                        }
                        else
                        {
                            // Nếu hết tên, tạo tên mới
                            newRoomName = $"{hotelType} Room {random.Next(100, 999)}";
                        }

                        // Chọn bed type ngẫu nhiên
                        newBedType = bedTypes[random.Next(0, bedTypes.Length)];

                        // Kiểm tra nếu cần update
                        bool needUpdate = room.Name != newRoomName || room.BedType != newBedType;

                        if (needUpdate)
                        {
                            var oldName = room.Name;
                            var oldBedType = room.BedType;
                            
                            room.Name = newRoomName;
                            room.BedType = newBedType;
                            
                            Console.WriteLine($"✅ {hotel!.Name}: '{oldName}' ({oldBedType}) → '{newRoomName}' ({newBedType})");
                            updatedCount++;
                        }
                    }
                }

                // Lưu thay đổi
                if (updatedCount > 0)
                {
                    await context.SaveChangesAsync();
                    Console.WriteLine($"🎉 Đã cập nhật thành công {updatedCount} phòng!");
                }
                else
                {
                    Console.WriteLine("ℹ️  Không có thay đổi nào cần cập nhật cho rooms!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi cập nhật tên room: {ex.Message}");
            }
        }

        public static async Task UpdateRoomPricesAsync(ApplicationDbContext context)
        {
            try
            {
                var rooms = await context.Rooms
                    .Include(r => r.Hotel)
                    .Where(r => r.Hotel != null)
                    .ToListAsync();

                if (!rooms.Any())
                {
                    Console.WriteLine("❌ Không có room nào để cập nhật giá!");
                    return;
                }

                Console.WriteLine($"🔄 Đang cập nhật giá cho {rooms.Count} phòng...");

                // Giá theo thành phố và loại room
                var priceRanges = new Dictionary<string, Dictionary<string, (int min, int max)>>
                {
                    ["Đà Lạt"] = new Dictionary<string, (int min, int max)>
                    {
                        ["Hotel"] = (800000, 2500000),
                        ["Resort"] = (1500000, 4500000),
                        ["Villa"] = (2000000, 6000000),
                        ["Homestay"] = (500000, 1200000),
                        ["Apartment"] = (600000, 1800000)
                    },
                    ["Vũng Tàu"] = new Dictionary<string, (int min, int max)>
                    {
                        ["Hotel"] = (700000, 2000000),
                        ["Resort"] = (1200000, 3800000),
                        ["Villa"] = (1800000, 5500000),
                        ["Homestay"] = (400000, 1000000),
                        ["Apartment"] = (500000, 1500000)
                    },
                    ["Phú Quốc"] = new Dictionary<string, (int min, int max)>
                    {
                        ["Hotel"] = (1000000, 3000000),
                        ["Resort"] = (2000000, 6000000),
                        ["Villa"] = (3000000, 8000000),
                        ["Homestay"] = (600000, 1500000),
                        ["Apartment"] = (800000, 2200000)
                    },
                    ["Đà Nẵng"] = new Dictionary<string, (int min, int max)>
                    {
                        ["Hotel"] = (900000, 2800000),
                        ["Resort"] = (1800000, 5200000),
                        ["Villa"] = (2500000, 7000000),
                        ["Homestay"] = (500000, 1300000),
                        ["Apartment"] = (700000, 2000000)
                    },
                    ["Nha Trang"] = new Dictionary<string, (int min, int max)>
                    {
                        ["Hotel"] = (800000, 2600000),
                        ["Resort"] = (1600000, 4800000),
                        ["Villa"] = (2200000, 6500000),
                        ["Homestay"] = (450000, 1100000),
                        ["Apartment"] = (600000, 1800000)
                    }
                };

                var random = new Random();
                int updatedCount = 0;

                foreach (var room in rooms)
                {
                    var city = room.Hotel!.City;
                    
                    // Xác định loại room
                    string roomType = "Hotel";
                    if (room.Name.Contains("Suite", StringComparison.OrdinalIgnoreCase) ||
                        room.Name.Contains("Villa", StringComparison.OrdinalIgnoreCase))
                        roomType = "Villa";
                    else if (room.Name.Contains("Resort", StringComparison.OrdinalIgnoreCase) ||
                             room.Name.Contains("Ocean", StringComparison.OrdinalIgnoreCase) ||
                             room.Name.Contains("Beach", StringComparison.OrdinalIgnoreCase))
                        roomType = "Resort";
                    else if (room.Name.Contains("Apartment", StringComparison.OrdinalIgnoreCase) ||
                             room.Name.Contains("Studio", StringComparison.OrdinalIgnoreCase))
                        roomType = "Apartment";
                    else if (room.Name.Contains("Cozy", StringComparison.OrdinalIgnoreCase) ||
                             room.Name.Contains("Traditional", StringComparison.OrdinalIgnoreCase))
                        roomType = "Homestay";

                    if (priceRanges.ContainsKey(city) && priceRanges[city].ContainsKey(roomType))
                    {
                        var (min, max) = priceRanges[city][roomType];
                        var newPrice = random.Next(min, max + 1);
                        
                        // Làm tròn về số đẹp (x000)
                        newPrice = (newPrice / 50000) * 50000;

                        if (room.Price != newPrice)
                        {
                            var oldPrice = room.Price;
                            room.Price = newPrice;
                            
                            Console.WriteLine($"💰 {room.Name}: {oldPrice:C0} → {newPrice:C0}");
                            updatedCount++;
                        }
                    }
                }

                if (updatedCount > 0)
                {
                    await context.SaveChangesAsync();
                    Console.WriteLine($"🎉 Đã cập nhật giá cho {updatedCount} phòng!");
                }
                else
                {
                    Console.WriteLine("ℹ️  Không có thay đổi giá nào cần cập nhật!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi cập nhật giá room: {ex.Message}");
            }
        }
    }
}