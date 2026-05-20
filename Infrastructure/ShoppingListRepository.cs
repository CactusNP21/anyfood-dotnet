using Application.ShoppingList.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

// Infrastructure/ShoppingLists/ShoppingListRepository.cs
public class ShoppingListRepository(AppDbContext ctx) : IShoppingListRepository
{
    public async Task<ShoppingList> CreateAsync(ShoppingList shoppingList)
    {
        ctx.ShoppingLists.Add(shoppingList);
        await ctx.SaveChangesAsync();
        return shoppingList;
    }

    public async Task<ShoppingList?> GetByIdAsync(int id)
        => await ctx.ShoppingLists
            .Include(sl => sl.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(sl => sl.Id == id);

    public async Task<IReadOnlyList<ShoppingList>> GetByUserAsync(string userId)
        => await ctx.ShoppingLists
            .Include(sl => sl.Items)
            .ThenInclude(i => i.Product)
            .Where(sl => sl.UserId == userId)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();

    public async Task<ShoppingListItem?> GetItemByIdAsync(int itemId)
        => await ctx.ShoppingListItems
            .FirstOrDefaultAsync(i => i.Id == itemId);

    public async Task UpdateItemAsync(ShoppingListItem item)
    {
        ctx.ShoppingListItems.Update(item);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(ShoppingList shoppingList)
    {
        ctx.ShoppingLists.Remove(shoppingList);
        await ctx.SaveChangesAsync();
    }
}