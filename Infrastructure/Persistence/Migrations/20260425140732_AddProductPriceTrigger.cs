using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPriceTrigger : Migration
    {
        protected override void Up(MigrationBuilder mb)
        {
            var sql = ReadScript("Triggers/recalculate_recipe_prices.sql");
            mb.Sql(sql);
        }

        protected override void Down(MigrationBuilder mb)
        {
            mb.Sql("DROP TRIGGER IF EXISTS on_product_price_changed ON products;");
            mb.Sql("DROP FUNCTION IF EXISTS recalculate_recipe_prices;");
        }

        private static string ReadScript(string name)
        {
            var assembly = typeof(AddProductPriceTrigger).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .Single(n => n.EndsWith(name.Replace("/", ".")));

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
