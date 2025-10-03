using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoookingHotels.Controllers
{
    [Authorize(Roles = "Admin")]
    [ServiceFilter(typeof(AdminActionLogAttribute))]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context) => _context = context;
        #region Dashboard
        public IActionResult Index()
        {
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalHotels = _context.Hotels.Count();
            ViewBag.TotalRooms = _context.Rooms.Count();
            ViewBag.TotalAmenities = _context.Amenities.Count();
            ViewBag.TotalBookings = _context.Bookings.Count();
            ViewBag.TotalVouchers = _context.Vouchers.Count();

            ViewBag.PaidBookings = _context.Bookings.Count(b => b.Status == BookingStatus.Paid);
            ViewBag.PendingBookings = _context.Bookings.Count(b => b.Status == BookingStatus.Pending);
            ViewBag.CancelledBookings = _context.Bookings.Count(b => b.Status == BookingStatus.Canceled);

            var last7days = DateTime.Today.AddDays(-6);
            var bookingByDay = _context.Bookings
                .Where(b => b.CreatedAt >= last7days)
                .GroupBy(b => b.CreatedAt.Value.Date)
                .Select(g => new {
                    Date = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(g => g.Date)
                .ToList();

            ViewBag.BookingDates = bookingByDay.Select(x => x.Date.ToString("MM-dd")).ToArray();
            ViewBag.BookingCounts = bookingByDay.Select(x => x.Count).ToArray();
            ViewBag.BookingTotals = bookingByDay.Select(x => x.Total).ToArray();

            var logs = _context.AdminLogs
                .OrderByDescending(l => l.CreatedAt)
                .Take(1000)
                .ToList();
            ViewBag.AdminLogs = logs;

            return View();
        }
        #endregion

        #region Users
        public IActionResult Users()
        {
            var users = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToList();
            return View(users);
        }

        public IActionResult CreateUser() => View();

        [HttpPost]
        public IActionResult CreateUser(User model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Users.Add(model);
            _context.SaveChanges();
            TempData["success"] = "User created successfully!";
            return RedirectToAction("Users");
        }

        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(User model)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.Status = model.Status;
            _context.SaveChanges();

            TempData["success"] = "User updated!";
            return RedirectToAction("Users");
        }

        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefault(u => u.UserId == id);
            if (user != null)
            {
                _context.UserRoles.RemoveRange(user.UserRoles);
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["success"] = "User deleted!";
            }
            return RedirectToAction("Users");
        }

        public IActionResult EditUserRoles(int id)
        {
            var user = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null) return NotFound();

            ViewBag.AllRoles = _context.Roles.ToList();
            return View(user);
        }

        [HttpPost]
        public IActionResult EditUserRoles(int userId, List<int> roleIds)
        {
            var user = _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null) return NotFound();

            _context.UserRoles.RemoveRange(user.UserRoles);

            if (roleIds != null && roleIds.Any())
            {
                foreach (var roleId in roleIds)
                {
                    _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
                }
            }

            _context.SaveChanges();
            TempData["success"] = "Roles updated!";
            return RedirectToAction("Users");
        }
        #endregion

        #region Bookings
        public IActionResult ManageBookings()
        {
            var bookings = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Hotel)
                .Include(b => b.Room)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return View(bookings);
        }

        [HttpPost]
        public IActionResult AdminCancel(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.User)
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Paid)
            {
                booking.Status = BookingStatus.Canceled;
                _context.SaveChanges();

                var voucher = new Voucher
                {
                    Code = "ADM-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                    Description = $"Voucher bồi hoàn cho booking {booking.BookingId}",
                    DiscountType = "Percent",
                    DiscountValue = 10,
                    MinOrderValue = 500000,
                    ExpiryDate = DateTime.Now.AddMonths(1),
                    Quantity = 1,
                    IsActive = true
                };

                _context.Vouchers.Add(voucher);
                _context.SaveChanges();

                TempData["Success"] = $"Booking #{booking.BookingId} đã bị Admin hủy. Voucher {voucher.Code} đã được tặng cho khách.";
            }
            else if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
            {
                booking.Status = BookingStatus.Canceled;
                _context.SaveChanges();
                TempData["Info"] = $"Booking #{booking.BookingId} đã được hủy (chưa thanh toán nên không có voucher).";
            }

            return RedirectToAction("ManageBookings");
        }
        #endregion

        #region Hotels
        [HttpPost]
        public IActionResult ToggleApproveHotel(int id)
        {
            var hotel = _context.Hotels.Find(id);
            if (hotel == null) return NotFound();

            hotel.IsApproved = !hotel.IsApproved;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                isApproved = hotel.IsApproved,
                message = hotel.IsApproved ? "Khách sạn đã được duyệt!" : "Khách sạn đã bị gỡ duyệt!"
            });
        }
        [HttpPost]
        public IActionResult ApproveHotel(int id)
        {
            var hotel = _context.Hotels.Find(id);
            if (hotel == null) return NotFound();

            hotel.IsApproved = true;
            _context.SaveChanges();

            TempData["success"] = "Khách sạn đã được duyệt!";
            return RedirectToAction("Hotels");
        }
        [HttpPost]
        public IActionResult UnapproveHotel(int id)
        {
            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == id);
            if (hotel == null) return NotFound();

            hotel.IsApproved = false;
            _context.SaveChanges();
            TempData["info"] = $"Hotel {hotel.Name} đã chuyển về Pending.";
            return RedirectToAction("Hotels");
        }
        public IActionResult Hotels()
        {
            var hotels = _context.Hotels
                .Include(h => h.Photoss)
                .Include(h => h.Rooms)
                .OrderByDescending(h => h.CreatedAt)
                .ToList();
            return View(hotels);
        }

        public IActionResult CreateHotel()
        {
            var vm = new Hotel { Status = true, CreatedAt = DateTime.Now };
            return View(vm);
        }

        [HttpPost]
        public IActionResult CreateHotel(Hotel model, List<IFormFile> images)
        {
            if (!ModelState.IsValid) return View(model);

            model.IsApproved = true;
            model.IsUserHostCreated = null;
            model.IsUserHostCreatedDate = null;
            model.CreatedAt = DateTime.Now;
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
            return RedirectToAction("Hotels");
        }

        public IActionResult EditHotel(int id)
        {
            var hotel = _context.Hotels
                .Include(h => h.Photoss)
                .FirstOrDefault(h => h.HotelId == id);

            if (hotel == null) return NotFound();
            return View(hotel);
        }

        [HttpPost]
        public IActionResult EditHotel(Hotel model, List<IFormFile> images)
        {
            if (!ModelState.IsValid) return View(model);

            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == model.HotelId);
            if (hotel == null) return NotFound();

            hotel.Name = model.Name;
            hotel.Description = model.Description;
            hotel.Address = model.Address;
            hotel.City = model.City;
            hotel.Country = model.Country;
            hotel.Latitude = model.Latitude;
            hotel.Longitude = model.Longitude;
            hotel.Status = true;
            hotel.CreatedAt = model.CreatedAt ?? hotel.CreatedAt;
            _context.SaveChanges();

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
                _context.SaveChanges();
            }
            return RedirectToAction("Hotels");
        }

        public IActionResult DeleteHotel(int id)
        {
            var hotel = _context.Hotels.Find(id);
            if (hotel != null)
            {
                hotel.Status = false;
                _context.Hotels.Update(hotel);
                _context.SaveChanges();
            }
            return RedirectToAction("Hotels");
        }
        #endregion

        #region Rooms
        public IActionResult Rooms()
        {
            var rooms = _context.Rooms.Include(r => r.Hotel).ToList();
            return View(rooms);
        }

        public IActionResult CreateRoom()
        {
            ViewBag.Hotels = _context.Hotels.ToList();
            ViewBag.Amenities = _context.Amenities.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult CreateRoom(Room model, List<IFormFile> images, List<int> amenityIds)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Hotels = _context.Hotels.ToList();
                return View(model);
            }

            _context.Rooms.Add(model);
            _context.SaveChanges();

            if (amenityIds != null && amenityIds.Count > 0)
            {
                foreach (var amenityId in amenityIds)
                {
                    _context.RoomAmenities.Add(new RoomAmenitie { RoomId = model.RoomId, AmenityId = amenityId });
                }
                _context.SaveChanges();
            }

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
                            RoomId = model.RoomId,
                            Url = "/Image/" + fileName,
                            SortOrder = 0
                        });
                    }
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Rooms");
        }

        public IActionResult EditRoom(int id)
        {
            var room = _context.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
                .Include(r => r.Photos)
                .FirstOrDefault(r => r.RoomId == id);

            if (room == null) return NotFound();

            ViewBag.Hotels = _context.Hotels.ToList();
            ViewBag.Amenities = _context.Amenities.ToList();
            return View(room);
        }

        [HttpPost]
        public IActionResult EditRoom(Room model, List<IFormFile> images, List<int> amenityIds)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Hotels = _context.Hotels.ToList();
                ViewBag.Amenities = _context.Amenities.ToList();
                return View(model);
            }

            _context.Rooms.Update(model);
            _context.SaveChanges();

            var existingAmenities = _context.RoomAmenities.Where(ra => ra.RoomId == model.RoomId);
            _context.RoomAmenities.RemoveRange(existingAmenities);

            if (amenityIds != null && amenityIds.Any())
            {
                foreach (var amenityId in amenityIds)
                {
                    _context.RoomAmenities.Add(new RoomAmenitie { RoomId = model.RoomId, AmenityId = amenityId });
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
                            RoomId = model.RoomId,
                            Url = "/Image/" + fileName,
                            SortOrder = 0
                        });
                    }
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Rooms");
        }

        public IActionResult DeleteRoom(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                _context.SaveChanges();
            }
            return RedirectToAction("Rooms");
        }
        #endregion

        #region Amenities
        public IActionResult Amenities()
        {
            var amenities = _context.Amenities.ToList();
            return View(amenities);
        }

        public IActionResult CreateAmenity() => View();

        [HttpPost]
        public IActionResult CreateAmenity(Amenities model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Amenities.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Amenities");
        }

        public IActionResult EditAmenity(int id)
        {
            var amenity = _context.Amenities.Find(id);
            if (amenity == null) return NotFound();
            return View(amenity);
        }

        [HttpPost]
        public IActionResult EditAmenity(Amenities model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Amenities.Update(model);
            _context.SaveChanges();
            return RedirectToAction("Amenities");
        }

        public IActionResult DeleteAmenity(int id)
        {
            var amenity = _context.Amenities.Find(id);
            if (amenity != null)
            {
                _context.Amenities.Remove(amenity);
                _context.SaveChanges();
            }
            return RedirectToAction("Amenities");
        }
        #endregion

        #region Vouchers
        public IActionResult Vouchers()
        {
            var vouchers = _context.Vouchers.Include(v => v.User).ToList();
            return View(vouchers);
        }

        public IActionResult CreateVoucher()
        {
            ViewBag.Users = _context.Users.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult CreateVoucher(Voucher model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Vouchers.Add(model);
            _context.SaveChanges();
            TempData["success"] = "Voucher created!";
            return RedirectToAction("Vouchers");
        }

        public IActionResult EditVoucher(int id)
        {
            var voucher = _context.Vouchers.Find(id);
            if (voucher == null) return NotFound();
            ViewBag.Users = _context.Users.ToList();
            return View(voucher);
        }

        [HttpPost]
        public IActionResult EditVoucher(Voucher model)
        {
            if (!ModelState.IsValid) return View(model);
            _context.Vouchers.Update(model);
            _context.SaveChanges();
            TempData["success"] = "Voucher updated!";
            return RedirectToAction("Vouchers");
        }

        public IActionResult DeleteVoucher(int id)
        {
            var voucher = _context.Vouchers.Find(id);
            if (voucher != null)
            {
                _context.Vouchers.Remove(voucher);
                _context.SaveChanges();
                TempData["success"] = "Voucher deleted!";
            }
            return RedirectToAction("Vouchers");
        }
        #endregion
    }
}
//        #region
//        public IActionResult Index()
//        {
//            ViewBag.TotalUsers = _context.Users.Count();
//            ViewBag.TotalHotels = _context.Hotels.Count();
//            ViewBag.TotalRooms = _context.Rooms.Count();
//            ViewBag.TotalAmenities = _context.Amenities.Count();
//            ViewBag.TotalBookings = _context.Bookings.Count();
//            ViewBag.TotalVouchers = _context.Vouchers.Count();

