using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandevuTakip.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffAndStaffServiceModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Staff",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Staff",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Staff",
                keyColumn: "Id",
                keyValue: new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"),
                columns: new[] { "Role", "UserId" },
                values: new object[] { "Staff", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Staff");
        }
    }
}
