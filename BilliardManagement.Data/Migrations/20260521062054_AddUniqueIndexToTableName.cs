using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilliardManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BilliardTables_TableName",
                table: "BilliardTables",
                column: "TableName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BilliardTables_TableName",
                table: "BilliardTables");
        }
    }
}