//            ViewBag.PaidBookings = _context.Bookings.Count(b => b.Status == BookingStatus.Paid);
//            ViewBag.PendingBookings = _context.Bookings.Count(b => b.Status == BookingStatus.Pending);
//            ViewBag.CancelledBookings = _context.Bookings.Count(b => b.Status == BookingStatus.Canceled);

//            var last7days = DateTime.Today.AddDays(-6);
//            var bookingByDay = _context.Bookings
//                .Where(b => b.CreatedAt >= last7days)
//                .GroupBy(b => b.CreatedAt.Value.Date)
//                .Select(g => new {
//                    Date = g.Key,
//                    Count = g.Count(),
//                    Total = g.Sum(x => x.Total)
//                })
//                .OrderBy(g => g.Date)
//                .ToList();

//            ViewBag.BookingDates = bookingByDay.Select(x => x.Date.ToString("MM-dd")).ToArray();
//            ViewBag.BookingCounts = bookingByDay.Select(x => x.Count).ToArray();
//            ViewBag.BookingTotals = bookingByDay.Select(x => x.Total).ToArray();
//            var logs = _context.AdminLogs
//            .OrderByDescending(l => l.CreatedAt)
//            .Take(1000)  
//            .ToList();
//            ViewBag.AdminLogs = logs;
//            return View();
//        }

