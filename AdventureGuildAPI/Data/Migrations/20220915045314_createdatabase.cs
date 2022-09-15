using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AdventureGuildAPI.Data.Migrations
{
    public partial class createdatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parties",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "approvals",
                columns: table => new
                {
                    approverid = table.Column<int>(type: "integer", nullable: false),
                    questid = table.Column<int>(type: "integer", nullable: false),
                    approved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approvals", x => new { x.approverid, x.questid });
                });

            migrationBuilder.CreateTable(
                name: "friendships",
                columns: table => new
                {
                    requestid = table.Column<int>(type: "integer", nullable: false),
                    acceptid = table.Column<int>(type: "integer", nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "guildrequests",
                columns: table => new
                {
                    requestid = table.Column<int>(type: "integer", nullable: false),
                    guildid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    isprivate = table.Column<bool>(type: "boolean", nullable: false),
                    leaderid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guilds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    emailaddress = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<byte[]>(type: "bytea", nullable: false),
                    passwordsalt = table.Column<byte[]>(type: "bytea", nullable: false),
                    firstname = table.Column<string>(type: "text", nullable: false),
                    lastname = table.Column<string>(type: "text", nullable: false),
                    money = table.Column<int>(type: "integer", nullable: false),
                    guildid = table.Column<int>(type: "integer", nullable: true),
                    partyid = table.Column<int>(type: "integer", nullable: true),
                    role = table.Column<string>(type: "text", nullable: false),
                    verified = table.Column<bool>(type: "boolean", nullable: false),
                    refreshtoken = table.Column<string>(type: "text", nullable: true),
                    verificationtoken = table.Column<byte[]>(type: "bytea", nullable: false),
                    resetpasswordtoken = table.Column<byte[]>(type: "bytea", nullable: true),
                    resetpassexpires = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_guilds_guildid",
                        column: x => x.guildid,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_users_parties_partyid",
                        column: x => x.partyid,
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "partyinvites",
                columns: table => new
                {
                    partyid = table.Column<int>(type: "integer", nullable: false),
                    invitename = table.Column<string>(type: "text", nullable: false),
                    acceptid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "fk_partyinvites_parties_partyid",
                        column: x => x.partyid,
                        principalTable: "parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_partyinvites_users_acceptid",
                        column: x => x.acceptid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    createddatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quests", x => x.id);
                    table.ForeignKey(
                        name: "fk_quests_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questchecks",
                columns: table => new
                {
                    questid = table.Column<int>(type: "integer", nullable: false),
                    requestid = table.Column<int>(type: "integer", nullable: false),
                    partyid = table.Column<int>(type: "integer", nullable: true),
                    imageref = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_questchecks", x => x.questid);
                    table.ForeignKey(
                        name: "fk_questchecks_parties_partyid",
                        column: x => x.partyid,
                        principalTable: "parties",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_questchecks_quests_questid",
                        column: x => x.questid,
                        principalTable: "quests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_questchecks_users_requestid",
                        column: x => x.requestid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_approvals_questid",
                table: "approvals",
                column: "questid");

            migrationBuilder.CreateIndex(
                name: "ix_friendships_acceptid",
                table: "friendships",
                column: "acceptid");

            migrationBuilder.CreateIndex(
                name: "ix_friendships_requestid",
                table: "friendships",
                column: "requestid");

            migrationBuilder.CreateIndex(
                name: "ix_guildrequests_guildid",
                table: "guildrequests",
                column: "guildid");

            migrationBuilder.CreateIndex(
                name: "ix_guildrequests_requestid",
                table: "guildrequests",
                column: "requestid");

            migrationBuilder.CreateIndex(
                name: "ix_guilds_leaderid",
                table: "guilds",
                column: "leaderid");

            migrationBuilder.CreateIndex(
                name: "ix_guilds_name",
                table: "guilds",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_partyinvites_acceptid",
                table: "partyinvites",
                column: "acceptid");

            migrationBuilder.CreateIndex(
                name: "ix_partyinvites_partyid",
                table: "partyinvites",
                column: "partyid");

            migrationBuilder.CreateIndex(
                name: "ix_questchecks_partyid",
                table: "questchecks",
                column: "partyid");

            migrationBuilder.CreateIndex(
                name: "ix_questchecks_requestid",
                table: "questchecks",
                column: "requestid");

            migrationBuilder.CreateIndex(
                name: "ix_quests_userid",
                table: "quests",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "ix_users_emailaddress",
                table: "users",
                column: "emailaddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_guildid",
                table: "users",
                column: "guildid");

            migrationBuilder.CreateIndex(
                name: "ix_users_partyid",
                table: "users",
                column: "partyid");

            migrationBuilder.CreateIndex(
                name: "ix_users_resetpasswordtoken",
                table: "users",
                column: "resetpasswordtoken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_verificationtoken",
                table: "users",
                column: "verificationtoken",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_approvals_quests_questid",
                table: "approvals",
                column: "questid",
                principalTable: "quests",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_approvals_users_approverid",
                table: "approvals",
                column: "approverid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_friendships_users_acceptid",
                table: "friendships",
                column: "acceptid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_friendships_users_requestid",
                table: "friendships",
                column: "requestid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_guildrequests_guilds_guildid",
                table: "guildrequests",
                column: "guildid",
                principalTable: "guilds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_guildrequests_users_requestid",
                table: "guildrequests",
                column: "requestid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_guilds_users_leaderid",
                table: "guilds",
                column: "leaderid",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_guilds_users_leaderid",
                table: "guilds");

            migrationBuilder.DropTable(
                name: "approvals");

            migrationBuilder.DropTable(
                name: "friendships");

            migrationBuilder.DropTable(
                name: "guildrequests");

            migrationBuilder.DropTable(
                name: "partyinvites");

            migrationBuilder.DropTable(
                name: "questchecks");

            migrationBuilder.DropTable(
                name: "quests");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "guilds");

            migrationBuilder.DropTable(
                name: "parties");
        }
    }
}
