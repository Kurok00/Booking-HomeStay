namespace ChillNest.MobileApp.Models;

public class VoucherModel
{
    public int VoucherId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty; // "Percent" or "Fixed"
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderValue { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public string DiscountDisplay { get; set; } = string.Empty;
}