//        public IActionResult Users()
//        {
//            var users = _context.Users
//                .Include(u => u.UserRoles)
//                .ThenInclude(ur => ur.Role)
//                .ToList();
//            return View(users);
//        }

//        public IActionResult CreateUser() => View();
//        [HttpPost]
//        public IActionResult CreateUser(User model)
//        {
//            if (!ModelState.IsValid) return View(model);
//            _context.Users.Add(model);
//            _context.SaveChanges();
//            TempData["success"] = "User created successfully!";
//            return RedirectToAction("Users");
//        }

//        public IActionResult EditUser(int id)
//        {
//            var user = _context.Users.Find(id);
//            if (user == null) return NotFound();
//            return View(user);
//        }
//        [HttpPost]
//        public IActionResult EditUser(User model)
//        {

//            var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId);
//            if (user == null) return NotFound();

//            user.UserName = model.UserName;
//            user.FullName = model.FullName;
//            user.Phone = model.Phone;
//            user.Status = model.Status;

//            _context.SaveChanges();

//            TempData["success"] = "User updated!";
//            return RedirectToAction("Users");
//        }

//        [Authorize(Roles = "Admin")]
//        public IActionResult ManageBookings()
//        {
//            var bookings = _context.Bookings
//                .Include(b => b.User)
//                .Include(b => b.Hotel)
//                .Include(b => b.Room)
//                .OrderByDescending(b => b.CreatedAt)
//                .ToList();

