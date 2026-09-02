using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddedPositionToModsJoinTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "INSTANCE_SERVER_MODS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "serverModsInstanceId",
                table: "INSTANCE_SERVER_MODS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "serverModsModId",
                table: "INSTANCE_SERVER_MODS",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "INSTANCE_CLIENT_MODS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "clientModsInstanceId",
                table: "INSTANCE_CLIENT_MODS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "clientModsModId",
                table: "INSTANCE_CLIENT_MODS",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_INSTANCE_SERVER_MODS_serverModsInstanceId_serverModsModId",
                table: "INSTANCE_SERVER_MODS",
                columns: new[] { "serverModsInstanceId", "serverModsModId" });

            migrationBuilder.CreateIndex(
                name: "IX_INSTANCE_CLIENT_MODS_clientModsInstanceId_clientModsModId",
                table: "INSTANCE_CLIENT_MODS",
                columns: new[] { "clientModsInstanceId", "clientModsModId" });

            migrationBuilder.AddForeignKey(
                name: "FK_INSTANCE_CLIENT_MODS_INSTANCE_CLIENT_MODS_clientModsInstanceId_clientModsModId",
                table: "INSTANCE_CLIENT_MODS",
                columns: new[] { "clientModsInstanceId", "clientModsModId" },
                principalTable: "INSTANCE_CLIENT_MODS",
                principalColumns: new[] { "InstanceId", "ModId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_INSTANCE_SERVER_MODS_INSTANCE_SERVER_MODS_serverModsInstanceId_serverModsModId",
                table: "INSTANCE_SERVER_MODS",
                columns: new[] { "serverModsInstanceId", "serverModsModId" },
                principalTable: "INSTANCE_SERVER_MODS",
                principalColumns: new[] { "InstanceId", "ModId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_INSTANCE_CLIENT_MODS_INSTANCE_CLIENT_MODS_clientModsInstanceId_clientModsModId",
                table: "INSTANCE_CLIENT_MODS");

            migrationBuilder.DropForeignKey(
                name: "FK_INSTANCE_SERVER_MODS_INSTANCE_SERVER_MODS_serverModsInstanceId_serverModsModId",
                table: "INSTANCE_SERVER_MODS");

            migrationBuilder.DropIndex(
                name: "IX_INSTANCE_SERVER_MODS_serverModsInstanceId_serverModsModId",
                table: "INSTANCE_SERVER_MODS");

            migrationBuilder.DropIndex(
                name: "IX_INSTANCE_CLIENT_MODS_clientModsInstanceId_clientModsModId",
                table: "INSTANCE_CLIENT_MODS");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "INSTANCE_SERVER_MODS");

            migrationBuilder.DropColumn(
                name: "serverModsInstanceId",
                table: "INSTANCE_SERVER_MODS");

            migrationBuilder.DropColumn(
                name: "serverModsModId",
                table: "INSTANCE_SERVER_MODS");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "INSTANCE_CLIENT_MODS");

            migrationBuilder.DropColumn(
                name: "clientModsInstanceId",
                table: "INSTANCE_CLIENT_MODS");

            migrationBuilder.DropColumn(
                name: "clientModsModId",
                table: "INSTANCE_CLIENT_MODS");
        }
    }
}
