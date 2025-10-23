using BoookingHotels.Data;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Scripts
{
    /// <summary>
    /// Script để fix duplicate amenities trong database
    /// Chạy bằng cách gọi từ Program.cs hoặc tạo endpoint temporary
    /// </summary>
    public class FixDuplicateAmenities
    {
        public static async Task FixAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🔧 Starting to fix duplicate amenities...");

            // 1. Lấy tất cả amenities
            var allAmenities = await context.Amenities.ToListAsync();
            Console.WriteLine($"📊 Total amenities: {allAmenities.Count}");

            // 2. Group by Name và Icon để tìm duplicates
            var amenityGroups = allAmenities
                .GroupBy(a => new { a.Name, a.Icon })
                .Where(g => g.Count() > 1)
                .ToList();

            Console.WriteLine($"🔍 Found {amenityGroups.Count} duplicate groups");

            int totalRemoved = 0;

            foreach (var group in amenityGroups)
            {
                // Giữ amenity có ID nhỏ nhất (master)
                var master = group.OrderBy(a => a.AmenityId).First();
                var duplicates = group.Where(a => a.AmenityId != master.AmenityId).ToList();

                Console.WriteLine($"\n📌 Processing: {master.Name} ({master.Icon})");
                Console.WriteLine($"   Master ID: {master.AmenityId}");
                Console.WriteLine($"   Duplicates: {duplicates.Count}");

                foreach (var duplicate in duplicates)
                {
                    // Update tất cả RoomAmenities trỏ đến duplicate này sang master
                    var roomAmenities = await context.RoomAmenities
                        .Where(ra => ra.AmenityId == duplicate.AmenityId)
                        .ToListAsync();

                    foreach (var ra in roomAmenities)
                    {
                        ra.AmenityId = master.AmenityId;
                    }

                    Console.WriteLine($"   ↳ Updated {roomAmenities.Count} room amenities from ID {duplicate.AmenityId} to {master.AmenityId}");

                    // Xóa duplicate amenity
                    context.Amenities.Remove(duplicate);
                    totalRemoved++;
                }
            }

            // 3. Save changes
            await context.SaveChangesAsync();

            Console.WriteLine($"\n✅ Completed! Removed {totalRemoved} duplicate amenities");

            // 4. Verify không còn duplicates
            var finalAmenities = await context.Amenities.ToListAsync();
            var finalDuplicates = finalAmenities
                .GroupBy(a => new { a.Name, a.Icon })
                .Where(g => g.Count() > 1)
                .ToList();

            Console.WriteLine($"📊 Final amenities count: {finalAmenities.Count}");
            Console.WriteLine($"🔍 Remaining duplicates: {finalDuplicates.Count}");

            if (finalDuplicates.Count == 0)
            {
                Console.WriteLine("🎉 SUCCESS! No more duplicates!");
            }
            else
            {
                Console.WriteLine("⚠️ Warning: Still have duplicates!");
            }
        }
    }
}
