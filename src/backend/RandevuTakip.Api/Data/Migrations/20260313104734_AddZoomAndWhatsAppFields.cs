using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandevuTakip.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddZoomAndWhatsAppFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZoomConfigJson",
                table: "Tenants",
                type: "text",
                nullable: true);
 
            migrationBuilder.AddColumn<string>(
                name: "ZoomMeetingId",
                table: "Appointments",
                type: "text",
                nullable: true);
 
            migrationBuilder.AddColumn<string>(
                name: "ZoomMeetingUrl",
                table: "Appointments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZoomConfigJson",
                table: "Tenants");
 
            migrationBuilder.DropColumn(
                name: "ZoomMeetingId",
                table: "Appointments");
 
            migrationBuilder.DropColumn(
                name: "ZoomMeetingUrl",
                table: "Appointments");
        }
    }
}
