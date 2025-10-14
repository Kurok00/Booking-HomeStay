using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BoookingHotels.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Details(int id)
        {
            var room = _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomAmenities!).ThenInclude(ra => ra.Amenity)
                .Include(r => r.Photos)
                .Include(r => r.Reviews!)
                    .ThenInclude(rv => rv.User)
                .Include(r => r.Reviews!)
                    .ThenInclude(rv => rv.Photos)
                .FirstOrDefault(r => r.RoomId == id);

            if (room == null) return NotFound();

            return View(room);
        }

    }
}
