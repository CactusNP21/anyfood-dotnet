using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RecipeHasVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LatestVersionId",
                table: "Recipes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_LatestVersionId",
                table: "Recipes",
                column: "LatestVersionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_RecipeVersions_LatestVersionId",
                table: "Recipes",
                column: "LatestVersionId",
                principalTable: "RecipeVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_RecipeVersions_LatestVersionId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_LatestVersionId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "LatestVersionId",
                table: "Recipes");
        }
    }
}
