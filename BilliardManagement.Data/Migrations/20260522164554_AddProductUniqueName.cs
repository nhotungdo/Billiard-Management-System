using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilliardManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductUniqueName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                WITH CTE AS (
                    SELECT Id,
                           ROW_NUMBER() OVER(
                               PARTITION BY LTRIM(RTRIM(LOWER(ProductName)))
                               ORDER BY Id
                           ) as rn
                    FROM Products
                )
                DELETE FROM Products WHERE Id IN (SELECT Id FROM CTE WHERE rn > 1);
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductName",
                table: "Products",
                column: "ProductName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ProductName",
                table: "Products");
        }
    }
}
