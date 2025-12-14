using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_BusinessId_Role",
                table: "Users",
                columns: new[] { "BusinessId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BusinessId_Available",
                table: "Products",
                columns: new[] { "BusinessId", "Available" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BusinessId_Status",
                table: "Orders",
                columns: new[] { "BusinessId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BusinessId_Date",
                table: "Appointments",
                columns: new[] { "BusinessId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BusinessId_EmployeeId",
                table: "Appointments",
                columns: new[] { "BusinessId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Date_EmployeeId_BusinessId",
                table: "Appointments",
                columns: new[] { "Date", "EmployeeId", "BusinessId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_BusinessId_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Products_BusinessId_Available",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BusinessId_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BusinessId_Date",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BusinessId_EmployeeId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Date_EmployeeId_BusinessId",
                table: "Appointments");
        }
    }
}
