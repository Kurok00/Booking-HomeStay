using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BoookingHotels.Models
{
    public class Hotel
    {
        public int HotelId { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required, StringLength(255)]
        public string Address { get; set; } = null!;

        [Required, StringLength(100)]
        public string City { get; set; } = null!;

        [Required, StringLength(100)]
        public string Country { get; set; } = null!;

        public bool? Status { get; set; } = true;

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Host tạo
        public bool? IsUserHostCreated { get; set; }
        public DateTime? IsUserHostCreatedDate { get; set; }

        // Admin duyệt
        public bool IsApproved { get; set; } = false;

        // Quan hệ
        public int? CreatedBy { get; set; }
        public User? Creator { get; set; }
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<Photos> Photoss { get; set; } = new List<Photos>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
