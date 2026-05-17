using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GAMINGSTORE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCouponScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicableCategoryIds",
                table: "Coupons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicableProductIds",
                table: "Coupons",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicableCategoryIds",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "ApplicableProductIds",
                table: "Coupons");
        }
    }
}
