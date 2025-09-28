using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoookingHotels.Models
{
    [Table("Amenities")]
    public class Amenities
    {
        [Key]
        public int AmenityId { get; set; }
        public string Name { get; set; } = null!;
        public string? Icon { get; set; }

        public ICollection<RoomAmenitie> RoomAmenities { get; set; } = new List<RoomAmenitie>();
    }


}
