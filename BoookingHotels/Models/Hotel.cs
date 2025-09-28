using System.ComponentModel.DataAnnotations;

namespace BoookingHotels.Models
{
    public class Hotel
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Country { get; set; } = null!;
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<Photos> Photoss { get; set; } = new List<Photos>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }


}
