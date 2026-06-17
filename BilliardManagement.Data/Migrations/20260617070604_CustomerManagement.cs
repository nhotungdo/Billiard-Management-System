using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilliardManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class CustomerManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "TableSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalVisits = table.Column<int>(type: "int", nullable: false),
                    TotalPlayHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalSpent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FirstVisitDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastVisitDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableSessions_CustomerId",
                table: "TableSessions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TableSessions_Customers_CustomerId",
                table: "TableSessions",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_TableSessions_Customers_CustomerId",
                table: "TableSessions");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_TableSessions_CustomerId",
                table: "TableSessions");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "TableSessions");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Invoices");
        }
    }
}
