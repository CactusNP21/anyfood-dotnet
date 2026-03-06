using anyfood_dotnet.Base;
using anyfood_dotnet.Dtos;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace anyfood_dotnet.Product;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext context) : ControllerRestBase<Models.Product, ProductResponse, ProductCreateRequest>(context)
{
    
}