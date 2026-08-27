using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdventureGuildAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeSocialRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_partyinvites_partyid",
                table: "partyinvites");

            migrationBuilder.DropIndex(
                name: "ix_guildrequests_requestid",
                table: "guildrequests");

            migrationBuilder.DropIndex(
                name: "ix_friendships_requestid",
                table: "friendships");

            migrationBuilder.AddColumn<int>(
                name: "inviterid",
                table: "partyinvites",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE partyinvites AS invite
                SET inviterid = users.id
                FROM users
                WHERE users.username = invite.invitename;");

            migrationBuilder.DropColumn(
                name: "invitename",
                table: "partyinvites");

            migrationBuilder.AlterColumn<int>(
                name: "inviterid",
                table: "partyinvites",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_partyinvites",
                table: "partyinvites",
                columns: new[] { "partyid", "acceptid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_guildrequests",
                table: "guildrequests",
                columns: new[] { "requestid", "guildid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_friendships",
                table: "friendships",
                columns: new[] { "requestid", "acceptid" });

            migrationBuilder.CreateIndex(
                name: "ix_partyinvites_inviterid",
                table: "partyinvites",
                column: "inviterid");

            migrationBuilder.AddForeignKey(
                name: "fk_partyinvites_users_inviterid",
                table: "partyinvites",
                column: "inviterid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_partyinvites_users_inviterid",
                table: "partyinvites");

            migrationBuilder.DropPrimaryKey(
                name: "pk_partyinvites",
                table: "partyinvites");

            migrationBuilder.DropIndex(
                name: "ix_partyinvites_inviterid",
                table: "partyinvites");

            migrationBuilder.DropPrimaryKey(
                name: "pk_guildrequests",
                table: "guildrequests");

            migrationBuilder.DropPrimaryKey(
                name: "pk_friendships",
                table: "friendships");

            migrationBuilder.DropColumn(
                name: "inviterid",
                table: "partyinvites");

            migrationBuilder.AddColumn<string>(
                name: "invitename",
                table: "partyinvites",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_partyinvites_partyid",
                table: "partyinvites",
                column: "partyid");

            migrationBuilder.CreateIndex(
                name: "ix_guildrequests_requestid",
                table: "guildrequests",
                column: "requestid");

            migrationBuilder.CreateIndex(
                name: "ix_friendships_requestid",
                table: "friendships",
                column: "requestid");
        }
    }
}
