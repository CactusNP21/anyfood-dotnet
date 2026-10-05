using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodDiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalorieGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    Calories = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalorieGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalorieGoals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodDiaryDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CalorieGoalOverride = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodDiaryDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodDiaryDays_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodDiaryEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FoodDiaryDayId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    RecipeId = table.Column<int>(type: "integer", nullable: true),
                    Weight = table.Column<float>(type: "real", nullable: false),
                    Time = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodDiaryEntries", x => x.Id);
                    table.CheckConstraint("CK_FoodDiaryEntry_RecipeOrProduct", "(\"RecipeId\" IS NOT NULL) != (\"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_FoodDiaryEntries_FoodDiaryDays_FoodDiaryDayId",
                        column: x => x.FoodDiaryDayId,
                        principalTable: "FoodDiaryDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodDiaryEntries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FoodDiaryEntries_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalorieGoals_UserId_EffectiveFrom",
                table: "CalorieGoals",
                columns: new[] { "UserId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodDiaryDays_UserId_Date",
                table: "FoodDiaryDays",
                columns: new[] { "UserId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodDiaryEntries_FoodDiaryDayId",
                table: "FoodDiaryEntries",
                column: "FoodDiaryDayId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodDiaryEntries_ProductId",
                table: "FoodDiaryEntries",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodDiaryEntries_RecipeId",
                table: "FoodDiaryEntries",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalorieGoals");

            migrationBuilder.DropTable(
                name: "FoodDiaryEntries");

            migrationBuilder.DropTable(
                name: "FoodDiaryDays");
        }
    }
}
