using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domu.Api.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "households",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    subscription_plan = table.Column<int>(type: "integer", nullable: false),
                    subscription_status = table.Column<int>(type: "integer", nullable: false),
                    subscription_current_period_ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    subscription_cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_households", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    category = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    barcode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_identifier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spaces", x => x.id);
                    table.ForeignKey(
                        name: "fk_spaces_households_household_id",
                        column: x => x.household_id,
                        principalTable: "households",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_spaces_spaces_parent_id",
                        column: x => x.parent_id,
                        principalTable: "spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    acquisition_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    expiration_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_entries_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "household_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_household_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_household_invitations_households_household_id",
                        column: x => x.household_id,
                        principalTable: "households",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_household_invitations_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "household_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_household_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_household_members_households_household_id",
                        column: x => x.household_id,
                        principalTable: "households",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_household_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_household_invitations_household_id_email_status",
                table: "household_invitations",
                columns: new[] { "household_id", "email", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_household_invitations_invited_by_user_id",
                table: "household_invitations",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_household_invitations_token",
                table: "household_invitations",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_household_members_household_id_user_id",
                table: "household_members",
                columns: new[] { "household_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_household_members_user_id",
                table: "household_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_households_owner_id_name",
                table: "households",
                columns: new[] { "owner_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_item_entries_item_id",
                table: "item_entries",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_items_space_id_name",
                table: "items",
                columns: new[] { "space_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_spaces_household_id_parent_id_name",
                table: "spaces",
                columns: new[] { "household_id", "parent_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_spaces_parent_id",
                table: "spaces",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_external_identifier",
                table: "users",
                column: "external_identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "household_invitations");

            migrationBuilder.DropTable(
                name: "household_members");

            migrationBuilder.DropTable(
                name: "item_entries");

            migrationBuilder.DropTable(
                name: "spaces");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "households");
        }
    }
}
