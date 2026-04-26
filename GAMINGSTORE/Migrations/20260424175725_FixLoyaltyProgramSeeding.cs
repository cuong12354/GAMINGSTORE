using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GAMINGSTORE.Migrations
{
    /// <inheritdoc />
    public partial class FixLoyaltyProgramSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed default member tiers
            migrationBuilder.InsertData(
                table: "MemberTiers",
                columns: new[] { "Name", "MinPoints", "MaxPoints", "DiscountPercentage", "Color", "Description" },
                values: new object[,]
                {
                    { "Đồng", 0, 999, 0m, "bronze", "Thành viên mới - Không có ưu đãi" },
                    { "Bạc", 1000, 4999, 5m, "silver", "Thành viên Bạc - Giảm 5% trên tất cả hóa đơn" },
                    { "Vàng", 5000, 14999, 10m, "gold", "Thành viên Vàng - Giảm 10% trên tất cả hóa đơn" },
                    { "Bạch Kim", 15000, 2147483647, 15m, "platinum", "Thành viên Bạch Kim - Giảm 15% + VIP support" }
                });

            // Update existing users to use valid MemberTierId
            migrationBuilder.Sql("UPDATE AspNetUsers SET MemberTierId = 1 WHERE MemberTierId IS NULL OR MemberTierId = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete seeded member tiers
            migrationBuilder.Sql("DELETE FROM MemberTiers WHERE Name IN ('Đồng', 'Bạc', 'Vàng', 'Bạch Kim')");
        }
    }
}
