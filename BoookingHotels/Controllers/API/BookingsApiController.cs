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
        }

        // GET: api/BookingsApi
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUserBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

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

        // GET: api/BookingsApi/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBooking(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

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
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<object>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Validate room exists
            var room = await _context.Rooms
                .Include(r => r.Hotel)
                .FirstOrDefaultAsync(r => r.RoomId == request.RoomId);

            if (room == null)
                return NotFound(new { message = "Room not found" });

            if (!room.Status)
                return BadRequest(new { message = "Room is not available" });

            // Check if room is booked for the requested dates
            var isBooked = await _context.Bookings.AnyAsync(b =>
                b.RoomId == request.RoomId &&
                b.Status != BookingStatus.Canceled &&
                (request.CheckIn < b.CheckOut && request.CheckOut > b.CheckIn));

            if (isBooked)
                return BadRequest(new { message = "Room is already booked for the selected dates" });

            // Calculate total price
            var nights = (request.CheckOut - request.CheckIn).Days;
            var totalPrice = nights * room.Price;

            // Create booking
            var booking = new Booking
            {
                UserId = userId,
                RoomId = request.RoomId,
                HotelId = room.HotelId,
                CheckIn = request.CheckIn,
                CheckOut = request.CheckOut,
                SubTotal = totalPrice,
                Total = totalPrice,
                GuestName = request.GuestName ?? "",
                GuestPhone = request.GuestPhone ?? "",
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, new
            {
                booking.BookingId,
                booking.CheckIn,
                booking.CheckOut,
                TotalPrice = booking.Total,
                Status = booking.Status.ToString(),
                message = "Booking created successfully",
                hotelName = room.Hotel?.Name,
                roomName = room.Name,
                nights
            });
        }

        // PUT: api/BookingsApi/5/cancel
        [Authorize]
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<object>> CancelBooking(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

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
    }

    // Request models
    public class CreateBookingRequest
    {
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
    }

    public class CheckAvailabilityRequest
    {
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }
}