//            return View(bookings);
//        }

//        public IActionResult DeleteUser(int id)
//        {
//            var user = _context.Users
//                .Include(u => u.UserRoles)
//                .FirstOrDefault(u => u.UserId == id);
//            if (user != null)
//            {
//                _context.UserRoles.RemoveRange(user.UserRoles);
//                _context.Users.Remove(user);
//                _context.SaveChanges();
//                TempData["success"] = "User deleted!";
//            }
//            return RedirectToAction("Users");
//        }

//        public IActionResult EditUserRoles(int id)
//        {
//            var user = _context.Users
//                .Include(u => u.UserRoles)
//                .ThenInclude(ur => ur.Role)
//                .FirstOrDefault(u => u.UserId == id);

//            if (user == null) return NotFound();

//            ViewBag.AllRoles = _context.Roles.ToList();
//            return View(user);
//        }

//        [HttpPost]
//        public IActionResult EditUserRoles(int userId, List<int> roleIds)
//        {
//            var user = _context.Users
//                .Include(u => u.UserRoles)
//                .FirstOrDefault(u => u.UserId == userId);

//            if (user == null) return NotFound();

//            _context.UserRoles.RemoveRange(user.UserRoles);

//            if (roleIds != null && roleIds.Any())
//            {
//                foreach (var roleId in roleIds)
//                {
//                    _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
//                }
//            }

