using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domu.Api.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_CreatedAtExpiresAtAcceptedAt_HouseholdInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "household_invitations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expires_at",
                table: "household_invitations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "household_invitations");

            migrationBuilder.DropColumn(
                name: "expires_at",
                table: "household_invitations");
        }
    }
}
