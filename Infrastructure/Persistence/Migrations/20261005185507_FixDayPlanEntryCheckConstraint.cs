using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDayPlanEntryCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DayPlanEntry_RecipeVersionOrProductVersion",
                table: "DayPlanEntries");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DayPlanEntry_RecipeVersionOrProductVersion",
                table: "DayPlanEntries",
                sql: "(\"RecipeId\" IS NOT NULL) != (\"ProductId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DayPlanEntry_RecipeVersionOrProductVersion",
                table: "DayPlanEntries");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DayPlanEntry_RecipeVersionOrProductVersion",
                table: "DayPlanEntries",
                // Postgres already rewrote the stored constraint to "RecipeId" when the column was renamed;
                // only the EF model text was stale, so Down restores the same DB state.
                sql: "(\"RecipeId\" IS NOT NULL) != (\"ProductId\" IS NOT NULL)");
        }
    }
}
