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
                Photos = hotel.Photoss.Where(p => p.RoomId == null).Select(p => new
                {
                    p.PhotoId,
                    p.Url
                }).ToList(),
                Rooms = hotel.Rooms.Select(r => new
                {
                    r.RoomId,
                    r.Name,
                    r.Capacity,
                    r.BedType,
                    r.Size,
                    r.Price,
                    IsAvailable = r.Status,
                    MainPhoto = r.Photos != null && r.Photos.Any()
                        ? r.Photos.First().Url
                        : "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=400",
                    Photos = r.Photos != null ? r.Photos.Select(p => p.Url).ToList() : new List<string>(),
                    Amenities = (r.RoomAmenities != null && r.RoomAmenities.Any()
                        ? r.RoomAmenities.Where(ra => ra.Amenity != null)
                            .Select(ra => (object)new
                            {
                                ra.Amenity!.AmenityId,
                                ra.Amenity.Name,
                                ra.Amenity.Icon
                            })
                        : Enumerable.Empty<object>()).ToList(),
                    AvgRating = r.Reviews != null && r.Reviews.Any(rv => rv.IsVisible)
                        ? r.Reviews.Where(rv => rv.IsVisible).Average(rv => rv.Rating)
                        : 0,
                    ReviewCount = r.Reviews != null ? r.Reviews.Count(rv => rv.IsVisible) : 0
                }).ToList(),
                Reviews = hotel.Rooms
                    .SelectMany(r => r.Reviews ?? new List<Review>())
                    .Where(rv => rv.IsVisible)
                    .OrderByDescending(rv => rv.CreatedAt)
                    .Select(rv => new
                    {
                        rv.ReviewId,
                        rv.Rating,
                        rv.Comment,
                        rv.CreatedAt,
                        User = new
                        {
                            rv.User!.UserId,
                            rv.User.UserName,
                            rv.User.FullName,
                            rv.User.AvatarUrl
                        },
                        RoomName = rv.Room != null ? rv.Room.Name : ""
                    })
                    .Take(20)
                    .ToList(),
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
