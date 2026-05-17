using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GAMINGSTORE.Migrations
{
    /// <inheritdoc />
    public partial class SetDefaultMenuVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Categories SET IsMenuVisible = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
