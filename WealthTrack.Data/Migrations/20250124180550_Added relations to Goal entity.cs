using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthTrack.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedrelationstoGoalentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalBudget_Goal_GoalId",
                table: "GoalBudget");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalCategory_Goal_GoalId",
                table: "GoalCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalWallet_Goal_GoalId",
                table: "GoalWallet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Goal",
                table: "Goal");

            migrationBuilder.RenameTable(
                name: "Goal",
                newName: "Goals");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Goals",
                table: "Goals",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalBudget_Goals_GoalId",
                table: "GoalBudget",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalCategory_Goals_GoalId",
                table: "GoalCategory",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalWallet_Goals_GoalId",
                table: "GoalWallet",
                column: "GoalId",
                principalTable: "Goals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoalBudget_Goals_GoalId",
                table: "GoalBudget");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalCategory_Goals_GoalId",
                table: "GoalCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_GoalWallet_Goals_GoalId",
                table: "GoalWallet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Goals",
                table: "Goals");

            migrationBuilder.RenameTable(
                name: "Goals",
                newName: "Goal");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Goal",
                table: "Goal",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoalBudget_Goal_GoalId",
                table: "GoalBudget",
                column: "GoalId",
                principalTable: "Goal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalCategory_Goal_GoalId",
                table: "GoalCategory",
                column: "GoalId",
                principalTable: "Goal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoalWallet_Goal_GoalId",
                table: "GoalWallet",
                column: "GoalId",
                principalTable: "Goal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
