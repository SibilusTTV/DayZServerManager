using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddSeparateInstanceModTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "INSTANCE_CLIENT_MODS",
                columns: table => new
                {
                    InstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INSTANCE_CLIENT_MODS", x => new { x.InstanceId, x.ModId });
                    table.ForeignKey(
                        name: "FK_INSTANCE_CLIENT_MODS_INSTANCES_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "INSTANCES",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_INSTANCE_CLIENT_MODS_MODS_ModId",
                        column: x => x.ModId,
                        principalTable: "MODS",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_INSTANCE_CLIENT_MODS_ModId",
                table: "INSTANCE_CLIENT_MODS",
                column: "ModId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "INSTANCE_CLIENT_MODS");
        }
    }
}
