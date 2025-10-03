using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .Where(v => v.IsActive && v.UserId == null || v.UserId == userId)
                .Include(v => v.UsedVoucherIds) // Load navigation
                .OrderBy(v => v.ExpiryDate)
                .ToList();

            ViewData["Title"] = "My Vouchers";
            return View("MyVouchers", vouchers);
        }

        public IActionResult VoucherUsed()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var vouchers = _db.UserVouchers
                .Where(uv => uv.UserId == userId)
                .Select(uv => uv.Voucher)
                .OrderByDescending(v => v.ExpiryDate)
                .ToList();

            ViewData["Title"] = "Voucher đã dùng";
            return View("MyVouchers", vouchers);
        }

    }
}
