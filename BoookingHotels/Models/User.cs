using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoookingHotels.Models
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string Password { get; set; } = null!;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

}