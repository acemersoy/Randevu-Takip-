using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RandevuTakip.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("a0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"));

            migrationBuilder.DeleteData(
                table: "StaffServices",
                keyColumn: "Id",
                keyValue: new Guid("e1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"));

            migrationBuilder.DeleteData(
                table: "StaffServices",
                keyColumn: "Id",
                keyValue: new Guid("e1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2"));

            migrationBuilder.DeleteData(
                table: "StaffServices",
                keyColumn: "Id",
                keyValue: new Guid("e1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3"));

            migrationBuilder.DeleteData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "WorkingHours",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("b1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"));

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("b2e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2"));

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("b3e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3"));

            migrationBuilder.DeleteData(
                table: "Staff",
                keyColumn: "Id",
                keyValue: new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"));

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "BookingFormSchema", "CreatedAt", "Industry", "IsActive", "Name", "Slug", "ThemeJson" },
                values: new object[] { new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), "[{\"id\": \"complaint\", \"type\": \"textarea\", \"label\": \"Şikayetiniz\", \"required\": true}]", new DateTime(2026, 2, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Dentist", true, "Örnek Diş Kliniği", "dentist", "{\"primaryColor\": \"#4f46e5\", \"logoUrl\": \"\"}" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "CreatedAt", "Email", "PasswordHash", "Role", "TenantId" },
                values: new object[] { new Guid("a0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), new DateTime(2026, 2, 27, 0, 0, 0, 0, DateTimeKind.Utc), "admin@demo.com", "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgdtsh7UUCV5pUtZTC0Esh9C6Z.G", "Owner", new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") });

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
                table: "Staff",
                columns: new[] { "Id", "Bio", "CreatedAt", "Email", "IsActive", "Name", "ProfilePictureUrl", "Role", "TenantId", "Title", "UserId" },
                values: new object[] { new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), "15 yıllık deneyimli ortodonti uzmanı.", new DateTime(2026, 2, 27, 0, 0, 0, 0, DateTimeKind.Utc), "ayse@demo.com", true, "Dr. Ayşe Yılmaz", null, "Staff", new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), "Başhekim / Ortodontist", null });

            migrationBuilder.InsertData(
                table: "WorkingHours",
                columns: new[] { "Id", "CloseTime", "DayOfWeek", "IsClosed", "OpenTime", "SlotStepMinutes", "StaffId", "TenantId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new TimeSpan(0, 18, 0, 0, 0), 1, false, new TimeSpan(0, 9, 0, 0, 0), 30, null, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new TimeSpan(0, 18, 0, 0, 0), 2, false, new TimeSpan(0, 9, 0, 0, 0), 30, null, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new TimeSpan(0, 18, 0, 0, 0), 3, false, new TimeSpan(0, 9, 0, 0, 0), 30, null, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new TimeSpan(0, 18, 0, 0, 0), 4, false, new TimeSpan(0, 9, 0, 0, 0), 30, null, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("00000000-0000-0000-0000-000000000005"), new TimeSpan(0, 18, 0, 0, 0), 5, false, new TimeSpan(0, 9, 0, 0, 0), 30, null, new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") }
                });

            migrationBuilder.InsertData(
                table: "StaffServices",
                columns: new[] { "Id", "ServiceId", "StaffId" },
                values: new object[,]
                {
                    { new Guid("e1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), new Guid("b1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"), new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("e1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2"), new Guid("b2e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a2"), new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") },
                    { new Guid("e1e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3"), new Guid("b3e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a3"), new Guid("c0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1") }
                });
        }
    }
}
