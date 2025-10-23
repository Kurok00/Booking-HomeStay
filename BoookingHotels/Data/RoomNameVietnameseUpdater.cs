using Microsoft.EntityFrameworkCore;
using BoookingHotels.Models;

namespace BoookingHotels.Data
{
    public static class RoomNameVietnameseUpdater
    {
        public static async Task UpdateAllRoomNamesToVietnameseAsync(ApplicationDbContext context)
        {
            try
            {
                Console.WriteLine("🇻🇳 === BẮT ĐẦU CÂP NHẬT TÊN PHÒNG SANG TIẾNG VIỆT ===");

                // Lấy tất cả rooms với hotel
                var rooms = await context.Rooms
                    .Include(r => r.Hotel)
                    .Where(r => r.Hotel != null)
                    .ToListAsync();

                if (!rooms.Any())
                {
                    Console.WriteLine("❌ Không có phòng nào trong database!");
                    return;
                }

                Console.WriteLine($"📊 Tìm thấy {rooms.Count} phòng cần cập nhật tên");

                var random = new Random();
                int updatedCount = 0;

                // Danh sách tên phòng tiếng Việt theo loại khách sạn
                var vietnameseRoomNames = new Dictionary<string, List<string>>
                {
                    ["Resort"] = new List<string>
                    {
                        "Phòng Hướng Biển Cao Cấp", "Villa Vườn Nhiệt Đới", "Villa Bãi Biển",
                        "Phòng Có Bể Bơi Riêng", "Căn Hộ Tổng Thống", "Phòng Suite Sang Trọng",
                        "Phòng Deluxe Hướng Biển", "Phòng Premier Vườn Xanh", "Villa Bể Bơi Vô Cực",
                        "Phòng Spa Cao Cấp", "Biệt Thự Hoàng Gia", "Suite Sân Thượng Hoàng Hôn",
                        "Phòng View Biển Panorama", "Villa Gia Đình", "Phòng Honeymoon Lãng Mạn"
                    },
                    ["Hotel"] = new List<string>
                    {
                        "Phòng Executive Suite", "Phòng Business Deluxe", "Phòng Premium King",
                        "Phòng Corner Suite", "Phòng Hướng Thành Phố", "Phòng Superior Giường Đôi",
                        "Phòng Grand Executive", "Phòng Signature Suite", "Phòng Panoramic Suite",
                        "Phòng Club Level", "Phòng Ambassador Suite", "Phòng Tầng Cao Cấp",
                        "Phòng Deluxe", "Phòng Standard Plus", "Phòng Family Suite"
                    },
                    ["Villa"] = new List<string>
                    {
                        "Biệt Thự Riêng Tư", "Biệt Thự Vườn", "Biệt Thự Sườn Đồi",
                        "Biệt Thự Hướng Thung Lũng", "Nhà Gỗ Núi", "Nhà Vườn Lãng Mạn",
                        "Biệt Thự Gia Đình", "Villa Nghỉ Dưỡng Sang Trọng", "Villa Ẩn Mình",
                        "Biệt Thự View Toàn Cảnh", "Villa Executive", "Villa Di Sản",
                        "Biệt Thự Cổ Điển", "Villa Hiện Đại", "Biệt Thự Hai Tầng"
                    },
                    ["Homestay"] = new List<string>
                    {
                        "Phòng Gia Đình Ấm Cúng", "Phòng Truyền Thống", "Nhà Vườn",
                        "Phòng Trải Nghiệm Địa Phương", "Căn Nhà Xưa", "Nhà Di Sản",
                        "Phòng Văn Hóa", "Villa Nông Thôn", "Phòng Gia Đình Địa Phương",
                        "Nhà Truyền Thống", "Phòng Mộc Mạc", "Nhà Làng Quê",
                        "Phòng Homestay Đôi", "Phòng Homestay Đơn", "Phòng Homestay Gia Đình"
                    },
                    ["Apartment"] = new List<string>
                    {
                        "Căn Hộ Studio", "Căn Hộ Một Phòng Ngủ", "Căn Hộ Executive",
                        "Căn Hộ Penthouse", "Căn Hộ Dịch Vụ", "Căn Hộ Thành Phố",
                        "Loft Hiện Đại", "Căn Hộ Cao Cấp", "Căn Hộ Đô Thị",
                        "Căn Hộ Contemporary", "Căn Hộ Trung Tâm", "Căn Hộ View Skyline",
                        "Căn Hộ Hai Phòng Ngủ", "Căn Hộ Ba Phòng Ngủ", "Căn Hộ Duplex"
                    },
                    ["Hostel"] = new List<string>
                    {
                        "Phòng Dorm 6 Giường", "Phòng Nữ 4 Giường", "Phòng Đôi Riêng Tư",
                        "Phòng Capsule", "Phòng Dorm 8 Giường", "Phòng Family 4 Người",
                        "Phòng Tập Thể", "Phòng Backpacker", "Phòng Shared Dorm",
                        "Phòng Private Twin", "Phòng Mixed Dorm", "Phòng Budget"
                    },
                    ["Boutique"] = new List<string>
                    {
                        "Phòng Designer Suite", "Loft Nghệ Thuật", "Phòng Luxury Studio",
                        "Phòng Premium Ban Công", "Phòng Boutique Deluxe", "Suite Thiết Kế",
                        "Phòng Artistic", "Phòng Unique Design", "Suite Phong Cách",
                        "Phòng Concept", "Phòng Signature Design", "Suite Contemporary"
                    }
                };

                // Danh sách bed types tiếng Việt
                var vietnameseBedTypes = new[]
                {
                    "Giường Đơn", "Giường Đôi", "Giường Queen", "Giường King",
                    "2 Giường Đơn", "Giường Tầng", "King + Sofa", "Queen + Sofa",
                    "2 Queen", "2 King", "3 Giường Đơn", "Giường Cỡ Lớn"
                };

                // Nhóm rooms theo hotel để đảm bảo tên không trùng trong cùng hotel
                var roomsByHotel = rooms.GroupBy(r => r.Hotel);

                foreach (var hotelGroup in roomsByHotel)
                {
                    var hotel = hotelGroup.Key;
                    var hotelRooms = hotelGroup.OrderBy(r => r.RoomId).ToList();

                    // Xác định loại khách sạn
                    string hotelType = DetermineHotelType(hotel!.Name);

                    // Lấy danh sách tên phòng cho loại hotel này
                    var availableNames = new List<string>(vietnameseRoomNames[hotelType]);

                    Console.WriteLine($"\n🏨 Cập nhật {hotelRooms.Count} phòng cho '{hotel.Name}' (Loại: {hotelType})");

                    int roomNumber = 1;
                    foreach (var room in hotelRooms)
                    {
                        string newRoomName;

                        // Chọn tên từ danh sách có sẵn
                        if (availableNames.Any())
                        {
                            var nameIndex = random.Next(0, availableNames.Count);
                            newRoomName = availableNames[nameIndex];
                            availableNames.RemoveAt(nameIndex); // Xóa để tránh trùng lặp
                        }
                        else
                        {
                            // Nếu hết tên, tạo tên có số thứ tự
                            newRoomName = $"Phòng {GetVietnameseRoomType(hotelType)} {roomNumber:D2}";
                        }

                        // Chọn bed type tiếng Việt
                        string newBedType = vietnameseBedTypes[random.Next(0, vietnameseBedTypes.Length)];

                        var oldName = room.Name;
                        var oldBedType = room.BedType;

                        room.Name = newRoomName;
                        room.BedType = newBedType;

                        Console.WriteLine($"  ✅ Phòng #{roomNumber}: '{oldName}' → '{newRoomName}' | '{oldBedType}' → '{newBedType}'");

                        updatedCount++;
                        roomNumber++;
                    }
                }

                // Lưu thay đổi
                await context.SaveChangesAsync();
                Console.WriteLine($"\n🎉 ĐÃ CẬP NHẬT THÀNH CÔNG {updatedCount} PHÒNG SANG TIẾNG VIỆT!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi cập nhật tên phòng: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static string DetermineHotelType(string hotelName)
        {
            var name = hotelName.ToLower();

            if (name.Contains("resort") || name.Contains("spa"))
                return "Resort";
            else if (name.Contains("villa") || name.Contains("biệt thự"))
                return "Villa";
            else if (name.Contains("homestay") || name.Contains("nhà nghỉ"))
                return "Homestay";
            else if (name.Contains("apartment") || name.Contains("căn hộ"))
                return "Apartment";
            else if (name.Contains("hostel") || name.Contains("backpacker"))
                return "Hostel";
            else if (name.Contains("boutique") || name.Contains("luxury"))
                return "Boutique";
            else
                return "Hotel";
        }

        private static string GetVietnameseRoomType(string hotelType)
        {
            return hotelType switch
            {
                "Resort" => "Resort",
                "Villa" => "Villa",
                "Homestay" => "Homestay",
                "Apartment" => "Căn Hộ",
                "Hostel" => "Dorm",
                "Boutique" => "Boutique",
                _ => "Deluxe"
            };
        }
    }
}
