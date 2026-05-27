using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BilliardManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTableStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableStatusHistories_BilliardTables_TableId",
                        column: x => x.TableId,
                        principalTable: "BilliardTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableStatusHistories_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableStatusHistories_ChangedById",
                table: "TableStatusHistories",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_TableStatusHistories_TableId",
                table: "TableStatusHistories",
                column: "TableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableStatusHistories");
        }
    }
}
