using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandevuTakip.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsOnlineToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "Services",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "Services");
        }
    }
}
