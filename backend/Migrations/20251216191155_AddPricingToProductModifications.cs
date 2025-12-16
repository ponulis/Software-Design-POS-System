using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingToProductModifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FixedPriceAddition",
                table: "ProductModifications",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentagePriceIncrease",
                table: "ProductModifications",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceType",
                table: "ProductModifications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixedPriceAddition",
                table: "ProductModifications");

            migrationBuilder.DropColumn(
                name: "PercentagePriceIncrease",
                table: "ProductModifications");

            migrationBuilder.DropColumn(
                name: "PriceType",
                table: "ProductModifications");
        }
    }
}
