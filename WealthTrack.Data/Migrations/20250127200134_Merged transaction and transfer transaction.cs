using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthTrack.Data.Migrations
{
    /// <inheritdoc />
    public partial class Mergedtransactionandtransfertransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "SourceWalletId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TargetWalletId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceWalletId",
                table: "Transactions",
                column: "SourceWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TargetWalletId",
                table: "Transactions",
                column: "TargetWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_SourceWalletId",
                table: "Transactions",
                column: "SourceWalletId",
                principalTable: "Wallets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_TargetWalletId",
                table: "Transactions",
                column: "TargetWalletId",
                principalTable: "Wallets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_SourceWalletId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_TargetWalletId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SourceWalletId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TargetWalletId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SourceWalletId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TargetWalletId",
                table: "Transactions");

            migrationBuilder.CreateTable(
                name: "TransferTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TransactionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferTransactions_Wallets_SourceWalletId",
                        column: x => x.SourceWalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransferTransactions_Wallets_TargetWalletId",
                        column: x => x.TargetWalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransactions_SourceWalletId",
                table: "TransferTransactions",
                column: "SourceWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferTransactions_TargetWalletId",
                table: "TransferTransactions",
                column: "TargetWalletId");
        }
    }
}
