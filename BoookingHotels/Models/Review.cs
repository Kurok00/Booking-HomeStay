namespace BoookingHotels.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int? HotelId { get; set; }
        public int? RoomId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVisible { get; set; } = true; // Mặc định là hiển thị

        public User User { get; set; } = null!;
        public Hotel? Hotel { get; set; }
        public Room? Room { get; set; }
        public ICollection<ReviewPhoto> Photos { get; set; } = new List<ReviewPhoto>();
    }
}
