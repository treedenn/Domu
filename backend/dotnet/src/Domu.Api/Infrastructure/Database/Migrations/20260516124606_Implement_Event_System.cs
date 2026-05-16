using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domu.Api.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Implement_Event_System : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    target_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    request_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    client_app = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    client_platform = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    client_version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    client_build = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_events_actor_user_id_occurred_at",
                table: "user_events",
                columns: new[] { "actor_user_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_events_household_id_occurred_at",
                table: "user_events",
                columns: new[] { "household_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_events_occurred_at",
                table: "user_events",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_events_target_type_target_id",
                table: "user_events",
                columns: new[] { "target_type", "target_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_events");
        }
    }
}
