using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Star_Events.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSentToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationSent",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationSent",
                table: "Events");
        }
    }
}
