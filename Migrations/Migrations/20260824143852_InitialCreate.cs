using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BANS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BanId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemainingTime = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BANS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "INSTANCES",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    instanceId = table.Column<int>(type: "INTEGER", nullable: false),
                    serverFolder = table.Column<string>(type: "TEXT", nullable: false),
                    hostName = table.Column<string>(type: "TEXT", nullable: false),
                    missionName = table.Column<string>(type: "TEXT", nullable: false),
                    vanillaMissionName = table.Column<string>(type: "TEXT", nullable: false),
                    missionTemplateName = table.Column<string>(type: "TEXT", nullable: false),
                    serverConfigName = table.Column<string>(type: "TEXT", nullable: false),
                    profileName = table.Column<string>(type: "TEXT", nullable: false),
                    steamPort = table.Column<int>(type: "INTEGER", nullable: false),
                    serverPort = table.Column<int>(type: "INTEGER", nullable: false),
                    steamQueryPort = table.Column<int>(type: "INTEGER", nullable: false),
                    RConPort = table.Column<int>(type: "INTEGER", nullable: false),
                    RConPassword = table.Column<string>(type: "TEXT", nullable: false),
                    cpuCount = table.Column<int>(type: "INTEGER", nullable: false),
                    noFilePatching = table.Column<bool>(type: "INTEGER", nullable: false),
                    doLogs = table.Column<bool>(type: "INTEGER", nullable: false),
                    adminLog = table.Column<bool>(type: "INTEGER", nullable: false),
                    freezeCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    netLog = table.Column<bool>(type: "INTEGER", nullable: false),
                    limitFPS = table.Column<int>(type: "INTEGER", nullable: false),
                    mapName = table.Column<string>(type: "TEXT", nullable: false),
                    restartOnUpdate = table.Column<bool>(type: "INTEGER", nullable: false),
                    restartInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    autoStartServer = table.Column<bool>(type: "INTEGER", nullable: false),
                    makeBackups = table.Column<bool>(type: "INTEGER", nullable: false),
                    deleteBackups = table.Column<bool>(type: "INTEGER", nullable: false),
                    backupPath = table.Column<string>(type: "TEXT", nullable: false),
                    maxKeepTime = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INSTANCES", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MODS",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    workshopID = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MODS", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PLAYERS",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Uid = table.Column<string>(type: "TEXT", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ip = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLAYERS", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "SCHEDULER_CONFIGS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UseNickFilter = table.Column<bool>(type: "INTEGER", nullable: false),
                    FilteredNickMsg = table.Column<string>(type: "TEXT", nullable: false),
                    BadNames = table.Column<string>(type: "TEXT", nullable: false),
                    Timeout = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCHEDULER_CONFIGS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "STEAM_CREDENTIALS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SteamUsername = table.Column<string>(type: "TEXT", nullable: false),
                    SteamPassword = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STEAM_CREDENTIALS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MESSAGES",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsTimeOfDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    WaitTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Interval = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Instanceid = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MESSAGES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MESSAGES_INSTANCES_Instanceid",
                        column: x => x.Instanceid,
                        principalTable: "INSTANCES",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "INSTANCE_SERVER_MODS",
                columns: table => new
                {
                    InstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INSTANCE_SERVER_MODS", x => new { x.InstanceId, x.ModId });
                    table.ForeignKey(
                        name: "FK_INSTANCE_SERVER_MODS_INSTANCES_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "INSTANCES",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_INSTANCE_SERVER_MODS_MODS_ModId",
                        column: x => x.ModId,
                        principalTable: "MODS",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SERVER_PLAYERS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BanId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsWhitelisted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBanned = table.Column<bool>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SERVER_PLAYERS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SERVER_PLAYERS_BANS_BanId",
                        column: x => x.BanId,
                        principalTable: "BANS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SERVER_PLAYERS_INSTANCES_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "INSTANCES",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SERVER_PLAYERS_PLAYERS_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "PLAYERS",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_INSTANCE_SERVER_MODS_ModId",
                table: "INSTANCE_SERVER_MODS",
                column: "ModId");

            migrationBuilder.CreateIndex(
                name: "IX_MESSAGES_Instanceid",
                table: "MESSAGES",
                column: "Instanceid");

            migrationBuilder.CreateIndex(
                name: "IX_SERVER_PLAYERS_BanId",
                table: "SERVER_PLAYERS",
                column: "BanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SERVER_PLAYERS_InstanceId",
                table: "SERVER_PLAYERS",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_SERVER_PLAYERS_PlayerId",
                table: "SERVER_PLAYERS",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "INSTANCE_SERVER_MODS");

            migrationBuilder.DropTable(
                name: "MESSAGES");

            migrationBuilder.DropTable(
                name: "SCHEDULER_CONFIGS");

            migrationBuilder.DropTable(
                name: "SERVER_PLAYERS");

            migrationBuilder.DropTable(
                name: "STEAM_CREDENTIALS");

            migrationBuilder.DropTable(
                name: "MODS");

            migrationBuilder.DropTable(
                name: "BANS");

            migrationBuilder.DropTable(
                name: "INSTANCES");

            migrationBuilder.DropTable(
                name: "PLAYERS");
        }
    }
}
