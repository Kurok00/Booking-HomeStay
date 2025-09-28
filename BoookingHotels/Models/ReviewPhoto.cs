using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoookingHotels.Models
{
    [Table("ReviewPhotos")]

    public class ReviewPhoto
    {
        [Key]
        public int PhotoId { get; set; }
        public int ReviewId { get; set; }
        public string Url { get; set; } = null!;
        public int SortOrder { get; set; }

        public Review Review { get; set; } = null!;
    }


}
