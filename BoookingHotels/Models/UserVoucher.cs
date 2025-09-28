using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoookingHotels.Models
{
    [Table("UserVouchers")]
    public class UserVoucher
    {
        [Key]
        public int UserVoucherId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int VoucherId { get; set; }


        // Navigation
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("VoucherId")]
        public Voucher Voucher { get; set; } = null!;
    }
}
