using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BCrypt.Net;
using System.Text.Json;

namespace BoookingHotels.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AuthController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        #region Auth
        public IActionResult Index() => View();

        public IActionResult Login() => View();

        public interface IEmailSender
        {
            void Send(string to, string content);
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                return View();
            }

            // Check if password is valid (supports both BCrypt hash and plain text for backward compatibility)
            bool isPasswordValid = false;
            try
            {
                // Try BCrypt verification first
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            }
            catch
            {
                // If BCrypt fails, compare plain text (for old/seeded data)
                isPasswordValid = (user.Password == password);
            }

            if (!isPasswordValid)
            {
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                return View();
            }

            var roles = (from ur in _db.UserRoles
                         join r in _db.Roles on ur.RoleId equals r.RoleId
                         where ur.UserId == user.UserId
                         select r.RoleName).ToList();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (roles.Contains("Reviewer"))
            {
                return RedirectToAction("Create", "Blogs");
            }
            else if (roles.Contains("Host"))
            {
                return RedirectToAction("MyHotels", "Host");
            }
            else
            {
                return RedirectToAction("Index", "Auth");
            }


        }


        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string email, string phone, string password)
        {
            if (_db.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email đã tồn tại");
                return View();
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var tempUser = new
            {
                Email = email,
                Phone = phone,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(password),
                Otp = otp,
                ExpireAt = DateTime.Now.AddMinutes(5)
            };

            TempData["TempUser"] = JsonSerializer.Serialize(tempUser);
            _emailSender.Send(email, $"Mã OTP của bạn là: {otp}");

            return RedirectToAction("VerifyOtp");
        }

        public IActionResult VerifyOtp() => View();

        [HttpPost]
        public IActionResult VerifyOtp(string otp)
        {
            if (!TempData.TryGetValue("TempUser", out var raw))
                return RedirectToAction("Register");

            var tempUser = JsonSerializer.Deserialize<TempUserOtpModel>(raw?.ToString() ?? string.Empty);

            if (tempUser == null || tempUser.ExpireAt < DateTime.Now)
            {
                ModelState.AddModelError("", "Mã OTP đã hết hạn. Vui lòng đăng ký lại.");
                return RedirectToAction("Register");
            }

            if (otp != tempUser.Otp)
            {
                ModelState.AddModelError("", "Sai mã OTP");
                TempData["TempUser"] = JsonSerializer.Serialize(tempUser); // giữ lại để nhập lại
                return View();
            }

            // Tạo user mới
            var user = new User
            {
                UserName = tempUser.Email,
                Email = tempUser.Email,
                Status = true,
                Phone = tempUser.Phone,
                Password = tempUser.HashedPassword,
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            // Kiểm tra role "User" đã có chưa, nếu chưa thì tạo
            var role = _db.Roles.FirstOrDefault(r => r.RoleName == "User");
            if (role == null)
            {
                role = new Role { RoleName = "User" };
                _db.Roles.Add(role);
                _db.SaveChanges();
            }

            // Gán mặc định role User cho tài khoản vừa tạo
            _db.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId
            });
            _db.SaveChanges();

            return RedirectToAction("Login");
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại");
                return View();
            }

            string otp = new Random().Next(100000, 999999).ToString();
            var tempOtp = new TempUserOtpModel
            {
                Email = email,
                Otp = otp,
                ExpireAt = DateTime.Now.AddMinutes(5)
            };

            TempData["ResetOtp"] = JsonSerializer.Serialize(tempOtp);
            _emailSender.Send(email, $"Mã OTP để đặt lại mật khẩu của bạn là: {otp}");

            return RedirectToAction("ResetPassword");
        }

        public IActionResult ResetPassword() => View();

        [HttpPost]
        public IActionResult ResetPassword(string otp, string newPassword)
        {
            if (!TempData.TryGetValue("ResetOtp", out var raw))
                return RedirectToAction("ForgotPassword");

            var tempOtp = JsonSerializer.Deserialize<TempUserOtpModel>(raw?.ToString() ?? string.Empty);
            if (tempOtp == null || tempOtp.ExpireAt < DateTime.Now)
            {
                ModelState.AddModelError("", "OTP đã hết hạn");
                return RedirectToAction("ForgotPassword");
            }

            if (otp != tempOtp.Otp)
            {
                ModelState.AddModelError("", "Sai mã OTP");
                TempData["ResetOtp"] = JsonSerializer.Serialize(tempOtp); // giữ lại để thử lại
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == tempOtp.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Người dùng không tồn tại");
                return RedirectToAction("ForgotPassword");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _db.SaveChanges();

            TempData["Success"] = "Mật khẩu đã được đặt lại";
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = _db.Users.FirstOrDefault(u => u.UserId == userId);
            return View(user);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Profile(User model, IFormFile? avatarFile)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = _db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.Phone = model.Phone;

            // Handle avatar upload
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(avatarFile.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["Error"] = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif)";
                    return RedirectToAction("Profile");
                }

                // Create unique filename
                var fileName = $"avatar_{userId}_{Guid.NewGuid()}{fileExtension}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image", "avatars");
                
                // Create directory if not exists
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);

                // Delete old avatar if exists
                if (!string.IsNullOrEmpty(user.AvatarUrl) && user.AvatarUrl.StartsWith("/Image/avatars/"))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save new avatar
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    avatarFile.CopyTo(stream);
                }

                user.AvatarUrl = $"/Image/avatars/{fileName}";
            }

            _db.SaveChanges();

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }
        #endregion
    }
    public class TempUserOtpModel
    {
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
    }

}