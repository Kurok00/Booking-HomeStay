using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Controllers
{
    [Authorize(Roles = "Host")]
    public class HostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================= HOTELS =============================

        // Danh sách khách sạn của host hiện tại
        public IActionResult MyHotels()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var hotels = _context.Hotels
                .Where(h => h.CreatedBy == userId && h.IsUserHostCreated == true)
                .OrderByDescending(h => h.CreatedAt)
                .ToList();

            return View(hotels);
        }

        // GET: Host/CreateHotel
        public IActionResult CreateHotel() => View();

        [HttpPost]
        public IActionResult CreateHotel(Hotel model, List<IFormFile> images)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            model.IsApproved = false; // pending admin
            model.IsUserHostCreated = true;
            model.IsUserHostCreatedDate = DateTime.Now;
            model.CreatedAt = DateTime.Now;
            model.CreatedBy = userId;
            model.Status = true;

            _context.Hotels.Add(model);
            _context.SaveChanges();

            // Upload ảnh khách sạn
            if (images != null && images.Count > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                foreach (var img in images)
                {
                    if (img?.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        img.CopyTo(stream);

                        _context.Photoss.Add(new Photos
                        {
                            HotelId = model.HotelId,
                            Url = "/Image/" + fileName,
                            SortOrder = 0
                        });
                    }
                }
                _context.SaveChanges();
            }

            TempData["info"] = "Khách sạn đã gửi yêu cầu. Vui lòng chờ Admin duyệt.";
            return RedirectToAction("MyHotels");
        }

        // ========================= ROOMS =============================

        // Danh sách phòng của host
        public IActionResult MyRooms()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var rooms = _context.Rooms
                .Include(r => r.Hotel)
                .Where(r => r.Hotel.CreatedBy == userId && r.Hotel.IsUserHostCreated == true)
                .OrderByDescending(r => r.RoomId)
                .ToList();

            return View(rooms);
        }

        // GET: Host/CreateRoom
        public IActionResult CreateRoom(int hotelId)
        {
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == hotelId);
            if (hotel == null || !hotel.IsApproved)
            {
                TempData["error"] = "Khách sạn chưa được duyệt, không thể thêm phòng.";
                return RedirectToAction("MyHotels");
            }

            ViewBag.Amenities = _context.Amenities.ToList();
            return View(new Room { HotelId = hotelId });
        }

        [HttpPost]
        public IActionResult CreateRoom(Room model, List<IFormFile> images, List<int> amenityIds)
        {
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == model.HotelId);
            if (hotel == null || !hotel.IsApproved)
            {
                TempData["error"] = "Khách sạn chưa được duyệt, không thể thêm phòng.";
                return RedirectToAction("MyHotels");
            }

            if (!ModelState.IsValid) return View(model);

            _context.Rooms.Add(model);
            _context.SaveChanges();

            // Amenities
            if (amenityIds != null)
            {
                foreach (var aid in amenityIds)
                {
                    _context.RoomAmenities.Add(new RoomAmenitie { RoomId = model.RoomId, AmenityId = aid });
                }
                _context.SaveChanges();
            }

            // Upload ảnh phòng
            if (images != null && images.Count > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                foreach (var img in images)
                {
                    if (img.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        img.CopyTo(stream);

                        _context.Photoss.Add(new Photos
                        {
                            RoomId = model.RoomId,
                            Url = "/Image/" + fileName,
                            SortOrder = 0
                        });
                    }
                }
                _context.SaveChanges();
            }

            TempData["success"] = "Phòng đã được tạo.";
            return RedirectToAction("MyHotels");
        }

        // GET: Host/EditRoom
        public IActionResult EditRoom(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var room = _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomAmenities)
                .FirstOrDefault(r => r.RoomId == id && r.Hotel.CreatedBy == userId);

            if (room == null)
            {
                TempData["error"] = "Không tìm thấy phòng hoặc bạn không có quyền sửa.";
                return RedirectToAction("MyRooms");
            }

            ViewBag.Amenities = _context.Amenities.ToList();
            ViewBag.SelectedAmenities = room.RoomAmenities.Select(a => a.AmenityId).ToList();

            return View(room);
        }

        [HttpPost]
        public IActionResult EditRoom(Room model, List<int> amenityIds, List<IFormFile> images)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var room = _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomAmenities)
                .FirstOrDefault(r => r.RoomId == model.RoomId && r.Hotel.CreatedBy == userId);

            if (room == null)
            {
                TempData["error"] = "Không tìm thấy phòng hoặc bạn không có quyền sửa.";
                return RedirectToAction("MyRooms");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Amenities = _context.Amenities.ToList();
                return View(model);
            }

            // Update room info
            room.Name = model.Name;
            room.Price = model.Price;
            room.Capacity = model.Capacity;

            // Update amenities
            _context.RoomAmenities.RemoveRange(room.RoomAmenities);
            if (amenityIds != null)
            {
                foreach (var aid in amenityIds)
                {
                    _context.RoomAmenities.Add(new RoomAmenitie { RoomId = room.RoomId, AmenityId = aid });
                }
            }

            // Upload new images
            if (images != null && images.Count > 0)
            {
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                foreach (var img in images)
                {
                    if (img.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        img.CopyTo(stream);

                        _context.Photoss.Add(new Photos
                        {
                            RoomId = room.RoomId,
                            Url = "/Image/" + fileName,
                            SortOrder = 0
                        });
                    }
                }
            }

            _context.SaveChanges();
            TempData["success"] = "Cập nhật phòng thành công!";
            return RedirectToAction("MyRooms");
        }

        // DELETE: Host/DeleteRoom
        public IActionResult DeleteRoom(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            var room = _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefault(r => r.RoomId == id && r.Hotel.CreatedBy == userId);

            if (room == null)
            {
                TempData["error"] = "Không tìm thấy phòng hoặc bạn không có quyền xóa.";
                return RedirectToAction("MyRooms");
            }

            _context.Rooms.Remove(room);
            _context.SaveChanges();

            TempData["success"] = "Đã xóa phòng thành công.";
            return RedirectToAction("MyRooms");
        }
    }
}
