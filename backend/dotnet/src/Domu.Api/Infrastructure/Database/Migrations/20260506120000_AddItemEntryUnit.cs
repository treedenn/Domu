using Domu.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domu.Api.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260506120000_AddItemEntryAmountsAndContainer")]
    public partial class AddItemEntryAmountsAndContainer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "item_entries",
                newName: "initial_quantity");

            migrationBuilder.AlterColumn<decimal>(
                name: "initial_quantity",
                table: "item_entries",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "current_quantity",
                table: "item_entries",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "unit",
                table: "item_entries",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "container_type",
                table: "item_entries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE item_entries SET current_quantity = initial_quantity;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "container_type",
                table: "item_entries");

            migrationBuilder.DropColumn(
                name: "current_quantity",
                table: "item_entries");

            migrationBuilder.DropColumn(
                name: "unit",
                table: "item_entries");

            migrationBuilder.AlterColumn<int>(
                name: "initial_quantity",
                table: "item_entries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.RenameColumn(
                name: "initial_quantity",
                table: "item_entries",
                newName: "quantity");
        }
    }
}
