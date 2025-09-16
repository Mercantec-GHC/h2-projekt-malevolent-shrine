using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FixGojoPasswordFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "HashedPassword",
                value: "$2a$11$XvW7nt9K.oF9nK7nMGqXnOHzCzEEwQm6HZJV8KfODaI3kJWkQkQaK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "HashedPassword",
                value: "AQAAAAIAAYagAAAAEDR2mIMzeg+rQ7xlh0m8hpkoEaQ3do9AivJjCtZ3Nky52Z+4t7o5gV88ZIvSzpv9/g==");
        }
    }
}
