using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilliardManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComboToTableSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComboHours",
                table: "TableSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ComboId",
                table: "TableSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVip",
                table: "Combos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TableSessions_ComboId",
                table: "TableSessions",
                column: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_TableSessions_Combos_ComboId",
                table: "TableSessions",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableSessions_Combos_ComboId",
                table: "TableSessions");

            migrationBuilder.DropIndex(
                name: "IX_TableSessions_ComboId",
                table: "TableSessions");

            migrationBuilder.DropColumn(
                name: "ComboHours",
                table: "TableSessions");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "TableSessions");

            migrationBuilder.DropColumn(
                name: "IsVip",
                table: "Combos");
        }
    }
}
