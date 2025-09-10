using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Star_Events.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToTicketSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TicketSales",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TicketSales");
        }
    }
}
