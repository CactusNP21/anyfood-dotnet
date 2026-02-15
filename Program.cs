using anyfood_dotnet;
using Mapster;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddEndpointsApiExplorer(); // Потрібен для аналізу ендпоінтів
builder.Services.AddSwaggerGen(); // Додає генератор Swagger-документації

builder.Services.AddDbContext<DataBaseContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllers();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddMapster();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
        policy  =>
        {
            policy.WithOrigins("http://localhost:4200") // Angular's URL
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
// app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

// ... тут ваш інший код (app.UseSwagger, app.UseHttpsRedirection...)
app.MapControllers();


app.Run();