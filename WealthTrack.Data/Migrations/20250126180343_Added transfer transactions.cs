using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthTrack.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addedtransfertransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,9)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TransactionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SourceWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferTransactions");
        }
    }
}
