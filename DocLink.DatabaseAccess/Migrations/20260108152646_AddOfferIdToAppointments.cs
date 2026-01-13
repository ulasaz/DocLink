using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocLink.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferIdToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Street",
                schema: "public",
                table: "Location",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                schema: "public",
                table: "Location",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferId",
                schema: "public",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_OfferId",
                schema: "public",
                table: "Appointments",
                column: "OfferId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Offers_OfferId",
                schema: "public",
                table: "Appointments",
                column: "OfferId",
                principalSchema: "public",
                principalTable: "Offers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Offers_OfferId",
                schema: "public",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_OfferId",
                schema: "public",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "OfferId",
                schema: "public",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                schema: "public",
                table: "Location",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                schema: "public",
                table: "Location",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
