using BoookingHotels.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CheckCities()
        {
            var cities = await _context.Hotels
                .Where(h => h.IsApproved == true)
                .GroupBy(h => h.City)
                .Select(g => new
                {
                    City = g.Key,
                    HotelCount = g.Count(),
                    Hotels = g.Select(h => h.Name).ToList()
                })
                .OrderByDescending(g => g.HotelCount)
                .ToListAsync();

            ViewBag.Cities = cities;
            return View();
        }
    }
}