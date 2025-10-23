using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoookingHotels.Models
{
    public class Blog
    {
        [Key]
        public int BlogId { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ShortDescription { get; set; }   // Mô tả ngắn

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Thumbnail { get; set; }   // Ảnh đại diện (thumbnail)

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required, MaxLength(255)]
        public string Author { get; set; } = "Reviewer";

        [ForeignKey("User")]
        public int ReviewerId { get; set; }

        public User? Reviewer { get; set; }

        // Liên kết với Hotel (optional - nếu blog review về 1 hotel cụ thể)
        [ForeignKey("Hotel")]
        public int? HotelId { get; set; }

        public Hotel? Hotel { get; set; }
    }
}
