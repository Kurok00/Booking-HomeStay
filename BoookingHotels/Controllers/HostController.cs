// ...existing code...

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

        public IActionResult MyHotels()
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["error"] = "Không xác định được người dùng.";
                return RedirectToAction("MyHotels");
            }
            var userId = int.Parse(userIdClaim.Value);
            var hotels = _context.Hotels
                .Where(h => h.CreatedBy == userId && h.IsUserHostCreated == true)
                .OrderByDescending(h => h.CreatedAt)
                .ToList();
            return View(hotels);
        }

        [HttpGet]
        public IActionResult EditHotel(int id)
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["error"] = "Không xác định được người dùng.";
                return RedirectToAction("MyHotels");
            }
            var userId = int.Parse(userIdClaim.Value);
            var hotel = _context.Hotels
                .Include(h => h.Photoss)
                .FirstOrDefault(h => h.HotelId == id && h.CreatedBy == userId);
            if (hotel == null)
            {
                TempData["error"] = "Không tìm thấy khách sạn hoặc bạn không có quyền sửa.";
                return RedirectToAction("MyHotels");
            }
            return View(hotel);
        }

        [HttpPost]
        public IActionResult EditHotel(Hotel model, List<IFormFile> images)
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["error"] = "Không xác định được người dùng.";
                return RedirectToAction("MyHotels");
            }
            var userId = int.Parse(userIdClaim.Value);
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == model.HotelId && h.CreatedBy == userId);
            if (hotel == null)
            {
                TempData["error"] = "Không tìm thấy khách sạn hoặc bạn không có quyền sửa.";
                return RedirectToAction("MyHotels");
            }
            if (!ModelState.IsValid) return View(model);

            hotel.Name = model.Name;
            hotel.Address = model.Address;
            hotel.City = model.City;
            hotel.Country = model.Country;
            hotel.Description = model.Description;
            hotel.Status = model.Status;
            hotel.Latitude = model.Latitude;
            hotel.Longitude = model.Longitude;

            // Handle deleted hotel images
            var deletedPhotoIds = Request.Form["deletedPhotoIds"].ToString();
            if (!string.IsNullOrEmpty(deletedPhotoIds))
            {
                var ids = deletedPhotoIds.Split(',').Select(x =>
                {
                    int.TryParse(x, out int idVal);
                    return idVal;
                }).Where(id => id > 0).ToList();
                if (ids.Count > 0)
                {
                    var photosToDelete = _context.Photoss.Where(p => ids.Contains(p.PhotoId) && p.HotelId == hotel.HotelId).ToList();
                    _context.Photoss.RemoveRange(photosToDelete);
                }
            }

            // Handle replaced hotel images
            var replacedPhotos = Request.Form.Files.Where(f => f.Name.StartsWith("replacedPhotos[")).ToList();
            foreach (var file in replacedPhotos)
            {
                var photoIdStr = file.Name.Replace("replacedPhotos[", "").Replace("]", "");
                if (int.TryParse(photoIdStr, out int photoId))
                {
                    var photo = _context.Photoss.FirstOrDefault(p => p.PhotoId == photoId && p.HotelId == hotel.HotelId);
                    if (photo != null && file.Length > 0)
                    {
                        var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadFolder, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        file.CopyTo(stream);
                        photo.Url = "/Image/" + fileName;
                    }
                }
            }
            // Handle new images
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
                            HotelId = hotel.HotelId,
                            Url = "/Image/" + fileName,
                            SortOrder = 0
                        });
                    }
                }
            }
            _context.SaveChanges();
            TempData["success"] = "Cập nhật khách sạn thành công!";
            return RedirectToAction("MyHotels");
        }

        public IActionResult CreateHotel() => View();

        [HttpPost]
        public IActionResult CreateHotel(Hotel model, List<IFormFile> images)
        {
            if (!ModelState.IsValid) return View(model);
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["error"] = "Không xác định được người dùng.";
                return RedirectToAction("MyHotels");
            }
            var userId = int.Parse(userIdClaim.Value);
            model.IsApproved = false;
            model.IsUserHostCreated = true;
            model.IsUserHostCreatedDate = DateTime.Now;
            model.CreatedAt = DateTime.Now;
            model.CreatedBy = userId;
            model.Status = true;
            _context.Hotels.Add(model);
            _context.SaveChanges();
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

        public IActionResult MyRooms()
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["error"] = "Không xác định được người dùng.";
                return RedirectToAction("MyHotels");
            }
            var userId = int.Parse(userIdClaim.Value);
            var rooms = _context.Rooms
                .Include(r => r.Hotel)
                .Where(r => r.Hotel != null && r.Hotel.CreatedBy == userId && r.Hotel.IsUserHostCreated == true)
                .OrderByDescending(r => r.RoomId)
                .ToList();
            return View(rooms);
        }

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
            if (amenityIds != null)
            {
                foreach (var aid in amenityIds)
                {
                    _context.RoomAmenities.Add(new RoomAmenitie { RoomId = model.RoomId, AmenityId = aid });
                }
                _context.SaveChanges();
            }
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
                            HotelId = model.HotelId,
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

        public IActionResult EditRoom(int id)
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["error"] = "Không xác định được người dùng.";
                return RedirectToAction("MyRooms");
            }
            var userId = int.Parse(userIdClaim.Value);
            var room = _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomAmenities)
                .FirstOrDefault(r => r.RoomId == id && r.Hotel != null && r.Hotel.CreatedBy == userId);
            if (room == null)
            {
                TempData["error"] = "Không tìm thấy phòng hoặc bạn không có quyền sửa.";
                return RedirectToAction("MyRooms");
            }
            ViewBag.Amenities = _context.Amenities.ToList();
            ViewBag.SelectedAmenities = room.RoomAmenities != null ? room.RoomAmenities.Select(a => a.AmenityId).ToList() : new List<int>();
            return View(room);
        }

        [HttpPost]
        public IActionResult EditRoom(Room model, List<int> amenityIds, List<IFormFile> images)
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["error"] = "Không xác định được người dùng.";
                return RedirectToAction("MyRooms");
            }
            var userId = int.Parse(userIdClaim.Value);
            var room = _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomAmenities)
                .FirstOrDefault(r => r.RoomId == model.RoomId && r.Hotel != null && r.Hotel.CreatedBy == userId);
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
            room.Name = model.Name;
            room.Price = model.Price;
            room.Capacity = model.Capacity;
            if (room.RoomAmenities != null)
            {
                _context.RoomAmenities.RemoveRange(room.RoomAmenities);
            }
            if (amenityIds != null)
            {
                foreach (var aid in amenityIds)
                {
                    _context.RoomAmenities.Add(new RoomAmenitie { RoomId = room.RoomId, AmenityId = aid });
                }
            }
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

        public IActionResult DeleteRoom(int id)
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["error"] = "Không xác định được người dùng.";
                return RedirectToAction("MyRooms");
            }
            var userId = int.Parse(userIdClaim.Value);
            var room = _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefault(r => r.RoomId == id && r.Hotel != null && r.Hotel.CreatedBy == userId);
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

        public IActionResult DetailHotel(int id)
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["error"] = "Không xác định được người dùng.";
                return RedirectToAction("MyHotels");
            }
            var userId = int.Parse(userIdClaim.Value);
            var hotel = _context.Hotels
                .Include(h => h.Photoss)
                .Include(h => h.Rooms)
                .FirstOrDefault(h => h.HotelId == id && h.CreatedBy == userId);
            if (hotel == null)
            {
                TempData["error"] = "Không tìm thấy khách sạn hoặc bạn không có quyền xem.";
                return RedirectToAction("MyHotels");
            }
            return View(hotel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult ReplaceHotelPhoto(int photoId, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image uploaded");

            var photo = _context.Photoss.FirstOrDefault(p => p.PhotoId == photoId);
            if (photo == null)
                return NotFound("Photo not found");

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
            var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }
            photo.Url = "/Image/" + fileName;
            _context.SaveChanges();
            return Json(new { success = true, url = photo.Url });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult DeleteHotelPhoto(int photoId)
        {
            var photo = _context.Photoss.FirstOrDefault(p => p.PhotoId == photoId);
            if (photo == null)
                return NotFound("Photo not found");
            _context.Photoss.Remove(photo);
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}
