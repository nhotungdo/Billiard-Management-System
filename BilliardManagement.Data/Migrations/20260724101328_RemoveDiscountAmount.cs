using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilliardManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDiscountAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Invoices]') AND name = N'DiscountAmount') ALTER TABLE [Invoices] DROP COLUMN [DiscountAmount];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
