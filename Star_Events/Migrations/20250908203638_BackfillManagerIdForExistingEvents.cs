using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Star_Events.Migrations
{
    /// <inheritdoc />
    public partial class BackfillManagerIdForExistingEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill ManagerId for existing events
            // First, get the first manager user ID from AspNetUsers table
            migrationBuilder.Sql(@"
                UPDATE Events 
                SET ManagerId = (
                    SELECT TOP 1 u.Id 
                    FROM AspNetUsers u 
                    INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId 
                    INNER JOIN AspNetRoles r ON ur.RoleId = r.Id 
                    WHERE r.Name = 'Manager'
                )
                WHERE ManagerId = '' OR ManagerId IS NULL
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
