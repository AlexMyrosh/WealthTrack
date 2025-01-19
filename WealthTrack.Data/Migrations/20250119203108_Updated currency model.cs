using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthTrack.Data.Migrations
{
    /// <inheritdoc />
    public partial class Updatedcurrencymodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Currencies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Currencies");
        }
    }
}
