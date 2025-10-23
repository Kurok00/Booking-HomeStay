using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.EntityFrameworkCore;

public class MissingRoomsUpdater
{
    private readonly ApplicationDbContext _context;
    private readonly Random _random;

    public MissingRoomsUpdater(ApplicationDbContext context)
    {
        _context = context;
        _random = new Random();
    }

    public async Task AddMissingRoomsAsync()
    {
        Console.WriteLine("🏨 === BẮT ĐẦU KIỂM TRA VÀ THÊM ROOMS CHO HOTELS ===");

        // Tìm các hotels chưa có rooms HOẶC có số lượng rooms <= 1
        var hotelsNeedingRooms = await _context.Hotels
            .Where(h => h.Status == true && h.IsApproved == true)
            .Select(h => new
            {
                Hotel = h,
                RoomCount = _context.Rooms.Count(r => r.HotelId == h.HotelId)
            })
            .Where(x => x.RoomCount <= 1)
            .ToListAsync();

        Console.WriteLine($"📊 Tìm thấy {hotelsNeedingRooms.Count} hotels cần thêm rooms (có 0 hoặc 1 room):");

        foreach (var item in hotelsNeedingRooms)
        {
            Console.WriteLine($"  - {item.Hotel.Name} (ID: {item.Hotel.HotelId}) tại {item.Hotel.City} - Hiện có: {item.RoomCount} room");
        }

        if (hotelsNeedingRooms.Count == 0)
        {
            Console.WriteLine("✅ Tất cả hotels đều đã có đủ rooms (>1 room)!");
            return;
        }

        Console.WriteLine("\n🏗️ Bắt đầu tạo 10 rooms cho mỗi hotel...");

        int totalRoomsAdded = 0;
        foreach (var item in hotelsNeedingRooms)
        {
            int roomsAdded = await CreateRoomsForHotel(item.Hotel, item.RoomCount);
            totalRoomsAdded += roomsAdded;
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"\n✅ Hoàn tất! Đã thêm tổng cộng {totalRoomsAdded} rooms cho {hotelsNeedingRooms.Count} hotels!");
    }

    private async Task<int> CreateRoomsForHotel(Hotel hotel, int existingRoomCount)
    {
        // Xác định loại khách sạn dựa trên tên
        var hotelType = GetHotelType(hotel.Name);
        var city = hotel.City;

        // Luôn tạo 10 rooms mới
        const int roomsToCreate = 10;

        Console.WriteLine($"  🏗️ Tạo {roomsToCreate} rooms cho {hotel.Name} (hiện có {existingRoomCount} room)...");

        for (int i = 0; i < roomsToCreate; i++)
        {
            // Số thứ tự room bắt đầu từ số lượng hiện có + 1
            var roomNumber = existingRoomCount + i + 1;
            var room = CreateRoom(hotel, hotelType, city, roomNumber);
            _context.Rooms.Add(room);
        }

        return roomsToCreate;
    }

    private Room CreateRoom(Hotel hotel, string hotelType, string city, int roomNumber)
    {
        var roomTypes = GetRoomTypes(hotelType);
        var selectedRoomType = roomTypes[_random.Next(roomTypes.Count)];

        var basePrice = GetBasePriceByCity(city);
        var roomPrice = CalculateRoomPrice(basePrice, selectedRoomType.PriceMultiplier);

        return new Room
        {
            HotelId = hotel.HotelId,
            Name = $"{selectedRoomType.Name} {roomNumber:D2}",
            Price = roomPrice,
            Capacity = selectedRoomType.Capacity,
            Size = selectedRoomType.Size,
            BedType = selectedRoomType.BedType,
            Status = true
        };
    }

    private string GetHotelType(string hotelName)
    {
        var name = hotelName.ToLower();

        if (name.Contains("resort") || name.Contains("beach"))
            return "Resort";
        else if (name.Contains("homestay") || name.Contains("villa"))
            return "Homestay";
        else if (name.Contains("hostel") || name.Contains("backpacker"))
            return "Hostel";
        else if (name.Contains("boutique") || name.Contains("luxury"))
            return "Boutique";
        else if (name.Contains("apartment") || name.Contains("studio"))
            return "Apartment";
        else
            return "Hotel";
    }

    private List<RoomTypeInfo> GetRoomTypes(string hotelType)
    {
        return hotelType switch
        {
            "Resort" => new List<RoomTypeInfo>
                {
                    new("Deluxe Ocean View", 2, 35, "King", 1.8m),
                    new("Family Suite", 4, 50, "2 Queen", 2.2m),
                    new("Presidential Villa", 6, 80, "King + Sofa", 3.5m),
                    new("Garden View", 2, 30, "Queen", 1.4m),
                    new("Pool Access", 3, 45, "King", 2.0m)
                },
            "Homestay" => new List<RoomTypeInfo>
                {
                    new("Cozy Family Room", 4, 25, "2 Single", 1.2m),
                    new("Private Double", 2, 20, "Double", 1.0m),
                    new("Shared Dorm", 6, 15, "3 Bunk", 0.6m),
                    new("Master Suite", 3, 35, "King", 1.5m)
                },
            "Hostel" => new List<RoomTypeInfo>
                {
                    new("Mixed Dorm 6", 6, 12, "3 Bunk", 0.4m),
                    new("Female Dorm 4", 4, 10, "2 Bunk", 0.5m),
                    new("Private Twin", 2, 15, "2 Single", 0.8m),
                    new("Capsule Pod", 1, 8, "Single", 0.6m)
                },
            "Boutique" => new List<RoomTypeInfo>
                {
                    new("Designer Suite", 2, 40, "King", 2.5m),
                    new("Artistic Loft", 3, 50, "Queen + Sofa", 2.8m),
                    new("Luxury Studio", 2, 35, "King", 2.2m),
                    new("Premium Balcony", 2, 30, "Queen", 2.0m)
                },
            "Apartment" => new List<RoomTypeInfo>
                {
                    new("1BR Apartment", 2, 40, "Queen", 1.5m),
                    new("2BR Family", 4, 60, "King + Twin", 2.0m),
                    new("Studio", 2, 25, "Double", 1.2m),
                    new("Penthouse", 6, 100, "King + Sofa", 3.0m)
                },
            _ => new List<RoomTypeInfo> // Hotel
                {
                    new("Standard Room", 2, 25, "Queen", 1.0m),
                    new("Superior Room", 2, 30, "King", 1.3m),
                    new("Deluxe Room", 3, 35, "King", 1.6m),
                    new("Junior Suite", 4, 45, "King + Sofa", 2.0m),
                    new("Executive Suite", 4, 55, "King + Sofa", 2.5m)
                }
        };
    }

    private decimal GetBasePriceByCity(string city)
    {
        return city switch
        {
            "Đà Lạt" => 800000m,      // Cao nhất do khí hậu mát mẻ
            "Phú Quốc" => 750000m,    // Cao do du lịch biển
            "Đà Nẵng" => 650000m,     // Trung bình cao
            "Nha Trang" => 600000m,   // Trung bình
            "Vũng Tàu" => 500000m,    // Thấp nhất do gần TP.HCM
            _ => 550000m              // Mặc định
        };
    }

    private decimal CalculateRoomPrice(decimal basePrice, decimal multiplier)
    {
        var price = basePrice * multiplier;

        // Làm tròn đến 50,000
        var rounded = Math.Round(price / 50000m) * 50000m;

        // Đảm bảo giá tối thiểu
        return Math.Max(rounded, 200000m);
    }

    private record RoomTypeInfo(string Name, int Capacity, int Size, string BedType, decimal PriceMultiplier);
}