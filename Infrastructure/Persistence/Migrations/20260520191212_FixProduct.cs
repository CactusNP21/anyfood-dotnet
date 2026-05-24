using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavedRecipes_Recipes_RecipeId",
                table: "SavedRecipes");

            migrationBuilder.DropIndex(
                name: "IX_SavedRecipes_RecipeId",
                table: "SavedRecipes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DayPlanEntry_RecipeVersionOrProductVersion",
                table: "DayPlanEntries");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "SavedRecipes");

            migrationBuilder.CreateIndex(
                name: "IX_SavedRecipes_RecipeVersionId",
                table: "SavedRecipes",
                column: "RecipeVersionId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DayPlanEntry_RecipeVersionOrProductVersion",
                table: "DayPlanEntries",
                sql: "(\"RecipeVersionId\" IS NOT NULL) != (\"ProductId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedRecipes_RecipeVersions_RecipeVersionId",
                table: "SavedRecipes",
                column: "RecipeVersionId",
                principalTable: "RecipeVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavedRecipes_RecipeVersions_RecipeVersionId",
                table: "SavedRecipes");

            migrationBuilder.DropIndex(
                name: "IX_SavedRecipes_RecipeVersionId",
                table: "SavedRecipes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DayPlanEntry_RecipeVersionOrProductVersion",
                table: "DayPlanEntries");

            migrationBuilder.AddColumn<int>(
                name: "RecipeId",
                table: "SavedRecipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SavedRecipes_RecipeId",
                table: "SavedRecipes",
                column: "RecipeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DayPlanEntry_RecipeVersionOrProductVersion",
                table: "DayPlanEntries",
                sql: "(\"RecipeVersionId\" IS NOT NULL) != (\"ProductVersionId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedRecipes_Recipes_RecipeId",
                table: "SavedRecipes",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
