using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MovedCustomMessagesToSchedulerConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MESSAGES_INSTANCES_Instanceid",
                table: "MESSAGES");

            migrationBuilder.DropIndex(
                name: "IX_MESSAGES_Instanceid",
                table: "MESSAGES");

            migrationBuilder.DropColumn(
                name: "Instanceid",
                table: "MESSAGES");

            migrationBuilder.DropColumn(
                name: "restartInterval",
                table: "INSTANCES");

            migrationBuilder.DropColumn(
                name: "restartOnUpdate",
                table: "INSTANCES");

            migrationBuilder.AddColumn<int>(
                name: "restartInterval",
                table: "SCHEDULER_CONFIGS",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "restartOnUpdate",
                table: "SCHEDULER_CONFIGS",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SchedulerConfigId",
                table: "MESSAGES",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MESSAGES_SchedulerConfigId",
                table: "MESSAGES",
                column: "SchedulerConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_MESSAGES_SCHEDULER_CONFIGS_SchedulerConfigId",
                table: "MESSAGES",
                column: "SchedulerConfigId",
                principalTable: "SCHEDULER_CONFIGS",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MESSAGES_SCHEDULER_CONFIGS_SchedulerConfigId",
                table: "MESSAGES");

            migrationBuilder.DropIndex(
                name: "IX_MESSAGES_SchedulerConfigId",
                table: "MESSAGES");

            migrationBuilder.DropColumn(
                name: "restartInterval",
                table: "SCHEDULER_CONFIGS");

            migrationBuilder.DropColumn(
                name: "restartOnUpdate",
                table: "SCHEDULER_CONFIGS");

            migrationBuilder.DropColumn(
                name: "SchedulerConfigId",
                table: "MESSAGES");

            migrationBuilder.AddColumn<int>(
                name: "Instanceid",
                table: "MESSAGES",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "restartInterval",
                table: "INSTANCES",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "restartOnUpdate",
                table: "INSTANCES",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MESSAGES_Instanceid",
                table: "MESSAGES",
                column: "Instanceid");

            migrationBuilder.AddForeignKey(
                name: "FK_MESSAGES_INSTANCES_Instanceid",
                table: "MESSAGES",
                column: "Instanceid",
                principalTable: "INSTANCES",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