//            _context.SaveChanges();
//            TempData["success"] = "Roles updated!";
//            return RedirectToAction("Users");
//        }

//        public IActionResult Hotels()
//        {
//            var hotels = _context.Hotels
//                .Include(h => h.Photoss)
//                .Include(h => h.Rooms)
//                .OrderByDescending(h => h.CreatedAt)
//                .ToList();
//            return View(hotels);
//        }

//        public IActionResult CreateHotel()
//        {
//            var vm = new Hotel
//            {
//                Status = true,
//                CreatedAt = DateTime.Now   
//            };
//            return View(vm);
//        }

//        [HttpPost]
//        public IActionResult CreateHotel(Hotel model, List<IFormFile> images)
//        {
//            if (!ModelState.IsValid) return View(model);

//            model.Status ??= true;
//            model.CreatedAt ??= DateTime.Now; 

//            _context.Hotels.Add(model);
//            _context.SaveChanges();

//            if (images != null && images.Count > 0)
//            {
//                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
//                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

//                foreach (var img in images)
//                {
//                    if (img?.Length > 0)
//                    {
//                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
//                        var filePath = Path.Combine(uploadFolder, fileName);
//                        using var stream = new FileStream(filePath, FileMode.Create);
//                        img.CopyTo(stream);

//                        _context.Photoss.Add(new Photos
//                        {
//                            HotelId = model.HotelId,
//                            Url = "/Image/" + fileName,
//                            SortOrder = 0
//                        });
//                    }
//                }
//                _context.SaveChanges();
//            }

//            return RedirectToAction("Hotels");
//        }

//        public IActionResult EditHotel(int id)
//        {
//            var hotel = _context.Hotels
//                .Include(h => h.Photoss)
//                .FirstOrDefault(h => h.HotelId == id);

//            if (hotel == null) return NotFound();
//            return View(hotel);
//        }

//        [HttpPost]
//        public IActionResult EditHotel(Hotel model, List<IFormFile> images)
//        {
//            if (!ModelState.IsValid) return View(model);

//            var hotel = _context.Hotels.FirstOrDefault(h => h.HotelId == model.HotelId);
//            if (hotel == null) return NotFound();

//            hotel.Name = model.Name;
//            hotel.Description = model.Description;
//            hotel.Address = model.Address;
//            hotel.City = model.City;
//            hotel.Country = model.Country;
//            hotel.Latitude = model.Latitude;
//            hotel.Longitude = model.Longitude;

//            hotel.Status = true;

//            hotel.CreatedAt = model.CreatedAt ?? hotel.CreatedAt;

//            _context.SaveChanges();

//            if (images != null && images.Count > 0)
//            {
//                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
//                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

//                foreach (var img in images)
//                {
//                    if (img?.Length > 0)
//                    {
//                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
//                        var filePath = Path.Combine(uploadFolder, fileName);
//                        using var stream = new FileStream(filePath, FileMode.Create);
//                        img.CopyTo(stream);

//                        _context.Photoss.Add(new Photos
//                        {
//                            HotelId = hotel.HotelId,
//                            Url = "/Image/" + fileName,
//                            SortOrder = 0
//                        });
//                    }
//                }
//                _context.SaveChanges();
//            }

//            return RedirectToAction("Hotels");
//        }

//        public IActionResult DeleteHotel(int id)
//        {
//            var hotel = _context.Hotels.Find(id);
//            if (hotel != null)
//            {
//                hotel.Status = false;
//                _context.Hotels.Update(hotel);
//                _context.SaveChanges();
//            }
//            return RedirectToAction("Hotels");
//        }

//        public IActionResult Rooms()
//        {
//            var rooms = _context.Rooms.Include(r => r.Hotel).ToList();
//            return View(rooms);
//        }

//        public IActionResult CreateRoom()
//        {
//            ViewBag.Hotels = _context.Hotels.ToList();
//            ViewBag.Amenities = _context.Amenities.ToList();

