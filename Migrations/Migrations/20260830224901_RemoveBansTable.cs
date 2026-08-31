using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBansTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SERVER_PLAYERS_BANS_BanId",
                table: "SERVER_PLAYERS");

            migrationBuilder.DropTable(
                name: "BANS");

            migrationBuilder.DropIndex(
                name: "IX_SERVER_PLAYERS_BanId",
                table: "SERVER_PLAYERS");

            migrationBuilder.DropColumn(
                name: "BanId",
                table: "SERVER_PLAYERS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BanId",
                table: "SERVER_PLAYERS",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BANS",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    BanId = table.Column<int>(type: "INTEGER", nullable: false),
                    InstanceId = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerGuid = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    RemainingTime = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BANS", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SERVER_PLAYERS_BanId",
                table: "SERVER_PLAYERS",
                column: "BanId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SERVER_PLAYERS_BANS_BanId",
                table: "SERVER_PLAYERS",
                column: "BanId",
                principalTable: "BANS",
                principalColumn: "Id");
        }
    }
}
