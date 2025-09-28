using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoookingHotels.Models
{
    [Table("Photos")]

    public class Photos
    {
        [Key]
        public int PhotoId { get; set; }
        public int? HotelId { get; set; }
        public int? RoomId { get; set; }
        public string Url { get; set; } = null!;
        public int? SortOrder { get; set; }

        public Hotel? Hotel { get; set; }
        public Room? Room { get; set; }
    }


}
