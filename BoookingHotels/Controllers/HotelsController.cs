    using BoookingHotels.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Controllers
{
    public class HotelsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HotelsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, string sortBy, DateTime? checkIn, DateTime? checkOut)
        {
            var hotels = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Photoss)
                .Where(h=>h.Status == true && h.IsApproved == true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                hotels = hotels.Where(h =>
                    h.Name.Contains(search) || 
                    h.Address.Contains(search) ||
                    h.City.Contains(search));
            }

             if (checkIn.HasValue && checkOut.HasValue)
            {
                hotels = hotels.Where(h => h.Rooms.Any(r =>
                    !_context.Bookings.Any(b =>
                        b.RoomId == r.RoomId &&
                        (checkIn < b.CheckOut && checkOut > b.CheckIn)
                    )
                ));
            }

            hotels = sortBy switch
            {
                "price" => hotels.OrderBy(h => h.Rooms.Min(r => r.Price)),
                "location" => hotels.OrderBy(h => h.City),
                _ => hotels.OrderByDescending(h => h.CreatedAt)
            };

            return View(await hotels.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var hotel = await _context.Hotels
              .Include(h => h.Photoss) 
              .Include(h => h.Rooms)
                  .ThenInclude(r => r.Photos)
              .Include(h => h.Rooms)
                  .ThenInclude(r => r.RoomAmenities)
                      .ThenInclude(ra => ra.Amenity)
              .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null) return NotFound();

            return View(hotel);
        }

        public IActionResult Nearby(double latitude, double longitude, double radiusKm = 5)
        {
            var hotels = _context.Hotels
                .Where(h => h.Latitude != null && h.Longitude != null)
                .AsEnumerable()
                .Where(h =>
                {
                    var R = 6371; 
                    var dLat = (Math.PI / 180) * (h.Latitude.Value - latitude);
                    var dLon = (Math.PI / 180) * (h.Longitude.Value - longitude);
                    var lat1 = (Math.PI / 180) * latitude;
                    var lat2 = (Math.PI / 180) * h.Latitude.Value;

                    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                            Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
                    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    var distance = R * c;

                    return distance <= radiusKm;
                })
                .ToList();

            return View("Index", hotels); 
        }

    }
}
