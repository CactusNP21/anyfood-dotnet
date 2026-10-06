using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeSalt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Salt",
                table: "Recipes",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            // Заповнюємо сіль на 100 г для наявних рецептів так само, як RecipeService рахує інші макроси
            migrationBuilder.Sql("""
                UPDATE "Recipes" r
                SET "Salt" = s.salt
                FROM (
                    SELECT rp."RecipeId",
                           SUM(p."Salt" * rp."Weight") / NULLIF(SUM(rp."Weight"), 0) AS salt
                    FROM "RecipeProducts" rp
                    JOIN "Products" p ON p."Id" = rp."ProductId"
                    GROUP BY rp."RecipeId"
                ) s
                WHERE r."Id" = s."RecipeId" AND s.salt IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Recipes");
        }
    }
}
