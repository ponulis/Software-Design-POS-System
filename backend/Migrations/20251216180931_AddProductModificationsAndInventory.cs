using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProductModificationsAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificationValuesJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductModifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductModifications_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductModificationAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ModificationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModificationAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductModificationAssignments_ProductModifications_ModificationId",
                        column: x => x.ModificationId,
                        principalTable: "ProductModifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductModificationAssignments_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductModificationValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModificationId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModificationValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductModificationValues_ProductModifications_ModificationId",
                        column: x => x.ModificationId,
                        principalTable: "ProductModifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryModificationValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    ModificationValueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryModificationValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryModificationValues_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryModificationValues_ProductModificationValues_ModificationValueId",
                        column: x => x.ModificationValueId,
                        principalTable: "ProductModificationValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ProductId",
                table: "InventoryItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryModificationValues_InventoryItemId",
                table: "InventoryModificationValues",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryModificationValues_InventoryItemId_ModificationValueId",
                table: "InventoryModificationValues",
                columns: new[] { "InventoryItemId", "ModificationValueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryModificationValues_ModificationValueId",
                table: "InventoryModificationValues",
                column: "ModificationValueId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModificationAssignments_ModificationId",
                table: "ProductModificationAssignments",
                column: "ModificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModificationAssignments_ProductId",
                table: "ProductModificationAssignments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModificationAssignments_ProductId_ModificationId",
                table: "ProductModificationAssignments",
                columns: new[] { "ProductId", "ModificationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductModifications_BusinessId",
                table: "ProductModifications",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModifications_BusinessId_Name",
                table: "ProductModifications",
                columns: new[] { "BusinessId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductModificationValues_ModificationId",
                table: "ProductModificationValues",
                column: "ModificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModificationValues_ModificationId_Value",
                table: "ProductModificationValues",
                columns: new[] { "ModificationId", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryModificationValues");

            migrationBuilder.DropTable(
                name: "ProductModificationAssignments");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "ProductModificationValues");

            migrationBuilder.DropTable(
                name: "ProductModifications");
        }
    }
}
