CREATE OR REPLACE FUNCTION recalculate_recipe_prices()
    RETURNS TRIGGER AS $$
BEGIN
    UPDATE "Recipes" r
    SET "Price" = (
        SELECT SUM(rp."Weight" / 100.0 * p."Price")
        FROM "RecipeProducts" rp
                 JOIN "Products" p ON p."Id" = rp."ProductId"
        WHERE rp."RecipeId" = r."Id"
    )
    WHERE EXISTS (
        SELECT 1 FROM "RecipeProducts" rp
        WHERE rp."RecipeId" = r."Id" AND rp."ProductId" = NEW."Id"
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER on_product_price_changed
    AFTER UPDATE OF "Price" ON "Products"
    FOR EACH ROW
    WHEN (OLD."Price" IS DISTINCT FROM NEW."Price")
EXECUTE FUNCTION recalculate_recipe_prices();