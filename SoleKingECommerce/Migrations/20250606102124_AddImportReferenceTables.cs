using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoleKingECommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddImportReferenceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromImportId = table.Column<int>(type: "int", nullable: false),
                    ToImportId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportReferences_Imports_FromImportId",
                        column: x => x.FromImportId,
                        principalTable: "Imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImportReferences_Imports_ToImportId",
                        column: x => x.ToImportId,
                        principalTable: "Imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportReferenceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportReferenceId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: false),
                    QuantityAdded = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportReferenceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportReferenceItems_ImportReferences_ImportReferenceId",
                        column: x => x.ImportReferenceId,
                        principalTable: "ImportReferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportReferenceItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportReferenceItems_ImportReferenceId",
                table: "ImportReferenceItems",
                column: "ImportReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportReferenceItems_ProductVariantId",
                table: "ImportReferenceItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportReferences_FromImportId",
                table: "ImportReferences",
                column: "FromImportId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportReferences_ToImportId",
                table: "ImportReferences",
                column: "ToImportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportReferenceItems");

            migrationBuilder.DropTable(
                name: "ImportReferences");
        }
    }
}
