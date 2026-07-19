using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVersionRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayPlanEntries_RecipeVersions_RecipeVersionId",
                table: "DayPlanEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_RecipeVersions_LatestVersionId",
                table: "Recipes");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedRecipes_RecipeVersions_RecipeVersionId",
                table: "SavedRecipes");
            
            migrationBuilder.Sql(@"
        UPDATE ""DayPlanEntries"" e
        SET ""RecipeVersionId"" = rv.""RecipeId""
        FROM ""RecipeVersions"" rv
        WHERE e.""RecipeVersionId"" = rv.""Id"";
    ");

            migrationBuilder.Sql(@"
        UPDATE ""SavedRecipes"" sr
        SET ""RecipeVersionId"" = rv.""RecipeId""
        FROM ""RecipeVersions"" rv
        WHERE sr.""RecipeVersionId"" = rv.""Id"";
    ");

            migrationBuilder.DropTable(
                name: "RecipeVersionIngredients");

            migrationBuilder.DropTable(
                name: "RecipeVersions");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_LatestVersionId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "LatestVersionId",
                table: "Recipes");

            migrationBuilder.RenameColumn(
                name: "RecipeVersionId",
                table: "SavedRecipes",
                newName: "RecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_SavedRecipes_RecipeVersionId",
                table: "SavedRecipes",
                newName: "IX_SavedRecipes_RecipeId");

            migrationBuilder.RenameColumn(
                name: "RecipeVersionId",
                table: "DayPlanEntries",
                newName: "RecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_DayPlanEntries_RecipeVersionId",
                table: "DayPlanEntries",
                newName: "IX_DayPlanEntries_RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DayPlanEntries_Recipes_RecipeId",
                table: "DayPlanEntries",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedRecipes_Recipes_RecipeId",
                table: "SavedRecipes",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayPlanEntries_Recipes_RecipeId",
                table: "DayPlanEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedRecipes_Recipes_RecipeId",
                table: "SavedRecipes");

            migrationBuilder.RenameColumn(
                name: "RecipeId",
                table: "SavedRecipes",
                newName: "RecipeVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_SavedRecipes_RecipeId",
                table: "SavedRecipes",
                newName: "IX_SavedRecipes_RecipeVersionId");

            migrationBuilder.RenameColumn(
                name: "RecipeId",
                table: "DayPlanEntries",
                newName: "RecipeVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_DayPlanEntries_RecipeId",
                table: "DayPlanEntries",
                newName: "IX_DayPlanEntries_RecipeVersionId");

            migrationBuilder.AddColumn<int>(
                name: "LatestVersionId",
                table: "Recipes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecipeVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    RecipeId = table.Column<int>(type: "integer", nullable: false),
                    Calories = table.Column<float>(type: "real", nullable: false),
                    Carbs = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    Fat = table.Column<float>(type: "real", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Portions = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<float>(type: "real", nullable: false),
                    Protein = table.Column<float>(type: "real", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeVersions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecipeVersions_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeVersionIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    RecipeVersionId = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeVersionIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeVersionIngredients_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeVersionIngredients_RecipeVersions_RecipeVersionId",
                        column: x => x.RecipeVersionId,
                        principalTable: "RecipeVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_LatestVersionId",
                table: "Recipes",
                column: "LatestVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeVersionIngredients_ProductId",
                table: "RecipeVersionIngredients",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeVersionIngredients_RecipeVersionId",
                table: "RecipeVersionIngredients",
                column: "RecipeVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeVersions_CreatedByUserId",
                table: "RecipeVersions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeVersions_RecipeId",
                table: "RecipeVersions",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DayPlanEntries_RecipeVersions_RecipeVersionId",
                table: "DayPlanEntries",
                column: "RecipeVersionId",
                principalTable: "RecipeVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_RecipeVersions_LatestVersionId",
                table: "Recipes",
                column: "LatestVersionId",
                principalTable: "RecipeVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedRecipes_RecipeVersions_RecipeVersionId",
                table: "SavedRecipes",
                column: "RecipeVersionId",
                principalTable: "RecipeVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
