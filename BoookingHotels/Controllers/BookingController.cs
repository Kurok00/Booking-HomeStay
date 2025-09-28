using BoookingHotels.Data;
using BoookingHotels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;
[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Create(int roomId, DateTime? checkIn, DateTime? checkOut)
    {
        var room = _context.Rooms
            .Include(r => r.Hotel)
            .FirstOrDefault(r => r.RoomId == roomId);
        if (room == null) return NotFound();

        // Nếu chưa truyền ngày thì gán mặc định
        var defaultCheckIn = checkIn ?? DateTime.Today;
        var defaultCheckOut = checkOut ?? DateTime.Today.AddDays(1);

  
        var days = (defaultCheckOut - defaultCheckIn).Days;
        if (days <= 0) days = 1;

        var booking = new Booking
        {
            HotelId = room.HotelId,
            RoomId = room.RoomId,
            Status = BookingStatus.Pending,
            CheckIn = defaultCheckIn,
            CheckOut = defaultCheckOut,
            SubTotal = room.Price * days,
            Total = room.Price * days,
            Currency = "VND"
        };

        ViewBag.Room = room;
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var vouchers = _context.Vouchers
            .Where(v => v.IsActive && v.ExpiryDate > DateTime.Now &&
                   (v.UserId == null || v.UserId == userId))
            .ToList();

        ViewBag.Vouchers = vouchers;   
        return View(booking);
    }

    [HttpPost]
    public IActionResult Create(Booking booking, string paymentMethod, string? voucherCode)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();

            TempData["Error"] = "Vui lòng điền đầy đủ thông tin.";
            return RedirectToAction("Create", "Bookings", new { roomId = booking.RoomId });
        }


        var room = _context.Rooms
            .Include(r => r.Hotel)
            .FirstOrDefault(r => r.RoomId == booking.RoomId);
        if (room == null) return NotFound();

        var days = (booking.CheckOut - booking.CheckIn).Days;
        if (days <= 0) days = 1;

        bool isBooked = _context.Bookings.Any(b =>
       b.RoomId == booking.RoomId &&
       b.Status != BookingStatus.Canceled &&
       !(booking.CheckOut <= b.CheckIn || booking.CheckIn >= b.CheckOut)
   );

        if (isBooked)
        {
            TempData["Error"] = "Phòng này đã có người khác đặt trong khoảng thời gian bạn chọn.";
            return RedirectToAction("Create", new { roomId = booking.RoomId});
        }


        booking.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        booking.CreatedAt = DateTime.Now;
        booking.SubTotal = room.Price * days;
        booking.Total = booking.SubTotal;

        if (!string.IsNullOrEmpty(voucherCode))
        {
            var voucher = _context.Vouchers.FirstOrDefault(v =>
                v.Code == voucherCode &&
                v.IsActive &&
                v.ExpiryDate > DateTime.Now &&
                v.Quantity > 0);

            if (voucher != null && (voucher.MinOrderValue == null || booking.SubTotal >= voucher.MinOrderValue))
            {
                decimal discount = voucher.DiscountType == "Percent"
                    ? booking.SubTotal * (voucher.DiscountValue / 100)
                    : voucher.DiscountValue;

                booking.Discount = discount;
                booking.Total = booking.SubTotal - discount;

                // Trừ số lượng voucher
                voucher.Quantity -= 1;
                if (voucher.Quantity <= 0) voucher.IsActive = false;
            }
        }

        booking.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        booking.CreatedAt = DateTime.Now;
        booking.Currency = "VND";

        booking.Status = paymentMethod == "COD"
            ? BookingStatus.Paid
            : BookingStatus.Confirmed; 

        _context.Bookings.Add(booking);
        _context.SaveChanges();

        return RedirectToAction("Confirm", new { id = booking.BookingId });
    }

    // Xác nhận đặt phòng
    public IActionResult Confirm(int id)
    {
        var booking = _context.Bookings
            .Include(b => b.Hotel)
            .Include(b => b.Room)
            .FirstOrDefault(b => b.BookingId == id);

        if (booking == null) return NotFound();

        return View(booking);
    }
    [Authorize]
    public IActionResult MyBookings()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value); 

        var bookings = _context.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.Hotel)
            .Include(b => b.Room)
            .OrderByDescending(b => b.CreatedAt)
            .ToList();

        return View(bookings);
    }
    [HttpPost]
    public IActionResult Cancel(int id)
    {
        var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
        if (booking == null) return NotFound();

        if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
        {
            booking.Status = BookingStatus.Canceled;
            _context.SaveChanges();
        }

        return RedirectToAction("MyBookings");
    }
    [HttpPost]
    [Authorize]
    public IActionResult Pay(int id)
    {
        var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == id);
        if (booking == null || booking.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        {
            return Unauthorized();
        }

        if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
        {
            booking.Status = BookingStatus.Paid;
            _context.SaveChanges();
            TempData["Success"] = "Payment successful!";
        }
        else
        {
            TempData["Error"] = "This booking cannot be paid.";
        }

        return RedirectToAction("MyBookings");
    }
    [Authorize]
    public IActionResult ExportInvoice(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var booking = _context.Bookings
            .Include(b => b.Hotel)
            .Include(b => b.Room)
            .FirstOrDefault(b => b.BookingId == id && b.UserId == userId);

        if (booking == null) return NotFound();

        if (booking.Status != BookingStatus.Paid)
        {
            TempData["Error"] = "Hóa đơn chỉ có khi đơn đã được thanh toán.";
            return RedirectToAction("MyBookings");
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);

                page.Header().Text("HÓA ĐƠN ĐẶT PHÒNG")
                    .FontSize(20).Bold().AlignCenter();

                page.Content().Column(col =>
                {
                    col.Item().Text($"Mã hóa đơn: #{booking.BookingId}").FontSize(12);
                    col.Item().Text($"Khách sạn: {booking.Hotel.Name}").FontSize(12);
                    col.Item().Text($"Phòng: {booking.Room?.Name}").FontSize(12);
                    col.Item().Text($"Khách hàng: {booking.GuestName}").FontSize(12);
                    col.Item().Text($"SĐT: {booking.GuestPhone}").FontSize(12);
                    col.Item().Text($"Check-in: {booking.CheckIn:dd/MM/yyyy}").FontSize(12);
                    col.Item().Text($"Check-out: {booking.CheckOut:dd/MM/yyyy}").FontSize(12);
                    col.Item().Text($"Số ngày: {(booking.CheckOut - booking.CheckIn).Days}").FontSize(12);

                    col.Item().PaddingTop(10)
                        .Text($"Tổng tiền: {booking.Total:N0} {booking.Currency}")
                        .FontSize(14).Bold().FontColor(Colors.Green.Darken2);

                    col.Item().PaddingTop(20)
                        .Text("Cảm ơn quý khách đã đặt phòng tại BoookingHotels!")
                        .AlignCenter().Italic();
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("BoookingHotels © 2025 - ");
                    txt.Span("www.boookinghotels.com").Underline();
                });
            });
        });

        var pdfBytes = document.GeneratePdf();

        return File(pdfBytes, "application/pdf", $"Invoice_{booking.BookingId}.pdf");
    }

}