//            return View();
//        }
//        [HttpPost]
//        public IActionResult CreateRoom(Room model, List<IFormFile> images, List<int> amenityIds)
//        {
//            if (!ModelState.IsValid)
//            {
//                ViewBag.Hotels = _context.Hotels.ToList();
//                return View(model);
//            }

//            _context.Rooms.Add(model);
//            _context.SaveChanges();

//            if (amenityIds != null && amenityIds.Count > 0)
//            {
//                foreach (var amenityId in amenityIds)
//                {
//                    _context.RoomAmenities.Add(new RoomAmenitie
//                    {
//                        RoomId = model.RoomId,
//                        AmenityId = amenityId
//                    });
//                }
//                _context.SaveChanges();
//            }

//            if (images != null && images.Count > 0)
//            {
//                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");

//                if (!Directory.Exists(uploadFolder))
//                {
//                    Directory.CreateDirectory(uploadFolder);
//                }

//                foreach (var img in images)
//                {
//                    if (img != null && img.Length > 0)
//                    {
//                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
//                        var filePath = Path.Combine(uploadFolder, fileName);

//                        using (var stream = new FileStream(filePath, FileMode.Create))
//                        {
//                            img.CopyTo(stream);
//                        }

//                        _context.Photoss.Add(new Photos
//                        {
//                            RoomId = model.RoomId,
//                            Url = "/Image/" + fileName,
//                            SortOrder = 0
//                        });
//                    }
//                }

//                _context.SaveChanges();
//            }

//            return RedirectToAction("Rooms");
//        }

//        public IActionResult EditRoom(int id)
//        {
//            var room = _context.Rooms
//                .Include(r => r.Hotel)
//                .Include(r => r.RoomAmenities)
//                .ThenInclude(ra => ra.Amenity)
//                .Include(r => r.Photos)
//                .FirstOrDefault(r => r.RoomId == id);

//            if (room == null) return NotFound();

//            ViewBag.Hotels = _context.Hotels.ToList();
//            ViewBag.Amenities = _context.Amenities.ToList();

//            return View(room);
//        }

//        [HttpPost]
//        public IActionResult EditRoom(Room model, List<IFormFile> images, List<int> amenityIds)
//        {
//            if (!ModelState.IsValid)
//            {
//                ViewBag.Hotels = _context.Hotels.ToList();
//                ViewBag.Amenities = _context.Amenities.ToList();
//                return View(model);
//            }

//            _context.Rooms.Update(model);
//            _context.SaveChanges();

//            var existingAmenities = _context.RoomAmenities.Where(ra => ra.RoomId == model.RoomId);
//            _context.RoomAmenities.RemoveRange(existingAmenities);
//            if (amenityIds != null && amenityIds.Any())
//            {
//                foreach (var amenityId in amenityIds)
//                {
//                    _context.RoomAmenities.Add(new RoomAmenitie
//                    {
//                        RoomId = model.RoomId,
//                        AmenityId = amenityId
//                    });
//                }
//            }

//            if (images != null && images.Count > 0)
//            {
//                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
//                if (!Directory.Exists(uploadFolder))
//                {
//                    Directory.CreateDirectory(uploadFolder);
//                }

//                foreach (var img in images)
//                {
//                    if (img.Length > 0)
//                    {
//                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
//                        var filePath = Path.Combine(uploadFolder, fileName);

//                        using (var stream = new FileStream(filePath, FileMode.Create))
//                        {
//                            img.CopyTo(stream);
//                        }

//                        _context.Photoss.Add(new Photos
//                        {
//                            RoomId = model.RoomId,
//                            Url = "/Image/" + fileName,
//                            SortOrder = 0
//                        });
//                    }
//                }
//            }

//            _context.SaveChanges();
//            return RedirectToAction("Rooms");
//        }

//        public IActionResult DeleteRoom(int id)
//        {
//            var room = _context.Rooms.Find(id);
//            if (room != null)
//            {
//                _context.Rooms.Remove(room);
//                _context.SaveChanges();
//            }
//            return RedirectToAction("Rooms");
//        }

