using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocLink.Data.Migrations
{
    /// <inheritdoc />
    public partial class ManyToMany_Offers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Specialists_SpecialistId",
                schema: "public",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_Offers_SpecialistId",
                schema: "public",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "SpecialistId",
                schema: "public",
                table: "Offers");

            migrationBuilder.CreateTable(
                name: "OfferSpecialist",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferSpecialist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferSpecialist_Offers_OfferId",
                        column: x => x.OfferId,
                        principalSchema: "public",
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OfferSpecialist_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalSchema: "public",
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfferSpecialist_OfferId",
                schema: "public",
                table: "OfferSpecialist",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferSpecialist_SpecialistId",
                schema: "public",
                table: "OfferSpecialist",
                column: "SpecialistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfferSpecialist",
                schema: "public");

            migrationBuilder.AddColumn<Guid>(
                name: "SpecialistId",
                schema: "public",
                table: "Offers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SpecialistId",
                schema: "public",
                table: "Offers",
                column: "SpecialistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Specialists_SpecialistId",
                schema: "public",
                table: "Offers",
                column: "SpecialistId",
                principalSchema: "public",
                principalTable: "Specialists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
