using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BoookingHotels.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReviewsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ================== Thêm Review ==================
        [HttpPost]
        public IActionResult Create(int bookingId, int roomId, int rating, string comment, List<IFormFile>? photos)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Chỉ cho review nếu booking đã Paid
            var booking = _db.Bookings
                .FirstOrDefault(b => b.BookingId == bookingId && b.UserId == userId && b.Status == BookingStatus.Paid);
            if (booking == null)
                return Unauthorized();

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var review = new Review
                {
                    RoomId = booking.RoomId!.Value,   // lấy từ booking
                    HotelId = booking.HotelId,        // lấy từ booking
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };
                _db.Reviews.Add(review);
                _db.SaveChanges();

                if (photos != null && photos.Any())
                {
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "reviews");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    foreach (var img in photos)
                    {
                        if (img.Length > 0)
                        {
                            var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                            var filePath = Path.Combine(uploadFolder, fileName);

                            using var stream = new FileStream(filePath, FileMode.Create);
                            img.CopyTo(stream);

                            _db.ReviewPhotos.Add(new ReviewPhoto
                            {
                                ReviewId = review.ReviewId,
                                Url = "/Image/reviews/" + fileName
                            });
                        }
                    }
                    _db.SaveChanges();
                }

                transaction.Commit();
                TempData["Success"] = "Đánh giá của bạn đã được gửi!";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["Error"] = "Có lỗi xảy ra khi lưu đánh giá: " + ex.Message;
            }

            return RedirectToAction("MyBookings", "Bookings");
        }


        [AllowAnonymous]
        public IActionResult RoomReviews(int roomId)
        {
            var reviews = _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Photos)
                .Where(r => r.RoomId == roomId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(reviews);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var review = _db.Reviews.Include(r => r.Photos).FirstOrDefault(r => r.ReviewId == id);
            if (review == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (review.UserId != userId && !User.IsInRole("Admin"))
                return Unauthorized();

            // Xóa ảnh trong thư mục
            if (review.Photos != null)
            {
                foreach (var photo in review.Photos)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.Url.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
            }

            _db.Reviews.Remove(review);
            _db.SaveChanges();

            TempData["Success"] = "Review đã được xóa.";
            return RedirectToAction("MyBookings", "Bookings");
        }
        // ================== Danh sách Review của chính User ==================
        public IActionResult MyReviews()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var reviews = _db.Reviews
                .Include(r => r.Room).ThenInclude(r => r.Hotel)
                .Include(r => r.Photos)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(reviews);
        }

        // ================== Sửa Review ==================
        public IActionResult Edit(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var review = _db.Reviews
                .Include(r => r.Photos)
                .FirstOrDefault(r => r.ReviewId == id && r.UserId == userId);

            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost]
        public IActionResult Edit(Review model, List<IFormFile>? photos)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var review = _db.Reviews.Include(r => r.Photos)
                .FirstOrDefault(r => r.ReviewId == model.ReviewId && r.UserId == userId);

            if (review == null) return NotFound();

            review.Rating = model.Rating;
            review.Comment = model.Comment;
            review.CreatedAt = DateTime.Now;

            // Upload ảnh mới (không xóa ảnh cũ)
            if (photos != null && photos.Any())
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "reviews");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                foreach (var img in photos)
                {
                    if (img.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        img.CopyTo(stream);

                        _db.ReviewPhotos.Add(new ReviewPhoto
                        {
                            ReviewId = review.ReviewId,
                            Url = "/Image/reviews/" + fileName
                        });
                    }
                }
            }

            _db.SaveChanges();
            TempData["Success"] = "Review đã được cập nhật.";
            return RedirectToAction("MyReviews");
        }

    }
}