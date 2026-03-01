using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandevuTakip.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("a0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"),
                column: "PasswordHash",
                value: "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgdtsh7UUCV5pUtZTC0Esh9C6Z.G");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"),
                column: "Slug",
                value: "dentist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("a0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"),
                column: "PasswordHash",
                value: "$2a$12$R9h/lSuEbvTZLRe6v5pIVuBC6D.F9zE24vB8N0XfS6zX.q6v6v6v6");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("d0e3a6a1-a6a1-4a6a-a6a1-a6a1a6a1a6a1"),
                column: "Slug",
                value: "demo");
        }
    }
}
