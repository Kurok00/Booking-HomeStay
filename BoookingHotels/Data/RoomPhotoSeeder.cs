using BoookingHotels.Models;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Data
{
    public class RoomPhotoSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random;

        // Danh sách URL hình ảnh phòng khách sạn từ Unsplash
        private readonly List<string> _roomPhotoUrls = new List<string>
        {
            // Luxury Hotel Rooms
            "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=800",
            "https://images.unsplash.com/photo-1618773928121-c32242e63f39?w=800",
            "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800",
            "https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800",
            "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800",
            
            // Modern Bedrooms
            "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800",
            "https://images.unsplash.com/photo-1540518614846-7eded433c457?w=800",
            "https://images.unsplash.com/photo-1598928506311-c55ded91a20c?w=800",
            "https://images.unsplash.com/photo-1595526114035-0d45ed16cfbf?w=800",
            "https://images.unsplash.com/photo-1571508601891-ca5e7a713859?w=800",
            
            // Boutique Hotel Rooms
            "https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=800",
            "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?w=800",
            "https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800",
            "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=800",
            "https://images.unsplash.com/photo-1591088398332-8a7791972843?w=800",
            
            // Resort Rooms
            "https://images.unsplash.com/photo-1584132967334-10e028bd69f7?w=800",
            "https://images.unsplash.com/photo-1560185127-6ed189bf02f4?w=800",
            "https://images.unsplash.com/photo-1596701062351-8c2c14d1fdd0?w=800",
            "https://images.unsplash.com/photo-1512918728675-ed5a9ecdebfd?w=800",
            "https://images.unsplash.com/photo-1519710164239-da123dc03ef4?w=800",
            
            // Suite Rooms
            "https://images.unsplash.com/photo-1587985064135-0366536eab42?w=800",
            "https://images.unsplash.com/photo-1616047006789-b7af5afb8c20?w=800",
            "https://images.unsplash.com/photo-1585412727339-54e4bae3bbf9?w=800",
            "https://images.unsplash.com/photo-1631049552240-59c37f38802b?w=800",
            "https://images.unsplash.com/photo-1616486338812-3dadae4b4ace?w=800",
            
            // Minimalist Rooms
            "https://images.unsplash.com/photo-1596178060810-0a67b2295f1e?w=800",
            "https://images.unsplash.com/photo-1563298723-dcfebaa392e3?w=800",
            "https://images.unsplash.com/photo-1484101403633-562f891dc89a?w=800",
            "https://images.unsplash.com/photo-1615529328331-f8917597711f?w=800",
            "https://images.unsplash.com/photo-1585412727339-54e4bae3bbf9?w=800",
            
            // Cozy Rooms
            "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
            "https://images.unsplash.com/photo-1590381105924-c72589b9ef3f?w=800",
            "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800",
            "https://images.unsplash.com/photo-1445991842772-097fea258e7b?w=800",
            "https://images.unsplash.com/photo-1566195992011-5f6b21e539ce?w=800",
            
            // Beach/Ocean View Rooms
            "https://images.unsplash.com/photo-1559508551-44bff1de756b?w=800",
            "https://images.unsplash.com/photo-1602002418082-a4443e081dd1?w=800",
            "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
            "https://images.unsplash.com/photo-1544986581-efac024faf62?w=800",
            "https://images.unsplash.com/photo-1570213489059-0aac6626cade?w=800",
            
            // City View Rooms
            "https://images.unsplash.com/photo-1598928636135-d146006ff4be?w=800",
            "https://images.unsplash.com/photo-1595515106969-1ce29566ff1c?w=800",
            "https://images.unsplash.com/photo-1573052905904-34ad8c27f0cc?w=800",
            "https://images.unsplash.com/photo-1600121848594-d8644e57abab?w=800",
            "https://images.unsplash.com/photo-1560624052-449f5ddf0c31?w=800",
            
            // Twin Rooms
            "https://images.unsplash.com/photo-1609766975112-c5f8e38c1a96?w=800",
            "https://images.unsplash.com/photo-1631049552057-403cdb8f0658?w=800",
            "https://images.unsplash.com/photo-1606402179428-a57976d71fa4?w=800",
            "https://images.unsplash.com/photo-1579952363873-27f3bade9f55?w=800",
            "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800"
        };

        public RoomPhotoSeeder(ApplicationDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        public async Task SeedRoomPhotosAsync()
        {
            Console.WriteLine("🖼️ === BẮT ĐẦU SEED PHOTOS CHO ROOMS ===");

            // Lấy tất cả rooms chưa có photos hoặc có ít hơn 3 photos
            var roomsNeedingPhotos = await _context.Rooms
                .Select(r => new
                {
                    Room = r,
                    PhotoCount = _context.Photoss.Count(p => p.RoomId == r.RoomId)
                })
                .Where(x => x.PhotoCount < 3)
                .ToListAsync();

            Console.WriteLine($"📊 Tìm thấy {roomsNeedingPhotos.Count} rooms cần thêm photos (có < 3 photos)");

            if (roomsNeedingPhotos.Count == 0)
            {
                Console.WriteLine("✅ Tất cả rooms đều đã có đủ 3 photos!");
                return;
            }

            int totalPhotosAdded = 0;

            foreach (var item in roomsNeedingPhotos)
            {
                var room = item.Room;
                var currentPhotoCount = item.PhotoCount;
                var photosToAdd = 3 - currentPhotoCount;

                if (photosToAdd <= 0) continue;

                Console.WriteLine($"  🏠 Thêm {photosToAdd} photos cho room: {room.Name} (RoomId: {room.RoomId})");

                // Lấy ngẫu nhiên các URL không trùng lặp
                var selectedUrls = GetRandomUniqueUrls(photosToAdd);

                int sortOrder = currentPhotoCount + 1;
                foreach (var url in selectedUrls)
                {
                    var photo = new Photos
                    {
                        RoomId = room.RoomId,
                        HotelId = null,
                        Url = url,
                        SortOrder = sortOrder++
                    };

                    _context.Photoss.Add(photo);
                    totalPhotosAdded++;
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"\n✅ Hoàn tất! Đã thêm tổng cộng {totalPhotosAdded} photos cho {roomsNeedingPhotos.Count} rooms!");
        }

        private List<string> GetRandomUniqueUrls(int count)
        {
            var shuffled = _roomPhotoUrls.OrderBy(x => _random.Next()).ToList();
            return shuffled.Take(count).ToList();
        }
    }
}
