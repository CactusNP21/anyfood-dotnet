using System.Security.Cryptography;
using Application.Images.Interfaces;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Images.Services;

public class ImageProcessingService(IConfiguration configuration, BackgroundImageQueue queue) : IImageStorageService
{
    private static readonly int[] Widths = [200, 500, 1200]; // thumbnail, card, full
    private readonly string _root = configuration["Images:StoragePath"] ?? "/app/data/images";

    public async Task<ImageUploadResult> SaveAsync(Stream input, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        await input.CopyToAsync(ms, ct);

        var hash = Convert.ToHexString(SHA256.HashData(ms.ToArray()))[..16].ToLowerInvariant();
        var dir = Path.Combine(_root, hash);

        // Той самий контент → та сама папка. Нема сенсу перегенеровувати.
        if (Directory.Exists(dir))
            return new ImageUploadResult(hash, BuildUrls(hash));

        Directory.CreateDirectory(dir);

        ms.Position = 0;
        using var image = await Image.LoadAsync(ms, ct);

        var encoder = new WebpEncoder
        {
            Quality = 80,
            FileFormat = WebpFileFormatType.Lossy,
            Method = WebpEncodingMethod.BestQuality,
        };

        foreach (var width in Widths)
        {
            using var clone = image.Clone(ctx =>
            {
                if (image.Width > width) // не збільшуємо менші зображення
                    ctx.Resize(new ResizeOptions { Size = new Size(width, 0), Mode = ResizeMode.Max });
            });

            await clone.SaveAsync(Path.Combine(dir, $"{width}.webp"), encoder, ct);
        }

        return new ImageUploadResult(hash, BuildUrls(hash));
    }

    public Task DeleteAsync(string hash, CancellationToken ct = default)
    {
        var dir = Path.Combine(_root, hash);
        if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        return Task.CompletedTask;
    }

    public string Enqueue(byte[] bytes, Func<string, IServiceProvider, CancellationToken, Task> onProcessed)
    {
        var hash = Convert.ToHexString(SHA256.HashData(bytes))[..16].ToLowerInvariant();

        queue.Enqueue(new ImageVariantJob(bytes, hash, Widths, onProcessed));

        return hash;
    }

    private static Dictionary<int, string> BuildUrls(string hash)
        => Widths.ToDictionary(w => w, w => $"/images/{hash}/{w}.webp");
}