//        public IActionResult Amenities()
//        {
//            var amenities = _context.Amenities.ToList();
//            return View(amenities);
//        }

//        public IActionResult CreateAmenity()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult CreateAmenity(Amenities model)
//        {
//            if (!ModelState.IsValid) return View(model);

//            _context.Amenities.Add(model);
//            _context.SaveChanges();
//            return RedirectToAction("Amenities");
//        }

//        public IActionResult EditAmenity(int id)
//        {
//            var amenity = _context.Amenities.Find(id);
//            if (amenity == null) return NotFound();
//            return View(amenity);
//        }

//        [HttpPost]
//        public IActionResult EditAmenity(Amenities model)
//        {
//            if (!ModelState.IsValid) return View(model);

//            _context.Amenities.Update(model);
//            _context.SaveChanges();
//            return RedirectToAction("Amenities");
//        }

//        public IActionResult DeleteAmenity(int id)
//        {
//            var amenity = _context.Amenities.Find(id);
//            if (amenity != null)
//            {
//                _context.Amenities.Remove(amenity);
//                _context.SaveChanges();
//            }
//            return RedirectToAction("Amenities");
//        }

//        public IActionResult Vouchers()
//        {
//            var vouchers = _context.Vouchers.Include(v => v.User).ToList();
//            return View(vouchers);
//        }

//        public IActionResult CreateVoucher()
//        {
//            ViewBag.Users = _context.Users.ToList();
//            return View();
//        }

//        [HttpPost]
//        public IActionResult CreateVoucher(Voucher model)
//        {
//            if (!ModelState.IsValid) return View(model);
//            _context.Vouchers.Add(model);
//            _context.SaveChanges();
//            TempData["success"] = "Voucher created!";
//            return RedirectToAction("Vouchers");
//        }

//        public IActionResult EditVoucher(int id)
//        {
//            var voucher = _context.Vouchers.Find(id);
//            if (voucher == null) return NotFound();
//            ViewBag.Users = _context.Users.ToList();
//            return View(voucher);
//        }

//        [HttpPost]
//        public IActionResult EditVoucher(Voucher model)
//        {
//            if (!ModelState.IsValid) return View(model);
//            _context.Vouchers.Update(model);
//            _context.SaveChanges();
//            TempData["success"] = "Voucher updated!";
//            return RedirectToAction("Vouchers");
//        }

//        public IActionResult DeleteVoucher(int id)
//        {
//            var voucher = _context.Vouchers.Find(id);
//            if (voucher != null)
//            {
//                _context.Vouchers.Remove(voucher);
//                _context.SaveChanges();
//                TempData["success"] = "Voucher deleted!";
//            }
//            return RedirectToAction("Vouchers");
//        }

//        [HttpPost]
//        [Authorize(Roles = "Admin")]
//        public IActionResult AdminCancel(int id)
//        {
//            var booking = _context.Bookings
//                .Include(b => b.User)
//                .FirstOrDefault(b => b.BookingId == id);

//            if (booking == null) return NotFound();

//            if (booking.Status == BookingStatus.Paid)
//            {
//                booking.Status = BookingStatus.Canceled;
//                _context.SaveChanges();

//                var voucher = new Voucher
//                {
//                    Code = "ADM-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
//                    Description = $"Voucher bồi hoàn cho booking {booking.BookingId}",
//                    DiscountType = "Percent",
//                    DiscountValue = 10,
//                    MinOrderValue = 500000,
//                    ExpiryDate = DateTime.Now.AddMonths(1),
//                    Quantity = 1,
//                    IsActive = true
//                };

//                _context.Vouchers.Add(voucher);
//                _context.SaveChanges();

//                TempData["Success"] = $"Booking #{booking.BookingId} đã bị Admin hủy. Voucher {voucher.Code} đã được tặng cho khách.";
//            }
//            else if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
//            {
//                booking.Status = BookingStatus.Canceled;
//                _context.SaveChanges();
//                TempData["Info"] = $"Booking #{booking.BookingId} đã được hủy (chưa thanh toán nên không có voucher).";
//            }

//            return RedirectToAction("ManageBookings");
//        }
//#endregion

