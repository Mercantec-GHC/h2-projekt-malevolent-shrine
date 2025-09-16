using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class CreateVipRoomsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ExtraAmenities",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "VipServiceDescription",
                table: "Rooms");

            migrationBuilder.CreateTable(
                name: "VipRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    VipServiceDescription = table.Column<string>(type: "text", nullable: false),
                    ExtraAmenities = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VipRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VipRooms_Rooms_Id",
                        column: x => x.Id,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VipRooms");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Rooms",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraAmenities",
                table: "Rooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VipServiceDescription",
                table: "Rooms",
                type: "text",
                nullable: true);
        }
    }
}
