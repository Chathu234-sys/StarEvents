using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Star_Events.Migrations
{
    /// <inheritdoc />
    public partial class addManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsersId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_UsersId",
                table: "Events",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Users_UsersId",
                table: "Events",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Users_UsersId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_UsersId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Events");
        }
    }
}
