using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HotelsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HotelsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetHotels(
            string? search,
            string? city,
            DateTime? checkIn,
            DateTime? checkOut,
            int page = 1,
            int pageSize = 10)
        {
            Console.WriteLine($"🔵 [HotelsApi] GET request received - Page: {page}, PageSize: {pageSize}, Search: {search}");
            var hotels = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Photoss)
                .Where(h => h.Status == true && h.IsApproved == true)
                .AsQueryable();

            // Filter by search
            if (!string.IsNullOrWhiteSpace(search))
            {
                hotels = hotels.Where(h =>
                    h.Name.Contains(search) ||
                    h.Address.Contains(search) ||
                    h.City.Contains(search) ||
                    h.Country.Contains(search));
            }

            // Filter by city
            if (!string.IsNullOrWhiteSpace(city))
            {
                hotels = hotels.Where(h => h.City == city);
            }

            // Filter by availability
            if (checkIn.HasValue && checkOut.HasValue)
            {
                hotels = hotels.Where(h => h.Rooms.Any(r =>
                    !_context.Bookings.Any(b =>
                        b.RoomId == r.RoomId &&
                        (checkIn < b.CheckOut && checkOut > b.CheckIn)
                    )
                ));
            }

            var totalCount = await hotels.CountAsync();

            var result = await hotels
                .OrderByDescending(h => h.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new
                {
                    h.HotelId,
                    h.Name,
                    h.Address,
                    h.City,
                    h.Country,
                    h.Description,
                    h.Latitude,
                    h.Longitude,
                    MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.Price) : 0,
                    MainPhoto = h.Photoss.FirstOrDefault(p => p.RoomId == null) != null
                        ? h.Photoss.FirstOrDefault(p => p.RoomId == null)!.Url
                        : "https://via.placeholder.com/400x300",
                    Photos = h.Photoss.Where(p => p.RoomId == null).Select(p => p.Url).ToList(),
                    RoomCount = h.Rooms.Count(),
                    AvgRating = h.Rooms
                        .Where(r => r.Reviews != null && r.Reviews.Any())
                        .SelectMany(r => r.Reviews!)
                        .Where(rv => rv.IsVisible)
                        .Average(rv => (double?)rv.Rating) ?? 0
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Data = result
            });
        }

        // GET: api/HotelsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetHotel(int id)
        {
            Console.WriteLine($"🔵 [HotelsApi] GET request for hotel ID: {id}");

            var hotel = await _context.Hotels
                .Include(h => h.Photoss)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Photos)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.RoomAmenities!)
                        .ThenInclude(ra => ra.Amenity)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Reviews!)
                        .ThenInclude(rv => rv.User)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
                return NotFound(new { message = "Hotel not found" });

            // Aggregate all unique amenities from all rooms (remove duplicates by Name + Icon)
            var roomAmenities = hotel.Rooms
                .SelectMany(r => r.RoomAmenities ?? new List<RoomAmenitie>())
                .Where(ra => ra.Amenity != null)
                .Select(ra => ra.Amenity!)
                .GroupBy(a => new { a.Name, a.Icon })  // Group by Name AND Icon to remove duplicates
                .Select(g => g.First())  // Take first from each group
                .OrderBy(a => a.Name)
                .ToList();

            var allAmenities = roomAmenities.Any()
                ? roomAmenities.Select(a => new
                {
                    AmenitiesId = a.AmenityId,
                    Name = a.Name,
                    IconUrl = a.Icon ?? "🏨"
                }).ToList()
                : new[]
                {
                    new { AmenitiesId = 1, Name = "WiFi", IconUrl = "📶" },
                    new { AmenitiesId = 2, Name = "Air Conditioning", IconUrl = "❄️" },
                    new { AmenitiesId = 3, Name = "Restaurant", IconUrl = "🍽️" },
                    new { AmenitiesId = 4, Name = "Parking", IconUrl = "🅿️" },
                    new { AmenitiesId = 5, Name = "Pool", IconUrl = "🏊" },
                    new { AmenitiesId = 6, Name = "Gym", IconUrl = "🏋️" }
                }.ToList();

            var result = new
            {
                hotel.HotelId,
                hotel.Name,
                hotel.Address,
                hotel.City,
                hotel.Country,
                hotel.Description,
                hotel.Latitude,
                hotel.Longitude,
                hotel.Status,
                hotel.CreatedAt,
                MinPrice = hotel.Rooms.Any() ? hotel.Rooms.Min(r => r.Price) : 0,
                RoomCount = hotel.Rooms.Count(),
                Photos = hotel.Photoss.Where(p => p.RoomId == null).Select(p => p.Url).ToList(),
                Amenities = allAmenities,
                AvgRating = hotel.Rooms.Any()
                    ? hotel.Rooms.SelectMany(r => r.Reviews ?? new List<Review>())
                        .Where(rv => rv.IsVisible)
                        .Average(rv => (double?)rv.Rating) ?? 0
                    : 0,
                TotalReviews = hotel.Rooms.Sum(r => r.Reviews != null ? r.Reviews.Count(rv => rv.IsVisible) : 0)
            };

            return Ok(result);
        }

        // GET: api/HotelsApi/cities
        [HttpGet("cities")]
        public async Task<ActionResult<IEnumerable<object>>> GetTopCities()
        {
            var cities = await _context.Hotels
                .Where(h => h.IsApproved == true && h.Status == true)
                .GroupBy(h => h.City)
                .Select(g => new
                {
                    City = g.Key,
                    HotelCount = g.Count(),
                    Photo = g.SelectMany(h => h.Photoss).Select(p => p.Url).FirstOrDefault()
                        ?? "https://via.placeholder.com/400x300"
                })
                .OrderByDescending(g => g.HotelCount)
                .Take(10)
                .ToListAsync();

            return Ok(cities);
        }

        // GET: api/HotelsApi/nearby?lat=10.762622&lng=106.660172&radius=5
        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<object>>> GetNearbyHotels(
            double lat,
            double lng,
            double radius = 5)
        {
            var hotels = await _context.Hotels
                .Include(h => h.Photoss)
                .Include(h => h.Rooms)
                .Where(h => h.Latitude != null && h.Longitude != null && h.Status == true && h.IsApproved == true)
                .ToListAsync();

            var nearbyHotels = hotels
                .Select(h => new
                {
                    Hotel = h,
                    Distance = CalculateDistance(lat, lng, h.Latitude!.Value, h.Longitude!.Value)
                })
                .Where(x => x.Distance <= radius)
                .OrderBy(x => x.Distance)
                .Select(x => new
                {
                    x.Hotel.HotelId,
                    x.Hotel.Name,
                    x.Hotel.Address,
                    x.Hotel.City,
                    x.Hotel.Country,
                    x.Hotel.Latitude,
                    x.Hotel.Longitude,
                    Distance = Math.Round(x.Distance, 2),
                    MinPrice = x.Hotel.Rooms.Any() ? x.Hotel.Rooms.Min(r => r.Price) : 0,
                    MainPhoto = x.Hotel.Photoss.FirstOrDefault(p => p.RoomId == null) != null
                        ? x.Hotel.Photoss.FirstOrDefault(p => p.RoomId == null)!.Url
                        : "https://via.placeholder.com/400x300"
                })
                .ToList();

            return Ok(nearbyHotels);
        }

        // GET: api/HotelsApi/5/rooms
        [HttpGet("{id}/rooms")]
        public async Task<ActionResult<IEnumerable<object>>> GetHotelRooms(int id)
        {
            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Photos)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.RoomAmenities!)
                        .ThenInclude(ra => ra.Amenity)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
                return NotFound();

            var rooms = hotel.Rooms.Select(r => new
            {
                r.RoomId,
                r.Name,
                r.Price,
                r.Capacity,
                r.BedType,
                r.Size,
                r.Status,
                Photos = r.Photos?.Select(p => p.Url).ToList() ?? new List<string>(),
                Amenities = r.RoomAmenities?.Select(ra => new
                {
                    ra.Amenity.AmenityId,
                    ra.Amenity.Name,
                    ra.Amenity.Icon
                }).ToList()
            }).ToList();

            return Ok(rooms);
        }

        // GET: api/HotelsApi/5/reviews
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<IEnumerable<object>>> GetHotelReviews(int id)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Photos)
                .Where(r => r.HotelId == id && r.IsVisible)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReviewId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    User = new
                    {
                        r.User.UserId,
                        r.User.UserName,
                        AvatarUrl = r.User.AvatarUrl
                    },
                    Photos = r.Photos.Select(p => p.Url).ToList()
                })
                .ToListAsync();

            return Ok(reviews);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            var dLat = (Math.PI / 180) * (lat2 - lat1);
            var dLon = (Math.PI / 180) * (lon2 - lon1);
            var lat1Rad = (Math.PI / 180) * lat1;
            var lat2Rad = (Math.PI / 180) * lat2;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1Rad) * Math.Cos(lat2Rad);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
