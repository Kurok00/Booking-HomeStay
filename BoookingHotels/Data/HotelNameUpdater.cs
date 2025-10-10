using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Data
{
    public static class HotelNameUpdater
    {
        public static async Task UpdateHotelNamesAsync(ApplicationDbContext context)
        {
            try
            {
                // Lấy danh sách hotels hiện tại
                var hotels = await context.Hotels.ToListAsync();
                
                if (!hotels.Any())
                {
                    Console.WriteLine("❌ Không có hotel nào trong database!");
                    return;
                }

                Console.WriteLine($"🔄 Đang cập nhật tên cho {hotels.Count} khách sạn...");

                // Danh sách tên khách sạn thực tế theo từng thành phố
                var hotelNames = new Dictionary<string, List<string>>
                {
                    ["Đà Lạt"] = new List<string>
                    {
                        "Dalat Palace Heritage Hotel",
                        "Ana Mandara Villas Dalat Resort & Spa", 
                        "Swiss-Belresort Tuyen Lam Dalat",
                        "TTC Hotel Premium Dalat",
                        "Sammy Dalat Hotel",
                        "Dalat Wonder Resort",
                        "Villa Pink House Dalat",
                        "Zen Valley Dalat",
                        "Dalat De Charme Village Resort",
                        "Artitaya Dalat Hotel"
                    },
                    ["Vũng Tàu"] = new List<string>
                    {
                        "Imperial Hotel Vung Tau",
                        "Pullman Vung Tau",
                        "Fusion Suites Vung Tau",
                        "Malibu Hotel Vung Tau",
                        "Palace Hotel Vung Tau",
                        "Seaside Resort Vung Tau",
                        "Petro House Hotel Vung Tau",
                        "Green Dragon Water Palace",
                        "Hai Au Boutique Hotel & Spa",
                        "Victory Hotel Vung Tau"
                    },
                    ["Phú Quốc"] = new List<string>
                    {
                        "JW Marriott Phu Quoc Emerald Bay Resort & Spa",
                        "InterContinental Phu Quoc Long Beach Resort",
                        "La Veranda Resort Phu Quoc",
                        "Salinda Resort Phu Quoc Island",
                        "Vinpearl Resort & Spa Phu Quoc",
                        "Regent Phu Quoc",
                        "Sol Beach House Phu Quoc",
                        "Chen Sea Resort & Spa Phu Quoc",
                        "Cassia Cottage Resort Phu Quoc",
                        "Famiana Resort & Spa Phu Quoc"
                    },
                    ["Đà Nẵng"] = new List<string>
                    {
                        "InterContinental Danang Sun Peninsula Resort",
                        "Pullman Danang Beach Resort",
                        "Fusion Maia Da Nang",
                        "Hyatt Regency Danang Resort and Spa",
                        "Premier Village Danang Resort",
                        "Sheraton Grand Danang Resort",
                        "Novotel Danang Premier Han River",
                        "Vinpearl Resort & Golf Nam Hoi An",
                        "TIA Wellness Resort Danang",
                        "Muong Thanh Luxury Danang Hotel"
                    },
                    ["Nha Trang"] = new List<string>
                    {
                        "InterContinental Nha Trang",
                        "Sheraton Nha Trang Hotel & Spa",
                        "Vinpearl Resort Nha Trang",
                        "Fusion Resort Cam Ranh",
                        "Diamond Bay Resort & Spa",
                        "Amiana Resort and Villas Nha Trang",
                        "Liberty Central Nha Trang Hotel",
                        "Sunrise Nha Trang Beach Hotel & Spa",
                        "Best Western Premier Havana Nha Trang",
                        "Muine Bay Resort Nha Trang"
                    }
                };

                int updatedCount = 0;

                // Cập nhật từng hotel theo city
                foreach (var hotel in hotels)
                {
                    var city = hotel.City;
                    
                    if (hotelNames.ContainsKey(city))
                    {
                        var cityHotels = hotelNames[city];
                        var random = new Random(hotel.HotelId); // Seed theo ID để consistent
                        
                        // Lấy tên ngẫu nhiên từ danh sách thành phố đó
                        var newName = cityHotels[random.Next(0, cityHotels.Count)];
                        
                        // Kiểm tra tên mới khác tên cũ
                        if (hotel.Name != newName)
                        {
                            var oldName = hotel.Name;
                            hotel.Name = newName;
                            
                            // Cập nhật description cho phù hợp
                            hotel.Description = GenerateRealisticDescription(newName, city);
                            
                            Console.WriteLine($"✅ Updated: '{oldName}' → '{newName}' ({city})");
                            updatedCount++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️  Không tìm thấy thành phố '{city}' trong danh sách!");
                    }
                }

                // Lưu thay đổi
                if (updatedCount > 0)
                {
                    await context.SaveChangesAsync();
                    Console.WriteLine($"🎉 Đã cập nhật thành công {updatedCount} khách sạn!");
                }
                else
                {
                    Console.WriteLine("ℹ️  Không có thay đổi nào cần cập nhật!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi cập nhật tên hotel: {ex.Message}");
            }
        }

        private static string GenerateRealisticDescription(string hotelName, string city)
        {
            var descriptions = new Dictionary<string, string[]>
            {
                ["Đà Lạt"] = new[]
                {
                    "Khách sạn sang trọng tọa lạc tại trung tâm thành phố Đà Lạt, với khí hậu mát mẻ quanh năm và phong cảnh thơ mộng.",
                    "Resort cao cấp nằm giữa núi đồi Đà Lạt, mang đến không gian nghỉ dưỡng yên bình với thiết kế châu Âu độc đáo.",
                    "Khách sạn boutique với view hồ Xuân Hương tuyệt đẹp, phong cách kiến trúc Pháp cổ điển giữa lòng thành phố ngàn hoa."
                },
                ["Vũng Tàu"] = new[]
                {
                    "Resort biển đẳng cấp quốc tế tại bãi Sau Vũng Tàu, với bãi cát trắng mịn và dịch vụ 5 sao.",
                    "Khách sạn view biển tuyệt đẹp, nằm ngay trung tâm thành phố biển Vũng Tàu, gần các điểm du lịch nổi tiếng.",
                    "Resort nghỉ dưỡng sang trọng bên bờ biển Vũng Tàu, kết hợp hoàn hảo giữa thiên nhiên và tiện nghi hiện đại."
                },
                ["Phú Quốc"] = new[]
                {
                    "Resort đảo nhiệt đới đẳng cấp thế giới tại Phú Quốc, với bãi biển riêng tư và dịch vụ spa cao cấp.",
                    "Khách sạn luxury trên đảo Ngọc, mang đến trải nghiệm nghỉ dưỡng hoàn hảo với thiết kế hiện đại và thiên nhiên hoang sơ.",
                    "Resort biển 5 sao tại Phú Quốc với villa riêng biệt, hướng ra bãi Sao tuyệt đẹp và rừng nguyên sinh."
                },
                ["Đà Nẵng"] = new[]
                {
                    "Resort biển cao cấp tại Đà Nẵng với thiết kế hiện đại, nằm trên bãi biển Mỹ Khê xinh đẹp.",
                    "Khách sạn 5 sao ven sông Hàn, kết hợp hoàn hảo giữa phong cách Việt Nam và tiêu chuẩn quốc tế.",
                    "Resort nghỉ dưỡng sang trọng tại thành phố Đà Nẵng, gần sân bay và các điểm tham quan nổi tiếng."
                },
                ["Nha Trang"] = new[]
                {
                    "Resort biển đẳng cấp quốc tế tại vịnh Nha Trang tuyệt đẹp, với bãi cát vàng và nước biển trong xanh.",
                    "Khách sạn luxury trung tâm Nha Trang, view trọn vịnh biển và gần các khu vui chơi giải trí.",
                    "Resort cao cấp bên bờ biển Nha Trang, mang đến không gian nghỉ dưỡng hoàn hảo cho kỳ nghỉ tại thiên đường biển đảo."
                }
            };

            if (descriptions.ContainsKey(city))
            {
                var cityDescriptions = descriptions[city];
                var random = new Random(hotelName.GetHashCode());
                return cityDescriptions[random.Next(0, cityDescriptions.Length)];
            }

            return $"Khách sạn cao cấp tại {city} với dịch vụ chuyên nghiệp và tiện nghi hiện đại.";
        }
    }
}