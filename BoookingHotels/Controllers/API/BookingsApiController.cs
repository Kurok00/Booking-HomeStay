using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BoookingHotels.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsApiController(ApplicationDbContext context)
        {
            _context = context;
            Console.WriteLine("🟢 BookingsApiController initialized!");
        }

        // GET: api/BookingsApi?userId=123
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUserBookings([FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "Invalid userId" });
            }

            Console.WriteLine($"🔍 Getting bookings for userId: {userId}");

            var bookings = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.Hotel)
                .Include(b => b.Room)
                    .ThenInclude(r => r!.Photos)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.BookingId,
                    b.CheckIn,
                    b.CheckOut,
                    TotalPrice = b.Total,
                    Status = b.Status.ToString(),
                    b.CreatedAt,
                    Room = new
                    {
                        b.Room!.RoomId,
                        b.Room.Name,
                        b.Room.Price,
                        MainPhoto = b.Room.Photos != null && b.Room.Photos.Any()
                            ? b.Room.Photos.First().Url
                            : "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=400"
                    },
                    Hotel = new
                    {
                        b.Room.Hotel!.HotelId,
                        b.Room.Hotel.Name,
                        b.Room.Hotel.Address,
                        b.Room.Hotel.City
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/BookingsApi/5?userId=123
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBooking(int id, [FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "Invalid userId" });
            }

            Console.WriteLine($"🔍 Getting booking {id} for userId: {userId}");

            var booking = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.Hotel)
                        .ThenInclude(h => h!.Photoss)
                .Include(b => b.Room)
                    .ThenInclude(r => r!.Photos)
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomAmenities!)
                        .ThenInclude(ra => ra.Amenity)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            var result = new
            {
                booking.BookingId,
                booking.CheckIn,
                booking.CheckOut,
                TotalPrice = booking.Total,
                Status = booking.Status.ToString(),
                booking.CreatedAt,
                Room = new
                {
                    booking.Room!.RoomId,
                    booking.Room.Name,
                    booking.Room.Capacity,
                    booking.Room.BedType,
                    booking.Room.Size,
                    booking.Room.Price,
                    MainPhoto = booking.Room.Photos != null && booking.Room.Photos.Any()
                        ? booking.Room.Photos.First().Url
                        : "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=400",
                    Photos = booking.Room.Photos != null ? booking.Room.Photos.Select(p => p.Url).ToList() : new List<string>()
                },
                Hotel = new
                {
                    booking.Room.Hotel!.HotelId,
                    booking.Room.Hotel.Name,
                    booking.Room.Hotel.Address,
                    booking.Room.Hotel.City,
                    booking.Room.Hotel.Country,
                    booking.Room.Hotel.Latitude,
                    booking.Room.Hotel.Longitude,
                    MainPhoto = booking.Room.Hotel.Photoss.FirstOrDefault(p => p.RoomId == null) != null
                        ? booking.Room.Hotel.Photoss.FirstOrDefault(p => p.RoomId == null)!.Url
                        : "https://via.placeholder.com/400x300"
                },
                User = new
                {
                    booking.User!.UserId,
                    booking.User.UserName,
                    booking.User.Email,
                    booking.User.Phone
                },
                Nights = (booking.CheckOut - booking.CheckIn).Days
            };

            return Ok(result);
        }

        // POST: api/BookingsApi
        // Temporarily removed [Authorize] - mobile app will send userId in request
        [HttpPost]
        public async Task<ActionResult<object>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            Console.WriteLine("🟢 ========== CREATE BOOKING API CALLED ==========");
            Console.WriteLine($"🔵 Request received - RoomId: {request?.RoomId}, CheckIn: {request?.CheckIn}, CheckOut: {request?.CheckOut}");
            Console.WriteLine($"🔵 Guest: {request?.GuestName}, Phone: {request?.GuestPhone}");
            Console.WriteLine($"🔵 Voucher Code: {request?.VoucherCode}");
            Console.WriteLine($"🔵 UserId from request: {request?.UserId}");

            if (request == null)
            {
                Console.WriteLine("❌ Request is NULL!");
                return BadRequest(new { message = "Invalid request data" });
            }

            // Get userId from request body (mobile app sends it)
            int userId = request.UserId;
            if (userId <= 0)
            {
                Console.WriteLine("❌ Invalid UserId in request!");
                return BadRequest(new { message = "Please login to make a booking" });
            }

            Console.WriteLine($"🔵 UserId: {userId}");

            // Validate room exists
            Console.WriteLine($"🔍 Looking for room {request.RoomId}...");
            var room = await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.RoomId == request.RoomId);

            if (room == null)
            {
                Console.WriteLine($"❌ Room {request.RoomId} NOT FOUND!");
                return NotFound(new { message = "Room not found" });
            }

            Console.WriteLine($"✅ Room found: {room.Name}, Hotel: {room.Hotel?.Name}, Status: {room.Status}");

            Console.WriteLine($"✅ Room found: {room.Name}, Hotel: {room.Hotel?.Name}, Status: {room.Status}");

            if (!room.Status)
            {
                Console.WriteLine($"❌ Room {room.RoomId} is NOT available!");
                return BadRequest(new { message = "Room is not available" });
            }

            // Check if room is booked for the requested dates
            Console.WriteLine($"🔍 Checking booking conflicts for {request.CheckIn:yyyy-MM-dd} to {request.CheckOut:yyyy-MM-dd}...");
            var isBooked = await _context.Bookings.AnyAsync(b =>
                b.RoomId == request.RoomId &&
                b.Status != BookingStatus.Canceled &&
                (request.CheckIn < b.CheckOut && request.CheckOut > b.CheckIn));

            if (isBooked)
            {
                Console.WriteLine($"❌ Room {request.RoomId} already booked for selected dates!");
                return BadRequest(new { message = "Room is already booked for the selected dates" });
            }

            Console.WriteLine($"✅ Room available for booking!");

            Console.WriteLine($"✅ Room available for booking!");

            // Calculate total price
            var nights = (request.CheckOut - request.CheckIn).Days;
            if (nights <= 0) nights = 1;

            var subTotal = nights * room.Price;
            var totalPrice = subTotal;
            decimal discount = 0;

            Console.WriteLine($"💰 Base price: {nights} nights x {room.Price:N0} = {subTotal:N0} VND");

            // Apply voucher if provided
            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                Console.WriteLine($"🎫 Checking voucher: {request.VoucherCode}");

                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v =>
                    v.Code == request.VoucherCode &&
                    v.IsActive &&
                    v.ExpiryDate > DateTime.Now &&
                    v.Quantity > 0);

                if (voucher != null && (voucher.MinOrderValue == null || subTotal >= voucher.MinOrderValue))
                {
                    // Check if user already used this voucher
                    var alreadyUsed = await _context.UserVouchers.AnyAsync(uv =>
                        uv.UserId == userId && uv.VoucherId == voucher.VoucherId);

                    if (!alreadyUsed)
                    {
                        // Calculate discount
                        discount = voucher.DiscountType == "Percent"
                            ? subTotal * (voucher.DiscountValue / 100)
                            : voucher.DiscountValue;

                        totalPrice = subTotal - discount;

                        // Decrease voucher quantity
                        voucher.Quantity -= 1;
                        if (voucher.Quantity <= 0)
                            voucher.IsActive = false;

                        // Mark voucher as used by this user
                        _context.UserVouchers.Add(new UserVoucher
                        {
                            UserId = userId,
                            VoucherId = voucher.VoucherId
                        });

                        Console.WriteLine($"✅ Voucher applied! Discount: {discount:N0} VND, New Total: {totalPrice:N0} VND");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ User already used this voucher");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ Voucher invalid or expired");
                }
            }

            // Create booking
            Console.WriteLine($"📝 Creating booking entity...");
            Console.WriteLine($"💳 Payment method: {request.PaymentMethod}");

            // Set status based on payment method (same logic as web)
            var bookingStatus = request.PaymentMethod == "COD"
                ? BookingStatus.Paid   // COD = Thanh toán khi nhận phòng
                : BookingStatus.Confirmed; // ONLINE = Thanh toán online

            var booking = new Booking
            {
                UserId = userId,
                RoomId = request.RoomId,
                HotelId = room.HotelId,
                CheckIn = request.CheckIn,
                CheckOut = request.CheckOut,
                SubTotal = subTotal,
                Discount = discount > 0 ? discount : null,
                Total = totalPrice,
                GuestName = request.GuestName ?? "",
                GuestPhone = request.GuestPhone ?? "",
                Currency = "VND",
                Status = bookingStatus,
                CreatedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);
            Console.WriteLine($"💾 Saving to database...");
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Booking created successfully - ID: {booking.BookingId}, Room: {room.Name}, Guest: {booking.GuestName}");
            Console.WriteLine("🟢 ========== CREATE BOOKING COMPLETED ==========");

            return Ok(new
            {
                bookingId = booking.BookingId,
                checkIn = booking.CheckIn,
                checkOut = booking.CheckOut,
                totalPrice = booking.Total,
                status = booking.Status.ToString(),
                message = "Booking created successfully",
                hotelName = room.Hotel?.Name,
                roomName = room.Name,
                nights
            });
        }

        // POST: api/BookingsApi/5/cancel?userId=123
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<object>> CancelBooking(int id, [FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "Invalid userId" });
            }

            Console.WriteLine($"🔴 Canceling booking {id} for userId: {userId}");

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == userId);

            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            if (booking.Status == BookingStatus.Canceled)
                return BadRequest(new { message = "Booking is already cancelled" });

            if (booking.Status == BookingStatus.Paid)
                return BadRequest(new { message = "Cannot cancel paid booking" });

            booking.Status = BookingStatus.Canceled;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Booking cancelled successfully",
                bookingId = booking.BookingId,
                status = booking.Status.ToString()
            });
        }

        // POST: api/BookingsApi/check-availability
        [HttpPost("check-availability")]
        public async Task<ActionResult<object>> CheckAvailability([FromBody] CheckAvailabilityRequest request)
        {
            var room = await _context.Rooms.FindAsync(request.RoomId);

            if (room == null)
                return NotFound(new { message = "Room not found" });

            if (!room.Status)
                return Ok(new { available = false, message = "Room is not available" });

            var isBooked = await _context.Bookings.AnyAsync(b =>
                b.RoomId == request.RoomId &&
                b.Status != BookingStatus.Canceled &&
                (request.CheckIn < b.CheckOut && request.CheckOut > b.CheckIn));

            return Ok(new
            {
                available = !isBooked,
                message = isBooked ? "Room is booked for the selected dates" : "Room is available"
            });
        }

        // GET: api/BookingsApi/available-vouchers
        // Temporarily allow without auth for testing - will require userId parameter instead
        [HttpGet("available-vouchers")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableVouchers([FromQuery] int? userId)
        {
            Console.WriteLine($"🔵 [BookingsApi] GET available-vouchers - UserId: {userId}");

            // Get userId from auth token or query parameter
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int finalUserId = 0;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                finalUserId = int.Parse(userIdClaim);
                Console.WriteLine($"🔵 Authenticated UserId: {finalUserId}");
            }
            else if (userId.HasValue)
            {
                finalUserId = userId.Value;
                Console.WriteLine($"🔵 Using query param UserId: {finalUserId}");
            }
            else
            {
                // Return all vouchers if no user context
                Console.WriteLine($"🔵 No userId - returning all active vouchers");
                var allVouchers = await _context.Vouchers
                    .Where(v => v.IsActive
                        && v.ExpiryDate > DateTime.Now
                        && v.Quantity > 0)
                    .Select(v => new
                    {
                        v.VoucherId,
                        v.Code,
                        v.Description,
                        v.DiscountType,
                        v.DiscountValue,
                        v.MinOrderValue,
                        v.ExpiryDate,
                        v.Quantity,
                        DiscountDisplay = v.DiscountType == "Percent"
                            ? $"{v.DiscountValue}%"
                            : $"{v.DiscountValue:N0} VND"
                    })
                    .ToListAsync();

                Console.WriteLine($"✅ Returning {allVouchers.Count} vouchers");
                return Ok(allVouchers);
            }

            // Get vouchers that user hasn't used yet
            var vouchers = await _context.Vouchers
                .Where(v => v.IsActive
                    && v.ExpiryDate > DateTime.Now
                    && v.Quantity > 0
                    && !_context.UserVouchers.Any(uv => uv.UserId == finalUserId && uv.VoucherId == v.VoucherId))
                .Select(v => new
                {
                    v.VoucherId,
                    v.Code,
                    v.Description,
                    v.DiscountType,
                    v.DiscountValue,
                    v.MinOrderValue,
                    v.ExpiryDate,
                    v.Quantity,
                    DiscountDisplay = v.DiscountType == "Percent"
                        ? $"{v.DiscountValue}%"
                        : $"{v.DiscountValue:N0} VND"
                })
                .ToListAsync();

            Console.WriteLine($"✅ Returning {vouchers.Count} unused vouchers for user {finalUserId}");
            return Ok(vouchers);
        }
    }

    // Request models
    public class CreateBookingRequest
    {
        public int UserId { get; set; } // ✅ Mobile app will send userId
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? VoucherCode { get; set; } // ✅ Added voucher support
        public string PaymentMethod { get; set; } = "COD"; // ✅ Payment method: COD or ONLINE
    }

    public class CheckAvailabilityRequest
    {
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }
}
