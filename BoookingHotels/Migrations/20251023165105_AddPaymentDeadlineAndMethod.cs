using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoookingHotels.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentDeadlineAndMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Đã có cột PaymentDeadline và PaymentMethod trên production, bỏ qua thao tác thêm cột để tránh lỗi.
            // Nếu cần rollback, hãy tự xử lý thủ công trên database.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Đã có cột PaymentDeadline và PaymentMethod trên production, bỏ qua thao tác xóa cột để tránh lỗi.
        }
    }
}
