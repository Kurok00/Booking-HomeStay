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

        // ============================
        // 1️⃣ Trang Index - hiển thị danh sách + bộ lọc (với pagination)
        // ============================
        public async Task<IActionResult> Index(string? search, string? sortBy, DateTime? checkIn, DateTime? checkOut, string? city)
        {
            // 🔹 Lấy danh sách top city (hiển thị trên giao diện)
            ViewBag.TopCities = await _context.Hotels
                .Where(h => h.IsApproved == true)
                .GroupBy(h => h.City)
                .Select(g => new
                {
                    City = g.Key,
                    HotelCount = g.Count(),
                    Photo = g.SelectMany(h => h.Photoss).Select(p => p.Url).FirstOrDefault()
                })
                .OrderByDescending(g => g.HotelCount)
                .Take(6)
                .ToListAsync();

            // Load chỉ 15 hotels đầu tiên
            var hotels = await GetFilteredHotels(search, sortBy, checkIn, checkOut, city, 1, 15);

            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;
            ViewBag.SelectedCity = city;

            return View(hotels);
        }

        // ============================
        // 📌 API: Load thêm hotels (Lazy Loading)
        // ============================
        [HttpGet]
        public async Task<IActionResult> LoadMoreHotels(string? search, string? sortBy, DateTime? checkIn, DateTime? checkOut, string? city, int page = 1, int pageSize = 15)
        {
            var hotels = await GetFilteredHotels(search, sortBy, checkIn, checkOut, city, page, pageSize);
            return PartialView("_HotelCardsPartial", hotels);
        }

        // ============================
        // 🔧 Helper method để lọc hotels
        // ============================
        private async Task<List<Models.Hotel>> GetFilteredHotels(string? search, string? sortBy, DateTime? checkIn, DateTime? checkOut, string? city, int page, int pageSize)
        {
            var hotels = _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Photoss)
                .Where(h => h.Status == true && h.IsApproved == true)
                .AsQueryable();

            // 🔹 Lọc theo city (nếu được chọn)
            if (!string.IsNullOrWhiteSpace(city))
            {
                hotels = hotels.Where(h => h.City == city);
            }

            // 🔹 Tìm kiếm theo từ khóa
            if (!string.IsNullOrWhiteSpace(search))
            {
                hotels = hotels.Where(h =>
                    h.Name.Contains(search) ||
                    h.Address.Contains(search) ||
                    h.City.Contains(search) ||
                    h.Country.Contains(search));
            }

            // 🔹 Lọc theo ngày
            if (checkIn.HasValue && checkOut.HasValue)
            {
                hotels = hotels.Where(h => h.Rooms.Any(r =>
                    !_context.Bookings.Any(b =>
                        b.RoomId == r.RoomId &&
                        (checkIn < b.CheckOut && checkOut > b.CheckIn)
                    )
                ));
            }

            // 🔹 Sắp xếp
            hotels = sortBy switch
            {
                "price" => hotels.OrderBy(h => h.Rooms.Min(r => r.Price)),
                "location" => hotels.OrderBy(h => h.City),
                _ => hotels.OrderByDescending(h => h.CreatedAt)
            };

            // 🔹 Pagination
            return await hotels
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // ============================
        // 2️⃣ API: Gợi ý City (autocomplete)
        // ============================
        [HttpGet]
        public async Task<IActionResult> SuggestCity(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new object[0]);

            var cities = await _context.Hotels
                .Where(h => h.City.Contains(term))
                .GroupBy(h => h.City)
                .Select(g => new
                {
                    City = g.Key,
                    Photo = g.SelectMany(h => h.Photoss)
                             .Select(p => p.Url)
                             .FirstOrDefault() ?? "/images/default-destination.jpg"
                })
                .Take(10)
                .ToListAsync();

            return Json(cities);
        }


        [HttpGet]
        public async Task<IActionResult> SearchByCity(string city)
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.Photoss)
                .Where(h => h.City == city && (h.IsApproved == true) && (h.Status == true))
                .ToListAsync();

            return PartialView("_HotelListPartial", hotels);
        }

        // ============================
        // 4️⃣ Trang chi tiết khách sạn
        // ============================
        public async Task<IActionResult> Details(int id)
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

            if (hotel == null) return NotFound();

            // Lấy blogs liên quan đến hotel này
            var relatedBlogs = await _context.Blogs
                .Include(b => b.Reviewer)
                .Where(b => b.HotelId == id)
                .OrderByDescending(b => b.CreatedDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RelatedBlogs = relatedBlogs;

            return View(hotel);
        }

        // ============================
        // 5️⃣ Gợi ý khách sạn gần vị trí
        // ============================
        public IActionResult Nearby(double latitude, double longitude, double radiusKm = 5)
        {
            var hotels = _context.Hotels
                .Where(h => h.Latitude != null && h.Longitude != null)
                .AsEnumerable()
                .Where(h =>
                {
                    const double R = 6371; // bán kính trái đất km
                    var dLat = (Math.PI / 180) * (h.Latitude!.Value - latitude);
                    var dLon = (Math.PI / 180) * (h.Longitude!.Value - longitude);
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