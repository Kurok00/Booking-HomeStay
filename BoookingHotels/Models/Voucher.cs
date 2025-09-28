namespace BoookingHotels.Models
{
    public class Voucher
    {
        public int VoucherId { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = null!; // "Percent" hoặc "Amount"
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderValue { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int Quantity { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔑 Thêm UserId (nullable)
        public int? UserId { get; set; }
        public User? User { get; set; }
    }


}
