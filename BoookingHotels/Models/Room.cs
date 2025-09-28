using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoookingHotels.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public int HotelId { get; set; }
        public string Name { get; set; } = null!;
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public string? BedType { get; set; }
        public int? Size { get; set; }
        public bool? Status { get; set; }

        public Hotel? Hotel { get; set; } = null!;
        public ICollection<RoomAmenitie>? RoomAmenities { get; set; } = new List<RoomAmenitie>();
        public ICollection<Photos>? Photos { get; set; } = new List<Photos>();
        public ICollection<Review>? Reviews { get; set; } = new List<Review>();
    }


}
