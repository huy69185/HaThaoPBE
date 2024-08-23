using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderDateToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm cột OrderDate vào bảng Orders
            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "Orders",
                type: "DATETIME",  // hoặc "DATETIME" tùy thuộc vào kiểu dữ liệu mà SQLite sử dụng cho DateTime
                nullable: false,
                defaultValue: DateTime.Now); // Gán giá trị mặc định là ngày giờ hiện tại
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa cột OrderDate khỏi bảng Orders
            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "Orders");
        }
    }
}
