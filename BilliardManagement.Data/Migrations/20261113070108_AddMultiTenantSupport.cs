using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilliardManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ProductName",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Combos_ComboCode",
                table: "Combos");

            migrationBuilder.DropIndex(
                name: "IX_BilliardTables_TableName",
                table: "BilliardTables");

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "TableStatusHistories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "TableSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Shifts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Combos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "ComboItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "BilliardTables",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                });

            // Legacy data is assigned to the default store.
            migrationBuilder.Sql(
                "INSERT INTO [Stores] ([Id], [Name], [Address], [PhoneNumber], [Email], [LogoUrl], [IsActive], [CreatedAt]) " +
                "VALUES ('00000000-0000-0000-0000-000000000001', N'Cửa hàng mặc định', NULL, NULL, NULL, NULL, 1, GETUTCDATE());");

            migrationBuilder.Sql(
                "UPDATE [Users] SET [StoreId] = '00000000-0000-0000-0000-000000000001' WHERE [StoreId] IS NULL AND [Role] <> 0;");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StoreId",
                table: "Users",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_TableStatusHistories_StoreId",
                table: "TableStatusHistories",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_TableSessions_StoreId",
                table: "TableSessions",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_StoreId",
                table: "Shifts",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId",
                table: "Products",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId_ProductName",
                table: "Products",
                columns: new[] { "StoreId", "ProductName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_StoreId",
                table: "Customers",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_StoreId_PhoneNumber",
                table: "Customers",
                columns: new[] { "StoreId", "PhoneNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Combos_StoreId",
                table: "Combos",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Combos_StoreId_ComboCode",
                table: "Combos",
                columns: new[] { "StoreId", "ComboCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComboItems_StoreId",
                table: "ComboItems",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_StoreId",
                table: "Categories",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_BilliardTables_StoreId",
                table: "BilliardTables",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_BilliardTables_StoreId_TableName",
                table: "BilliardTables",
                columns: new[] { "StoreId", "TableName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Name",
                table: "Stores",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_BilliardTables_Stores_StoreId",
                table: "BilliardTables",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Stores_StoreId",
                table: "Categories",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComboItems_Stores_StoreId",
                table: "ComboItems",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Combos_Stores_StoreId",
                table: "Combos",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Stores_StoreId",
                table: "Customers",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Stores_StoreId",
                table: "Shifts",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TableSessions_Stores_StoreId",
                table: "TableSessions",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TableStatusHistories_Stores_StoreId",
                table: "TableStatusHistories",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Stores_StoreId",
                table: "Users",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BilliardTables_Stores_StoreId",
                table: "BilliardTables");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Stores_StoreId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_ComboItems_Stores_StoreId",
                table: "ComboItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Combos_Stores_StoreId",
                table: "Combos");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Stores_StoreId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Stores_StoreId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Stores_StoreId",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_TableSessions_Stores_StoreId",
                table: "TableSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_TableStatusHistories_Stores_StoreId",
                table: "TableStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Stores_StoreId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropIndex(
                name: "IX_Users_StoreId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TableStatusHistories_StoreId",
                table: "TableStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_TableSessions_StoreId",
                table: "TableSessions");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_StoreId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreId_ProductName",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Customers_StoreId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_StoreId_PhoneNumber",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Combos_StoreId",
                table: "Combos");

            migrationBuilder.DropIndex(
                name: "IX_Combos_StoreId_ComboCode",
                table: "Combos");

            migrationBuilder.DropIndex(
                name: "IX_ComboItems_StoreId",
                table: "ComboItems");

            migrationBuilder.DropIndex(
                name: "IX_Categories_StoreId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_BilliardTables_StoreId",
                table: "BilliardTables");

            migrationBuilder.DropIndex(
                name: "IX_BilliardTables_StoreId_TableName",
                table: "BilliardTables");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "TableStatusHistories");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "TableSessions");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Combos");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "ComboItems");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "BilliardTables");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductName",
                table: "Products",
                column: "ProductName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Combos_ComboCode",
                table: "Combos",
                column: "ComboCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilliardTables_TableName",
                table: "BilliardTables",
                column: "TableName",
                unique: true);
        }
    }
}
