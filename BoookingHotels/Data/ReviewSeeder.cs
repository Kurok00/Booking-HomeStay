using BoookingHotels.Models;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Data
{
    public static class ReviewSeeder
    {
        public static async Task SeedReviewsAsync(ApplicationDbContext context)
        {
            Console.WriteLine("⭐ === BẮT ĐẦU SEED REVIEWS ===");


            // Xóa tất cả reviews cũ để tạo lại
            var existingReviews = await context.Reviews.ToListAsync();
            if (existingReviews.Any())
            {
                Console.WriteLine($"🗑️  Đang xóa {existingReviews.Count} reviews cũ...");
                context.Reviews.RemoveRange(existingReviews);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Đã xóa reviews cũ");
            }

            // Lấy tất cả users (AsNoTracking)
            var users = await context.Users.AsNoTracking().ToListAsync();
            if (users.Count < 5)
            {
                Console.WriteLine($"❌ Cần ít nhất 5 user để seed reviews (hiện có {users.Count})!");
                return;
            }

            // Lấy tất cả rooms (AsNoTracking)
            var rooms = await context.Rooms.AsNoTracking().ToListAsync();
            if (!rooms.Any())
            {
                Console.WriteLine("❌ Không có room nào để tạo reviews!");
                return;
            }

            var random = new Random();
            var reviews = new List<Review>();

            // Danh sách các comment mẫu theo rating
            var comments = new Dictionary<int, List<string>>
            {
                [5] = new List<string>
                {
                    "Phòng rất tuyệt vời! Sạch sẽ, đầy đủ tiện nghi. Nhân viên nhiệt tình. Chắc chắn sẽ quay lại!",
                    "Tuyệt vời! Vượt xa mong đợi. View đẹp, phòng rộng rãi. Highly recommended!",
                    "Hoàn hảo! Không có gì để chê. Dịch vụ 5 sao, phòng sang trọng, vị trí thuận lợi.",
                    "Cực kỳ hài lòng! Phòng đẹp như hình, sạch sẽ, tiện nghi đầy đủ. Sẽ giới thiệu cho bạn bè.",
                    "Quá tuyệt! Trải nghiệm tuyệt vời. Nhân viên thân thiện, phòng hiện đại, vị trí đắc địa.",
                    "Perfect! Phòng rộng, view đẹp, giường êm. Ăn sáng ngon. Rất đáng tiền!",
                    "Xuất sắc! Mọi thứ đều hoàn hảo. Sẽ quay lại lần sau khi đến đây.",
                    "Tốt nhất từng ở! Phòng đẹp, sạch sẽ, an ninh tốt. Highly recommended!",
                    "5 sao xứng đáng! Phục vụ tận tình, phòng sang trọng, vị trí thuận tiện.",
                    "Tuyệt vời không thể tả! Mọi thứ đều vượt mong đợi. Chắc chắn sẽ quay lại!"
                },
                [4] = new List<string>
                {
                    "Phòng tốt, sạch sẽ. Có một vài điểm nhỏ cần cải thiện nhưng nhìn chung ok.",
                    "Khá hài lòng. View đẹp, phòng rộng. Giá hơi cao một chút.",
                    "Tốt! Phòng sạch, tiện nghi đầy đủ. Wifi hơi yếu nhưng chấp nhận được.",
                    "Khá ổn! Nhân viên thân thiện. Phòng có mùi hơi nặng lúc đầu nhưng sau ok.",
                    "Không tệ! Vị trí tốt, phòng đẹp. Âm thanh cách âm chưa tốt lắm.",
                    "Tạm ổn. Giá hợp lý, phòng sạch. Có thể cải thiện thêm về dịch vụ.",
                    "Khá ok! Phòng đẹp nhưng hơi nhỏ so với mong đợi.",
                    "Tốt! Sạch sẽ, tiện nghi đầy đủ. Điều hòa hơi ồn một chút.",
                    "Ổn! View đẹp, vị trí tốt. Giá có vẻ hơi cao.",
                    "Khá hài lòng. Phòng tốt, nhân viên nhiệt tình. Bãi đỗ xe hơi xa."
                },
                [3] = new List<string>
                {
                    "Bình thường. Phòng sạch nhưng hơi cũ. Giá tương đối ok.",
                    "Ở được. Vị trí tốt nhưng phòng chưa được tốt lắm.",
                    "Tạm ổn. Sạch sẽ nhưng tiện nghi hơi cũ. Cần nâng cấp.",
                    "Trung bình. Wifi yếu, điều hòa không mát lắm.",
                    "Ok nhưng không xuất sắc. Giá hơi cao so với chất lượng.",
                    "Bình thường thôi. Phòng nhỏ, view không đẹp lắm.",
                    "Tạm được. Sạch sẽ nhưng cách âm kém. Ngủ hơi khó.",
                    "Ở được. Nhân viên ok nhưng phòng cần cải tạo lại.",
                    "Trung bình khá. Vị trí tốt nhưng tiện nghi cần nâng cấp.",
                    "Không tệ nhưng không tốt. Giá hơi cao."
                },
                [2] = new List<string>
                {
                    "Không được tốt lắm. Phòng chưa sạch, tiện nghi cũ.",
                    "Hơi thất vọng. Không giống hình. Cần cải thiện nhiều.",
                    "Dưới mong đợi. Phòng nhỏ, cũ. Wifi không hoạt động.",
                    "Chưa tốt. Nhân viên chưa nhiệt tình. Phòng cần vệ sinh kỹ hơn.",
                    "Không hài lòng lắm. Giá cao nhưng chất lượng thấp.",
                    "Tệ. Phòng ồn, cách âm kém. Khó ngủ.",
                    "Không được như kỳ vọng. Nhiều tiện nghi hỏng.",
                    "Chưa ổn. Phòng có mùi, điều hòa không mát.",
                    "Thất vọng. Không sạch sẽ. Sẽ không quay lại.",
                    "Dưới trung bình. Cần cải thiện rất nhiều."
                },
                [1] = new List<string>
                {
                    "Rất tệ! Phòng bẩn, tiện nghi hỏng. Không recommend!",
                    "Kinh khủng! Không giống hình. Phòng bẩn, mùi khó chịu.",
                    "Tệ nhất từng ở! Nhân viên thái độ, phòng dơ bẩn.",
                    "Không thể chấp nhận được! Phòng ồn cả đêm, không ngủ được.",
                    "Quá tệ! Tiền mất tật mang. Không bao giờ quay lại!",
                    "Thất vọng tràn trề! Mọi thứ đều tệ.",
                    "1 sao cũng còn thừa! Phòng bẩn, dịch vụ tệ.",
                    "Không thể tệ hơn! Không đáng đồng tiền nào.",
                    "Tệ hại! Phòng hư hỏng nhiều, nhân viên không quan tâm.",
                    "Kinh khủng! Không bao giờ quay lại nơi này nữa!"
                }
            };


            Console.WriteLine($"📝 Đang tạo 5 reviews cho mỗi room (tổng {rooms.Count} rooms)...");

            int skippedRooms = 0;
            foreach (var room in rooms)
            {
                // Nếu user < 5 thì bỏ qua room này
                if (users.Count < 5)
                {
                    skippedRooms++;
                    continue;
                }
                // Mỗi room có 5 reviews từ 5 users khác nhau
                var shuffledUsers = users.OrderBy(x => random.Next()).Take(5).ToList();

                for (int i = 0; i < 5; i++)
                {
                    var rating = random.Next(1, 6); // 1-5 stars
                    var commentList = comments[rating];
                    var comment = commentList[random.Next(commentList.Count)];

                    reviews.Add(new Review
                    {
                        RoomId = room.RoomId,
                        UserId = shuffledUsers[i].UserId,
                        Rating = rating,
                        Comment = comment,
                        CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365)) // Random date trong năm qua
                    });
                }
            }

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Đã tạo {reviews.Count} reviews cho {rooms.Count - skippedRooms} rooms (bỏ qua {skippedRooms} rooms do thiếu user)");
            Console.WriteLine("⭐ === HOÀN THÀNH SEED REVIEWS ===");
        }
    }
}
