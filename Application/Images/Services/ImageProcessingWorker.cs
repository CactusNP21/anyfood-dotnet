using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Images.Services;

public class ImageProcessingWorker(
    BackgroundImageQueue queue,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ImageProcessingWorker> logger) : BackgroundService
{
    private readonly string _root = configuration["Images:StoragePath"] ?? "/app/data/images";

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var encoder = new WebpEncoder
        {
            Quality = 80,
            FileFormat = WebpFileFormatType.Lossy,
            Method = WebpEncodingMethod.BestQuality,
        };

        await foreach (var job in queue.ReadAllAsync(ct))
        {
            try
            {
                var dir = Path.Combine(_root, job.Hash);

                if (!Directory.Exists(dir)) // такий самий хеш вже оброблявся раніше
                {
                    Directory.CreateDirectory(dir);

                    using var image = Image.Load(job.Bytes); // декодування напряму з памʼяті
                    await Task.WhenAll(job.Widths.Select(w => SaveVariantAsync(image, w, dir, encoder, ct)));
                }

                var finalUrl = $"/images/{job.Hash}/{job.Widths.Max()}.webp";

                using var scope = scopeFactory.CreateScope();
                await job.OnProcessed(finalUrl, scope.ServiceProvider, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Не вдалося обробити зображення {Hash}", job.Hash);
            }
        }
    }

    private static async Task SaveVariantAsync(
        Image source, int width, string dir, WebpEncoder encoder, CancellationToken ct)
    {
        using var clone = source.Clone(ctx =>
        {
            if (source.Width > width)
                ctx.Resize(new ResizeOptions { Size = new Size(width, 0), Mode = ResizeMode.Max });
        });
        await clone.SaveAsync(Path.Combine(dir, $"{width}.webp"), encoder, ct);
    }
}