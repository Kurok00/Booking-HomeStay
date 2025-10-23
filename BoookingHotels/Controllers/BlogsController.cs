using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BoookingHotels.Controllers
{
    public class BlogsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public BlogsController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // Danh sách blog
        public IActionResult Index()
        {
            var blogs = _db.Blogs
                .Include(b => b.Hotel)
                .Include(b => b.Reviewer)
                .OrderByDescending(b => b.CreatedDate)
                .ToList();
            return View(blogs);
        }

        // Xem chi tiết blog
        public IActionResult Detail(int id)
        {
            var blog = _db.Blogs
                .Include(b => b.Hotel)
                .Include(b => b.Reviewer)
                .FirstOrDefault(b => b.BlogId == id);
            if (blog == null) return NotFound();
            return View(blog);
        }

        // GET: Blogs/Create - Cho cả Reviewer và Host
        [Authorize(Roles = "Reviewer,Host")]
        public IActionResult Create()
        {
            // Nếu là Host, lấy danh sách hotels của họ
            if (User.IsInRole("Host"))
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var myHotels = _db.Hotels
                    .Where(h => h.CreatedBy == userId)
                    .Select(h => new { h.HotelId, h.Name })
                    .ToList();
                ViewBag.MyHotels = myHotels;
            }
            return View();
        }

        // POST: Blogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reviewer,Host")]
        public IActionResult Create(Blog model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Host"))
                {
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    var myHotels = _db.Hotels
                        .Where(h => h.CreatedBy == userId)
                        .Select(h => new { h.HotelId, h.Name })
                        .ToList();
                    ViewBag.MyHotels = myHotels;
                }
                return View(model);
            }

            model.CreatedDate = DateTime.Now;

            // lấy user id từ claims
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId != null)
                model.ReviewerId = int.Parse(currentUserId);

            // lấy tên user làm Author
            model.Author = User.Identity?.Name ?? "User";

            // Nếu là Host và chọn hotel, verify hotel thuộc về họ
            if (User.IsInRole("Host") && model.HotelId.HasValue)
            {
                var userId = int.Parse(currentUserId!);
                var hotel = _db.Hotels.FirstOrDefault(h => h.HotelId == model.HotelId.Value && h.CreatedBy == userId);
                if (hotel == null)
                {
                    ModelState.AddModelError("HotelId", "Khách sạn không hợp lệ");
                    return View(model);
                }
            }

            // xử lý upload thumbnail
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "Image/blogs");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    imageFile.CopyTo(fileStream);
                }

                model.Thumbnail = "/Image/blogs/" + uniqueFileName;
            }
            else
            {
                model.Thumbnail = "/Image/blogs/default.jpg";
            }

            _db.Blogs.Add(model);
            _db.SaveChanges();

            TempData["success"] = "Đăng blog thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Blogs của tôi (cho Host)
        [Authorize(Roles = "Host")]
        public IActionResult MyBlogs()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var blogs = _db.Blogs
                .Include(b => b.Hotel)
                .Where(b => b.ReviewerId == userId)
                .OrderByDescending(b => b.CreatedDate)
                .ToList();
            return View(blogs);
        }
    }
}
