using Application.Images.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/images")]
[Authorize]
public class ImagesController(IImageStorageService imageStorageService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ImageUploadResult>> Upload(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await imageStorageService.SaveAsync(stream));
    }
}