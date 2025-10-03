using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                .OrderByDescending(b => b.CreatedDate)
                .ToList();
            return View(blogs);
        }

        // Xem chi tiết blog
        public IActionResult Detail(int id)
        {
            var blog = _db.Blogs.FirstOrDefault(b => b.BlogId == id);
            if (blog == null) return NotFound();
            return View(blog);
        }

        // GET: Blogs/Create
        [Authorize(Roles = "Reviewer")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Blogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Reviewer")]
        public IActionResult Create(Blog model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.CreatedDate = DateTime.Now;

            // lấy user id từ claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
                model.ReviewerId = int.Parse(userId);

            // lấy tên reviewer làm Author
            model.Author = User.Identity?.Name ?? "Reviewer";

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
    }
}
