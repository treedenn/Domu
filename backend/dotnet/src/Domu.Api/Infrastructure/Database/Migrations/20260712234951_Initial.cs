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
                    initial_quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    current_quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    unit = table.Column<int>(type: "integer", nullable: false),
                    container_type = table.Column<int>(type: "integer", nullable: false),
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
                name: "household_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    archived = table.Column<bool>(type: "boolean", nullable: false)
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "household_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    target_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false),
                    request_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    client_app = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    client_platform = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    client_version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_household_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_household_events_household_members_actor_member_id",
                        column: x => x.actor_member_id,
                        principalTable: "household_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "household_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    invited_by_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_household_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_household_invitations_household_members_invited_by_member_id",
                        column: x => x.invited_by_member_id,
                        principalTable: "household_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_household_invitations_households_household_id",
                        column: x => x.household_id,
                        principalTable: "households",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shopping_lists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    archived_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shopping_lists", x => x.id);
                    table.ForeignKey(
                        name: "fk_shopping_lists_household_members_created_by_member_id",
                        column: x => x.created_by_member_id,
                        principalTable: "household_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shopping_lists_households_household_id",
                        column: x => x.household_id,
                        principalTable: "households",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shopping_list_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shopping_list_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    container_quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    container_unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    @checked = table.Column<bool>(name: "checked", type: "boolean", nullable: false),
                    checked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    checked_by_member_id = table.Column<Guid>(type: "uuid", nullable: true),
                    space_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    added_by_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sort_order = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shopping_list_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_shopping_list_items_household_members_added_by_member_id",
                        column: x => x.added_by_member_id,
                        principalTable: "household_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shopping_list_items_household_members_checked_by_member_id",
                        column: x => x.checked_by_member_id,
                        principalTable: "household_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shopping_list_items_households_household_id",
                        column: x => x.household_id,
                        principalTable: "households",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shopping_list_items_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shopping_list_items_shopping_lists_shopping_list_id",
                        column: x => x.shopping_list_id,
                        principalTable: "shopping_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shopping_list_items_spaces_space_id",
                        column: x => x.space_id,
                        principalTable: "spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_household_events_actor_id_occurred_at",
                table: "household_events",
                columns: new[] { "actor_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_household_events_actor_member_id_occurred_at",
                table: "household_events",
                columns: new[] { "actor_member_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_household_events_household_id_occurred_at",
                table: "household_events",
                columns: new[] { "household_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_household_events_occurred_at",
                table: "household_events",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_household_events_target_type_target_id",
                table: "household_events",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_household_invitations_household_id_email_status",
                table: "household_invitations",
                columns: new[] { "household_id", "email", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_household_invitations_invited_by_member_id",
                table: "household_invitations",
                column: "invited_by_member_id");

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
                name: "ix_item_entries_item_id",
                table: "item_entries",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_items_space_id_name",
                table: "items",
                columns: new[] { "space_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_added_by_member_id",
                table: "shopping_list_items",
                column: "added_by_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_checked_by_member_id",
                table: "shopping_list_items",
                column: "checked_by_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_household_id",
                table: "shopping_list_items",
                column: "household_id");

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_item_id",
                table: "shopping_list_items",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_shopping_list_id",
                table: "shopping_list_items",
                column: "shopping_list_id");

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_shopping_list_id_checked",
                table: "shopping_list_items",
                columns: new[] { "shopping_list_id", "checked" });

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_shopping_list_id_sort_order",
                table: "shopping_list_items",
                columns: new[] { "shopping_list_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_space_id",
                table: "shopping_list_items",
                column: "space_id");

            migrationBuilder.CreateIndex(
                name: "ix_shopping_lists_created_by_member_id",
                table: "shopping_lists",
                column: "created_by_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_shopping_lists_household_id",
                table: "shopping_lists",
                column: "household_id");

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
                name: "household_events");

            migrationBuilder.DropTable(
                name: "household_invitations");

            migrationBuilder.DropTable(
                name: "item_entries");

            migrationBuilder.DropTable(
                name: "shopping_list_items");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "shopping_lists");

            migrationBuilder.DropTable(
                name: "spaces");

            migrationBuilder.DropTable(
                name: "household_members");

            migrationBuilder.DropTable(
                name: "households");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
