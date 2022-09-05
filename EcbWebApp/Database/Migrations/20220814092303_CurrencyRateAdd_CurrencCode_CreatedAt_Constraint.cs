using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcbWebApp.Database.Migrations
{
    public partial class CurrencyRateAdd_CurrencCode_CreatedAt_Constraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRates_CurrencyCode_CreatedAt",
                table: "CurrencyRates",
                columns: new[] { "CurrencyCode", "CreatedAt" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CurrencyRates_CurrencyCode_CreatedAt",
                table: "CurrencyRates");
        }
    }
}
