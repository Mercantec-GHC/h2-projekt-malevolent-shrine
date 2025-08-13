using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingAndVipRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_VipRoom_VipRoomId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VipRoom",
                table: "VipRoom");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "VipRoom");

            migrationBuilder.RenameTable(
                name: "VipRoom",
                newName: "Room");

            migrationBuilder.RenameColumn(
                name: "RoomNumber",
                table: "Room",
                newName: "Number");

            migrationBuilder.AddColumn<List<DateTime>>(
                name: "BookedDates",
                table: "Room",
                type: "timestamp with time zone[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Room",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Room",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Room",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "ExtraAmenities",
                table: "Room",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HotelId",
                table: "Room",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Room",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerNight",
                table: "Room",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Room",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "VipServiceDescription",
                table: "Room",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Room",
                table: "Room",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Hotel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotel", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Room_HotelId",
                table: "Room",
                column: "HotelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Room_Hotel_HotelId",
                table: "Room",
                column: "HotelId",
                principalTable: "Hotel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Room_VipRoomId",
                table: "Users",
                column: "VipRoomId",
                principalTable: "Room",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Room_Hotel_HotelId",
                table: "Room");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Room_VipRoomId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Hotel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Room",
                table: "Room");

            migrationBuilder.DropIndex(
                name: "IX_Room_HotelId",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "BookedDates",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "ExtraAmenities",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "PricePerNight",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "VipServiceDescription",
                table: "Room");

            migrationBuilder.RenameTable(
                name: "Room",
                newName: "VipRoom");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "VipRoom",
                newName: "RoomNumber");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "VipRoom",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VipRoom",
                table: "VipRoom",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_VipRoom_VipRoomId",
                table: "Users",
                column: "VipRoomId",
                principalTable: "VipRoom",
                principalColumn: "Id");
        }
    }
}
