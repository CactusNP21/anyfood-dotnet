using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeleteProductVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayPlanEntries_ProductVersions_ProductVersionId",
                table: "DayPlanEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeVersionIngredients_ProductVersions_ProductVersionId",
                table: "RecipeVersionIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedProducts_ProductVersions_ProductVersionId",
                table: "SavedProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingListItems_ProductVersions_ProductVersionId",
                table: "ShoppingListItems");

            migrationBuilder.DropTable(
                name: "ProductVersions");

            migrationBuilder.RenameColumn(
                name: "ProductVersionId",
                table: "ShoppingListItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingListItems_ProductVersionId",
                table: "ShoppingListItems",
                newName: "IX_ShoppingListItems_ProductId");

            migrationBuilder.RenameColumn(
                name: "ProductVersionId",
                table: "SavedProducts",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_SavedProducts_ProductVersionId",
                table: "SavedProducts",
                newName: "IX_SavedProducts_ProductId");

            migrationBuilder.RenameColumn(
                name: "ProductVersionId",
                table: "RecipeVersionIngredients",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeVersionIngredients_ProductVersionId",
                table: "RecipeVersionIngredients",
                newName: "IX_RecipeVersionIngredients_ProductId");

            migrationBuilder.RenameColumn(
                name: "ProductVersionId",
                table: "DayPlanEntries",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_DayPlanEntries_ProductVersionId",
                table: "DayPlanEntries",
                newName: "IX_DayPlanEntries_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_DayPlanEntries_Products_ProductId",
                table: "DayPlanEntries",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeVersionIngredients_Products_ProductId",
                table: "RecipeVersionIngredients",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedProducts_Products_ProductId",
                table: "SavedProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingListItems_Products_ProductId",
                table: "ShoppingListItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayPlanEntries_Products_ProductId",
                table: "DayPlanEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeVersionIngredients_Products_ProductId",
                table: "RecipeVersionIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedProducts_Products_ProductId",
                table: "SavedProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingListItems_Products_ProductId",
                table: "ShoppingListItems");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ShoppingListItems",
                newName: "ProductVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_ShoppingListItems_ProductId",
                table: "ShoppingListItems",
                newName: "IX_ShoppingListItems_ProductVersionId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "SavedProducts",
                newName: "ProductVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_SavedProducts_ProductId",
                table: "SavedProducts",
                newName: "IX_SavedProducts_ProductVersionId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "RecipeVersionIngredients",
                newName: "ProductVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeVersionIngredients_ProductId",
                table: "RecipeVersionIngredients",
                newName: "IX_RecipeVersionIngredients_ProductVersionId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "DayPlanEntries",
                newName: "ProductVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_DayPlanEntries_ProductId",
                table: "DayPlanEntries",
                newName: "IX_DayPlanEntries_ProductVersionId");

            migrationBuilder.CreateTable(
                name: "ProductVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Calories = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    Carbs = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fat = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    GlycemicIndex = table.Column<int>(type: "integer", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Protein = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVersions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVersions_ProductId",
                table: "ProductVersions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVersions_ProductId_VersionNumber",
                table: "ProductVersions",
                columns: new[] { "ProductId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DayPlanEntries_ProductVersions_ProductVersionId",
                table: "DayPlanEntries",
                column: "ProductVersionId",
                principalTable: "ProductVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeVersionIngredients_ProductVersions_ProductVersionId",
                table: "RecipeVersionIngredients",
                column: "ProductVersionId",
                principalTable: "ProductVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedProducts_ProductVersions_ProductVersionId",
                table: "SavedProducts",
                column: "ProductVersionId",
                principalTable: "ProductVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingListItems_ProductVersions_ProductVersionId",
                table: "ShoppingListItems",
                column: "ProductVersionId",
                principalTable: "ProductVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
