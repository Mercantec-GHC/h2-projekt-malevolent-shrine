using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGojoPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Используем статический BCrypt хеш для пароля "SuperSecretPassword123!"
            // Этот хеш был сгенерирован заранее для избежания динамических значений
            var staticHashedPassword = "$2a$11$XvW7nt9K.oF9nK7nMGqXnOHzCzEEwQm6HZJV8KfODaI3kJWkQkQaK";
            
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "HashedPassword",
                value: staticHashedPassword);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "HashedPassword",
                value: "$2a$11$hashed_password_here");
        }
    }
}
