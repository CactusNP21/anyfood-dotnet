using System.Security.Claims;
using System.Text;
using API.Middleware;
using Application.Auth.Interfaces;
using Application.Auth.Services;
using Application.Categories.Interfaces;
using Application.Categories.Services;
using Application.DayPlans.Interfaces;
using Application.DayPlans.Services;
using Application.Mapping;
using Application.Products.EventHandlers;
using Application.Products.Interfaces;
using Application.Products.Services;
using Application.RecipeCategories.Interfaces;
using Application.Recipes.Interfaces;
using Application.Recipes.Services;
using Application.ShoppingList.Interfaces;
using Application.ShoppingList.Service;
using Application.Users.Interfaces;
using Application.Users.Services;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Categories;
using Infrastructure.DayPlans;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Products;
using Infrastructure.RecipeCategory;
using Infrastructure.Recipes;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Додати одразу після var builder = WebApplication.CreateBuilder(args);
MappingConfig.Configure();

// ── Controllers ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Database ───────────────────────────────────────────────────────────────────

builder.Services.AddScoped<ProductPriceHistoryInterceptor>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .AddInterceptors(sp.GetRequiredService<ProductPriceHistoryInterceptor>())
    );

// ── Identity ───────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ── JWT ────────────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// ── CORS ───────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
        );
});

// ── Application Services ───────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// ── Categories ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ── Products ───────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductPriceHistoryRepository, ProductPriceHistoryRepository>();

// ── RecipeCategories ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IRecipeCategoryRepository, RecipeCategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ── Recipe ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IRecipeRepository, RecipesRepository>();
builder.Services.AddScoped<IRecipeService, RecipeService>();

//--DayPlan-------------------------------
builder.Services.AddScoped<IDayPlanRepository, DayPlanRepository>();
builder.Services.AddScoped<IDayPlanService, DayPlanService>();

// ── ShoppingList ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IShoppingSourceResolver, RecipeSourceResolver>();
builder.Services.AddScoped<IShoppingSourceResolver, ProductSourceResolver>();
builder.Services.AddScoped<IShoppingSourceResolver, DayPlanSourceResolver>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();
builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();

// ── Build ──────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Seed ───────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager);
    
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await RecipeLatestVersionSeeder.SeedAsync(db);  // <-- add this

}

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }
app.UseCors("Angular");

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Use(async (ctx, next) =>
{
    Console.WriteLine($">>> {ctx.Request.Method} {ctx.Request.Path}");
    await next();
    Console.WriteLine($"<<< {ctx.Response.StatusCode}");
});

app.UseCors("Angular");
// ...

app.Run();