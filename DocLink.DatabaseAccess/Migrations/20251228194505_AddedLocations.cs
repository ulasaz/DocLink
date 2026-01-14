using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocLink.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Location",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Post = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecialistLocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialistId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialistLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialistLocation_Location_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "public",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecialistLocation_Specialists_SpecialistId",
                        column: x => x.SpecialistId,
                        principalSchema: "public",
                        principalTable: "Specialists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialistLocation_LocationId",
                schema: "public",
                table: "SpecialistLocation",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialistLocation_SpecialistId",
                schema: "public",
                table: "SpecialistLocation",
                column: "SpecialistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialistLocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Location",
                schema: "public");
        }
    }
}
