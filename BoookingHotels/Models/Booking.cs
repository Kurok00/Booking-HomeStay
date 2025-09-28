using System.ComponentModel.DataAnnotations;

namespace BoookingHotels.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required(ErrorMessage = "Bạn phải chọn phòng")]
        public int? RoomId { get; set; }

        [Required(ErrorMessage = "Bạn phải chọn ngày nhận phòng")]
        public DateTime CheckIn { get; set; }

        [Required(ErrorMessage = "Bạn phải chọn ngày trả phòng")]
        public DateTime CheckOut { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string GuestName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string GuestPhone { get; set; } = null!;
        public decimal SubTotal { get; set; }
        public decimal? Discount { get; set; }
        public decimal Total { get; set; }
        public string? Currency { get; set; }
        public DateTime? CreatedAt { get; set; }

        public Hotel? Hotel { get; set; } = null!;
        public User? User { get; set; } = null!;
        public Room? Room { get; set; }
    }
    public enum BookingStatus
    {
        Pending,   
        Confirmed,  
        Paid,       
        Canceled   
    }
}
