using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Star_Events.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdToTicketSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "TicketSales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "TicketSales");
        }
    }
}
