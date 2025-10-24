using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BoookingHotels.Data;
using BoookingHotels.Models;

namespace BoookingHotels.Scripts
{
    public static class BlogSeeder
    {
        public static async Task Seed(ApplicationDbContext context)
        {
            // Chỉ seed blog cho các host (Role = Host), không seed cho admin/user
            var hosts = await context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Host"))
                .ToListAsync();

            Console.WriteLine($"🔎 Có {hosts.Count} host");
            int totalHotels = 0, totalBlogs = 0, skippedHotels = 0;
            foreach (var host in hosts)
            {
                var hotels = await context.Hotels.Where(h => h.CreatedBy == host.UserId).ToListAsync();
                Console.WriteLine($"👤 Host {host.UserName} ({host.UserId}) có {hotels.Count} hotel");
                totalHotels += hotels.Count;
                foreach (var hotel in hotels)
                {
                    var city = hotel.City ?? "Việt Nam";
                    var title = $"Trải nghiệm tại {hotel.Name} - {city}";
                    var shortDesc = $"Review khách sạn {hotel.Name} tại {city}, điểm đến nổi bật cho du khách.";
                    var content = $"Khách sạn {hotel.Name} tọa lạc tại {city}, nổi tiếng với dịch vụ chuyên nghiệp, phòng nghỉ tiện nghi và vị trí thuận lợi gần các điểm du lịch nổi tiếng. Trong chuyến đi vừa qua, tôi đã có dịp trải nghiệm không gian sang trọng, ẩm thực đa dạng và đội ngũ nhân viên thân thiện. Đặc biệt, khách sạn nằm gần các địa danh nổi tiếng, giúp việc di chuyển và khám phá trở nên dễ dàng. Đây là lựa chọn lý tưởng cho kỳ nghỉ hoặc công tác tại {city}. Nếu bạn đang tìm kiếm một nơi lưu trú chất lượng tại {city}, hãy cân nhắc khách sạn {hotel.Name} cho chuyến đi tiếp theo của mình!";
                    var blog = await context.Blogs.FirstOrDefaultAsync(b => b.HotelId == hotel.HotelId && b.ReviewerId == host.UserId);
                    if (blog == null)
                    {
                        blog = new Blog
                        {
                            Title = title,
                            Content = content,
                            ShortDescription = shortDesc,
                            Author = host.FullName ?? host.UserName,
                            CreatedDate = DateTime.Now.AddDays(-new Random().Next(1, 60)),
                            ReviewerId = host.UserId,
                            HotelId = hotel.HotelId
                        };
                        await context.Blogs.AddAsync(blog);
                        totalBlogs++;
                        Console.WriteLine($"✅ Thêm blog cho hotel {hotel.HotelId} ({hotel.Name}) của host {host.UserName}");
                    }
                    else
                    {
                        blog.Title = title;
                        blog.Content = content;
                        blog.ShortDescription = shortDesc;
                        blog.Author = host.FullName ?? host.UserName;
                        blog.CreatedDate = DateTime.Now.AddDays(-new Random().Next(1, 60));
                        totalBlogs++;
                        Console.WriteLine($"♻️ Overwrite blog cho hotel {hotel.HotelId} ({hotel.Name}) của host {host.UserName}");
                    }
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"📊 Tổng số hotel của host: {totalHotels}, blog đã thêm: {totalBlogs}, hotel bị bỏ qua: {skippedHotels}");

            // Seed thumbnail images for blogs
            var blogsWithoutThumbnail = await context.Blogs.Where(b => b.Thumbnail == null || b.Thumbnail == "").ToListAsync();
            var vietnamTravelImages = new[]
            {
                // Vietnam travel destinations
                "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=800&q=80", // Ha Long Bay
                "https://images.unsplash.com/photo-1465101046530-73398c7f28ca?auto=format&fit=crop&w=800&q=80", // Sapa mountains
                "https://images.unsplash.com/photo-1519125323398-675f0ddb6308?auto=format&fit=crop&w=800&q=80", // Phu Quoc beach
                "https://images.unsplash.com/photo-1529626455594-4ff0802cfb7e?auto=format&fit=crop&w=800&q=80", // Hoi An ancient town
                "https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=800&q=80", // Da Lat night sky
                "https://images.unsplash.com/photo-1519817650390-64a93db511ed?auto=format&fit=crop&w=800&q=80", // Nha Trang beach
                "https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=800&q=80", // Mui Ne sand dunes
                "https://images.unsplash.com/photo-1467269204594-9661b134dd2b?auto=format&fit=crop&w=800&q=80", // Fansipan mountain
                "https://images.unsplash.com/photo-1509228468518-180dd4864904?auto=format&fit=crop&w=800&q=80", // Hanoi street
                "https://images.unsplash.com/photo-1521401830884-6c03c1c87ebb?auto=format&fit=crop&w=800&q=80", // Resort beach in Vietnam
                "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=800&q=80", // Ho Chi Minh City view
                "https://images.unsplash.com/photo-1465101178521-c1a4c8a0f8f9?auto=format&fit=crop&w=800&q=80", // Vietnamese hotel room
            };
            var rand = new Random();
            foreach (var blog in blogsWithoutThumbnail)
            {
                blog.Thumbnail = vietnamTravelImages[rand.Next(vietnamTravelImages.Length)];
                Console.WriteLine($"🖼️ Thêm thumbnail địa điểm du lịch Việt Nam cho blog {blog.BlogId} - {blog.Title}");
            }
            await context.SaveChangesAsync();
        }
    }
}
