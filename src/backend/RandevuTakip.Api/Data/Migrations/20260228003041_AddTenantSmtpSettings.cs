using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandevuTakip.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSmtpSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmtpJson",
                table: "Tenants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmtpJson",
                table: "Tenants");
        }
    }
}
