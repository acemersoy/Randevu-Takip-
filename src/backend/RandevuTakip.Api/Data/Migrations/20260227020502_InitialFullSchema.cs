using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RandevuTakip.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialFullSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Industry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ThemeJson = table.Column<string>(type: "text", nullable: false),
                    BookingFormSchema = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    SlotStepMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkingHours_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CustomerPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ExtraJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "BookingFormSchema", "CreatedAt", "Industry", "IsActive", "Name", "Slug", "ThemeJson" },
                values: new object[] { new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), "[{\"id\": \"complaint\", \"type\": \"textarea\", \"label\": \"Şikayetiniz\", \"required\": true}]", new DateTime(2026, 2, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Dentist", true, "Örnek Diş Kliniği", "demo", "{\"primaryColor\": \"#4f46e5\", \"logoUrl\": \"\"}" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "CreatedAt", "Email", "PasswordHash", "Role", "TenantId" },
                values: new object[] { new Guid("a0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), new DateTime(2026, 2, 27, 0, 0, 0, 0, DateTimeKind.Utc), "admin@demo.com", "$2a$12$R9h/lSuEbvTZLRe6v5pIVuBC6D.F9zE24vB8N0XfS6zX.q6v6v6v6", "Owner", new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "CreatedAt", "Description", "DurationMinutes", "IsActive", "Name", "Price", "TenantId" },
                values: new object[,]
                {
                    { new Guid("b1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), new DateTime(2026, 2, 27, 0, 0, 0, 0, DateTimeKind.Utc), "İlk kontrol ve ağız sağlığı değerlendirmesi.", 20, true, "Genel Muayene", 500m, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("b2e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2"), new DateTime(2026, 2, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Estetik dolgu işlemi.", 45, true, "Kompozit Dolgu", 1200m, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("b3e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3"), new DateTime(2026, 2, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Derinlemesine diş taşı ve plak temizliği.", 30, true, "Diş Taşı Temizliği", 800m, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") }
                });

            migrationBuilder.InsertData(
                table: "WorkingHours",
                columns: new[] { "Id", "CloseTime", "DayOfWeek", "IsClosed", "OpenTime", "SlotStepMinutes", "TenantId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new TimeSpan(0, 18, 0, 0, 0), 1, false, new TimeSpan(0, 9, 0, 0, 0), 30, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new TimeSpan(0, 18, 0, 0, 0), 2, false, new TimeSpan(0, 9, 0, 0, 0), 30, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new TimeSpan(0, 18, 0, 0, 0), 3, false, new TimeSpan(0, 9, 0, 0, 0), 30, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new TimeSpan(0, 18, 0, 0, 0), 4, false, new TimeSpan(0, 9, 0, 0, 0), 30, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("00000000-0000-0000-0000-000000000005"), new TimeSpan(0, 18, 0, 0, 0), 5, false, new TimeSpan(0, 9, 0, 0, 0), 30, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_TenantId",
                table: "Admins",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TenantId",
                table: "Appointments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_TenantId",
                table: "Services",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_TenantId",
                table: "WorkingHours",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "WorkingHours");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
