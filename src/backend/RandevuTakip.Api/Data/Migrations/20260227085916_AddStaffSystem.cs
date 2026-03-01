using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RandevuTakip.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "WorkingHours",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staff_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffServices_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Staff",
                columns: new[] { "Id", "Bio", "CreatedAt", "Email", "IsActive", "Name", "ProfilePictureUrl", "TenantId", "Title" },
                values: new object[] { new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), "15 yıllık deneyimli ortodonti uzmanı.", new DateTime(2026, 2, 27, 0, 0, 0, 0, DateTimeKind.Utc), "ayse@demo.com", true, "Dr. Ayşe Yılmaz", null, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), "Başhekim / Ortodontist" });

            migrationBuilder.UpdateData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "StaffId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "StaffId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "StaffId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"),
                column: "StaffId",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
                column: "StaffId",
                value: null);

            migrationBuilder.InsertData(
                table: "StaffServices",
                columns: new[] { "Id", "ServiceId", "StaffId" },
                values: new object[,]
                {
                    { new Guid("e1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), new Guid("b1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("e1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2"), new Guid("b2e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2"), new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("e1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3"), new Guid("b3e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3"), new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_StaffId",
                table: "WorkingHours",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StaffId",
                table: "Appointments",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_TenantId",
                table: "Staff",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffServices_ServiceId",
                table: "StaffServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffServices_StaffId",
                table: "StaffServices",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Staff_StaffId",
                table: "Appointments",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkingHours_Staff_StaffId",
                table: "WorkingHours",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Staff_StaffId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkingHours_Staff_StaffId",
                table: "WorkingHours");

            migrationBuilder.DropTable(
                name: "StaffServices");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_WorkingHours_StaffId",
                table: "WorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_StaffId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "WorkingHours");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Appointments");
        }
    }
}
