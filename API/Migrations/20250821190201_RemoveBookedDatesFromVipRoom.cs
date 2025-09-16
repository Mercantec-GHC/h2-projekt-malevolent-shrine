using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBookedDatesFromVipRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Rooms_VipRoomId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_VipRoomId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VipRoomId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BookedDates",
                table: "Rooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VipRoomId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<List<DateTime>>(
                name: "BookedDates",
                table: "Rooms",
                type: "timestamp without time zone[]",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "VipRoomId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Users_VipRoomId",
                table: "Users",
                column: "VipRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Rooms_VipRoomId",
                table: "Users",
                column: "VipRoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }
    }
}
