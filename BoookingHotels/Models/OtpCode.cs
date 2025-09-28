using System;

namespace BoookingHotels.Models
{
    public class OtpCode
    {
        public Guid OtpId { get; set; }
        public int UserId { get; set; }
        public string Channel { get; set; } = "";
        public string Code { get; set; } = "";
        public DateTime ExpireAt { get; set; }
    }
}
