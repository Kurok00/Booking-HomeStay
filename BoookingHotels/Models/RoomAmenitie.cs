using System.ComponentModel.DataAnnotations;

namespace BoookingHotels.Models
{
    public class RoomAmenitie
    {

        public int RoomId { get; set; }
        public int AmenityId { get; set; }

        public Room Room { get; set; } = null!;
        public Amenities Amenity { get; set; } = null!;
    }

}
