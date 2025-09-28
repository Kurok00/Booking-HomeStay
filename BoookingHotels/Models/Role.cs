using System;

namespace BoookingHotels.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

}
