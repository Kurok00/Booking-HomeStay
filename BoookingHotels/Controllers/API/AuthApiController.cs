using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace BoookingHotels.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AuthApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        // POST: api/AuthApi/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check password
            bool isPasswordValid = false;
            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            }
            catch
            {
                isPasswordValid = (user.Password == request.Password);
            }

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Get user roles
            var roles = await (from ur in _db.UserRoles
                               join r in _db.Roles on ur.RoleId equals r.RoleId
                               where ur.UserId == user.UserId
                               select r.RoleName).ToListAsync();

            // Return user info (in real app, you should generate JWT token)
            return Ok(new
            {
                userId = user.UserId,
                userName = user.UserName,
                email = user.Email,
                fullName = user.FullName,
                phone = user.Phone,
                avatarUrl = user.AvatarUrl,
                roles = roles,
                message = "Login successful"
            });
        }

        // POST: api/AuthApi/register
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            var otp = new Random().Next(100000, 999999).ToString();

            // In mobile app, you should store this in a temporary storage or database
            // For now, we'll return the OTP (remove this in production!)
            var tempUser = new
            {
                Email = request.Email,
                Phone = request.Phone,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Otp = otp,
                ExpireAt = DateTime.Now.AddMinutes(5)
            };

            // Send OTP via email (removed for mobile testing - OTP returned directly)
            // _emailSender.Send(request.Email, $"Your OTP code is: {otp}");

            return Ok(new
            {
                message = "OTP sent to email",
                email = request.Email,
                // Remove this in production - only for testing
                otpForTesting = otp,
                tempUserToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tempUser)))
            });
        }

        // POST: api/AuthApi/verify-otp
        [HttpPost("verify-otp")]
        public async Task<ActionResult<object>> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.TempUserToken));
                var tempUser = JsonSerializer.Deserialize<TempUserModel>(json);

                if (tempUser == null || tempUser.ExpireAt < DateTime.Now)
                {
                    return BadRequest(new { message = "OTP expired" });
                }

                if (request.Otp != tempUser.Otp)
                {
                    return BadRequest(new { message = "Invalid OTP" });
                }

                // Create new user
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
                await _db.SaveChangesAsync();

                // Assign User role
                var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                if (role == null)
                {
                    role = new Role { RoleName = "User" };
                    _db.Roles.Add(role);
                    await _db.SaveChangesAsync();
                }

                _db.UserRoles.Add(new UserRole
                {
                    UserId = user.UserId,
                    RoleId = role.RoleId
                });
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Registration successful",
                    userId = user.UserId,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid token", error = ex.Message });
            }
        }

        // POST: api/AuthApi/forgot-password
        [HttpPost("forgot-password")]
        public async Task<ActionResult<object>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { message = "Email not found" });
            }

            string otp = new Random().Next(100000, 999999).ToString();
            var tempOtp = new
            {
                Email = request.Email,
                Otp = otp,
                ExpireAt = DateTime.Now.AddMinutes(5)
            };

            // _emailSender.Send(request.Email, $"Your password reset OTP is: {otp}");

            return Ok(new
            {
                message = "OTP sent to email",
                // Remove in production
                otpForTesting = otp,
                resetToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tempOtp)))
            });
        }

        // POST: api/AuthApi/reset-password
        [HttpPost("reset-password")]
        public async Task<ActionResult<object>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.ResetToken));
                var tempOtp = JsonSerializer.Deserialize<TempOtpModel>(json);

                if (tempOtp == null || tempOtp.ExpireAt < DateTime.Now)
                {
                    return BadRequest(new { message = "OTP expired" });
                }

                if (request.Otp != tempOtp.Otp)
                {
                    return BadRequest(new { message = "Invalid OTP" });
                }

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == tempOtp.Email);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Password reset successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid token", error = ex.Message });
            }
        }

        // GET: api/AuthApi/profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<object>> GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var roles = await (from ur in _db.UserRoles
                               join r in _db.Roles on ur.RoleId equals r.RoleId
                               where ur.UserId == user.UserId
                               select r.RoleName).ToListAsync();

            return Ok(new
            {
                user.UserId,
                user.UserName,
                user.Email,
                user.FullName,
                user.Phone,
                user.AvatarUrl,
                user.CreatedAt,
                roles
            });
        }

        // PUT: api/AuthApi/profile
        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<object>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            user.UserName = request.UserName ?? user.UserName;
            user.FullName = request.FullName ?? user.FullName;
            user.Phone = request.Phone ?? user.Phone;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully",
                user = new
                {
                    user.UserId,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    user.Phone,
                    user.AvatarUrl
                }
            });
        }
    }

    // Request models
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class VerifyOtpRequest
    {
        public string TempUserToken { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string ResetToken { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
    }

    public class TempUserModel
    {
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
    }

    public class TempOtpModel
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
    }
}
