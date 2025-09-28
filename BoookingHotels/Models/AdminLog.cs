using System;
using System.ComponentModel.DataAnnotations;

namespace BoookingHotels.Models
{
    public class AdminLog
    {
        [Key]
        public int LogId { get; set; }
        public int AdminId { get; set; }
        public string Action { get; set; } = null!;
        public string Entity { get; set; } = null!;
        public int? EntityId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? Admin { get; set; }
    }
}
