using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domu.Api.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberArchivedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_household_invitations_users_invited_by_user_id",
                table: "household_invitations");

            migrationBuilder.RenameColumn(
                name: "created_by_user_id",
                table: "shopping_lists",
                newName: "created_by_member_id");

            migrationBuilder.RenameColumn(
                name: "checked_by_user_id",
                table: "shopping_list_items",
                newName: "checked_by_member_id");

            migrationBuilder.RenameColumn(
                name: "added_by_user_id",
                table: "shopping_list_items",
                newName: "added_by_member_id");

            migrationBuilder.RenameColumn(
                name: "invited_by_user_id",
                table: "household_invitations",
                newName: "invited_by_member_id");

            migrationBuilder.RenameIndex(
                name: "ix_household_invitations_invited_by_user_id",
                table: "household_invitations",
                newName: "ix_household_invitations_invited_by_member_id");

            migrationBuilder.AddColumn<bool>(
                name: "archived",
                table: "household_members",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_shopping_lists_created_by_member_id",
                table: "shopping_lists",
                column: "created_by_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_added_by_member_id",
                table: "shopping_list_items",
                column: "added_by_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_shopping_list_items_checked_by_member_id",
                table: "shopping_list_items",
                column: "checked_by_member_id");

            migrationBuilder.AddForeignKey(
                name: "fk_household_invitations_household_members_invited_by_member_id",
                table: "household_invitations",
                column: "invited_by_member_id",
                principalTable: "household_members",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_shopping_list_items_household_members_added_by_member_id",
                table: "shopping_list_items",
                column: "added_by_member_id",
                principalTable: "household_members",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_shopping_list_items_household_members_checked_by_member_id",
                table: "shopping_list_items",
                column: "checked_by_member_id",
                principalTable: "household_members",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_shopping_lists_household_members_created_by_member_id",
                table: "shopping_lists",
                column: "created_by_member_id",
                principalTable: "household_members",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_household_invitations_household_members_invited_by_member_id",
                table: "household_invitations");

            migrationBuilder.DropForeignKey(
                name: "fk_shopping_list_items_household_members_added_by_member_id",
                table: "shopping_list_items");

            migrationBuilder.DropForeignKey(
                name: "fk_shopping_list_items_household_members_checked_by_member_id",
                table: "shopping_list_items");

            migrationBuilder.DropForeignKey(
                name: "fk_shopping_lists_household_members_created_by_member_id",
                table: "shopping_lists");

            migrationBuilder.DropIndex(
                name: "ix_shopping_lists_created_by_member_id",
                table: "shopping_lists");

            migrationBuilder.DropIndex(
                name: "ix_shopping_list_items_added_by_member_id",
                table: "shopping_list_items");

            migrationBuilder.DropIndex(
                name: "ix_shopping_list_items_checked_by_member_id",
                table: "shopping_list_items");

            migrationBuilder.DropColumn(
                name: "archived",
                table: "household_members");

            migrationBuilder.RenameColumn(
                name: "created_by_member_id",
                table: "shopping_lists",
                newName: "created_by_user_id");

            migrationBuilder.RenameColumn(
                name: "checked_by_member_id",
                table: "shopping_list_items",
                newName: "checked_by_user_id");

            migrationBuilder.RenameColumn(
                name: "added_by_member_id",
                table: "shopping_list_items",
                newName: "added_by_user_id");

            migrationBuilder.RenameColumn(
                name: "invited_by_member_id",
                table: "household_invitations",
                newName: "invited_by_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_household_invitations_invited_by_member_id",
                table: "household_invitations",
                newName: "ix_household_invitations_invited_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_household_invitations_users_invited_by_user_id",
                table: "household_invitations",
                column: "invited_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
