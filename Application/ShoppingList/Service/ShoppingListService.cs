using Application.Products.Interfaces;
using Application.ShoppingList.DTO;
using Application.ShoppingList.Interfaces;
using Domain.Entities;

namespace Application.ShoppingList.Service;

public class ShoppingListService(
    IEnumerable<IShoppingSourceResolver> resolvers,
    IShoppingListRepository repository,
    IProductRepository productRepository) : IShoppingListService
{
    public async Task<ShoppingListDto> GenerateAsync(GenerateShoppingListRequest request, string userId)
    {
        var all = new List<ShoppingIngredient>();
        foreach (var resolver in resolvers)
            all.AddRange(await resolver.ResolveAsync(request));

        var aggregated = all
            .GroupBy(i => i.ProductId)
            .Select(g => new ShoppingListItem
            {
                ProductId = g.Key,
                TotalWeight = g.Sum(i => i.Weight),
            })
            .ToList();

        var shoppingList = new global::Domain.Entities.ShoppingList
        {
            Name = request.Name,
            UserId = userId,
            Items = aggregated,
        };

        var created = await repository.CreateAsync(shoppingList);
        return await ToDtoAsync(created);
    }

    public async Task<IReadOnlyList<ShoppingListDto>> GetByUserAsync(string userId)
    {
        var lists = await repository.GetByUserAsync(userId);
        var result = new List<ShoppingListDto>();
        foreach (var list in lists)
            result.Add(await ToDtoAsync(list));
        return result;
    }

    public async Task<ShoppingListDto> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task TogglePurchasedAsync(int shoppingListId, int itemId)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    // ── Private: побудова DTO ───────────────────────────────────────────────

    private async Task<ShoppingListDto> ToDtoAsync(Domain.Entities.ShoppingList list)
    {
        var groups = await BuildGroupsAsync(list.Items);

        return new ShoppingListDto
        {
            Id = list.Id,
            Name = list.Name,
            CreatedAt = list.CreatedAt,
            Groups = groups,
            TotalPrice = groups.Sum(g => g.TotalPrice),
        };
    }

    // ── Private: побудова дерева груп за Path ────────────────────────────────

    private async Task<ICollection<ShoppingListGroupDto>> BuildGroupsAsync(
        ICollection<ShoppingListItem> listItems)
    {
        var itemChains = listItems.ToDictionary(
            i => i,
            i => i.Product.Path.Split('.').Select(int.Parse).ToList());

        // Ids, які є предками (не останнім елементом) хоча б для одного товару в списку —
        // саме вони обов'язково існують як вузли-групи в дереві
        var ancestorIds = itemChains.Values
            .SelectMany(chain => chain.Take(chain.Count - 1))
            .ToHashSet();

        // Для кожного товару визначаємо реальний шлях по дереву груп:
        // якщо власний id товару сам є предком для іншого товару в списку —
        // товар іде всередину "своєї" групи; інакше — у групу найближчого предка.
        var walkChains = itemChains.ToDictionary(
            kv => kv.Key,
            kv =>
            {
                var chain = kv.Value;
                var ancestorChain = chain.Take(chain.Count - 1).ToList();
                var ownIsGroup = chain.Count > 0 && ancestorIds.Contains(chain[^1]);

                // товар без предків АБО товар, що сам є групою — додаємо власний id у шлях
                if (ancestorChain.Count == 0 || ownIsGroup)
                    ancestorChain.Add(chain[^1]);

                return ancestorChain;
            });

        var neededIds = walkChains.Values.SelectMany(c => c).Distinct().ToList();
        var products = neededIds.Count > 0
            ? (await productRepository.GetByBatchIdAsync(neededIds)).ToDictionary(p => p.Id)
            : new Dictionary<int, Product>();

        var rootNodes = new Dictionary<int, GroupNode>();

        foreach (var (item, walkChain) in walkChains)
        {
            var currentLevel = rootNodes;
            GroupNode node = null!;

            foreach (var nodeId in walkChain)
            {
                if (!currentLevel.TryGetValue(nodeId, out node!))
                {
                    node = new GroupNode(products[nodeId]);
                    currentLevel[nodeId] = node;
                }

                currentLevel = node.Children;
            }

            node.Items.Add(item);
        }

        return rootNodes.Values.Select(ToGroupDto).ToList();
    }

    // Проміжна мутабельна структура для побудови дерева
    private class GroupNode(Product product)
    {
        public Product Product { get; } = product;
        public Dictionary<int, GroupNode> Children { get; } = [];
        public List<ShoppingListItem> Items { get; } = [];
    }

    private static ShoppingListGroupDto ToGroupDto(GroupNode node)
    {
        var subGroups = node.Children.Values.Select(ToGroupDto).ToList();
        var items = node.Items.Select(ToItemDto).ToList();

        return new ShoppingListGroupDto
        {
            ProductId = node.Product.Id,
            GroupName = node.Product.Name,
            ImageUrl = node.Product.ImageUrl,
            SubGroups = subGroups,
            Items = items,
            TotalWeight = items.Sum(i => i.TotalWeight) + subGroups.Sum(g => g.TotalWeight),
            TotalPrice = items.Sum(i => i.TotalPrice) + subGroups.Sum(g => g.TotalPrice),
        };
    }

    private static ShoppingListItemDto ToItemDto(ShoppingListItem i) => new()
    {
        Id = i.Id,
        ProductName = i.Product.Name,
        ImageUrl = i.Product.ImageUrl,
        TotalWeight = i.TotalWeight,
        PricePerKg = i.Product.Price,
        TotalPrice = i.Product.Price * (decimal)(i.TotalWeight / 1000f),
        IsPurchased = i.IsPurchased,
    };
}