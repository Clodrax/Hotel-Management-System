using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hotel.Migrations
{
    /// <inheritdoc />
    public partial class CreateBookingRoomTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRoom_Rooms_RoomsId",
                table: "BookingRoom");

            migrationBuilder.DropIndex(
                name: "IX_BookingRoom_RoomsId",
                table: "BookingRoom");

            migrationBuilder.DropColumn(
                name: "RoomsId",
                table: "BookingRoom");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRoom_RoomId",
                table: "BookingRoom",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRoom_Rooms_RoomId",
                table: "BookingRoom",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRoom_Rooms_RoomId",
                table: "BookingRoom");

            migrationBuilder.DropIndex(
                name: "IX_BookingRoom_RoomId",
                table: "BookingRoom");

            migrationBuilder.AddColumn<int>(
                name: "RoomsId",
                table: "BookingRoom",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BookingRoom_RoomsId",
                table: "BookingRoom",
                column: "RoomsId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRoom_Rooms_RoomsId",
                table: "BookingRoom",
                column: "RoomsId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
