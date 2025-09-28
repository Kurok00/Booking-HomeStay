using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BoookingHotels.Controllers
{
    [Authorize]
    public class VouchersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VouchersController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult MyVouchers()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var vouchers = _db.Vouchers
                .Where(v =>
                    v.IsActive &&
                    v.ExpiryDate >= DateTime.Now &&
                    (v.UserId == null || v.UserId == userId) &&
                    v.Quantity > 0
                )
                .OrderBy(v => v.ExpiryDate)
                .ToList();

            return View(vouchers);
        }
    }
}
