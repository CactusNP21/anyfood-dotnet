using System.Text.RegularExpressions;
using Application.Images.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Images.Services;

// Один файл може використовуватися кількома рецептами/кроками/продуктами: каталог /images/{hash}/
// спільний для всіх, хто завантажив те саме зображення, тому видаляємо лише хеші без посилань
public partial class ImageCleanupService(
    IImageUsageRepository usageRepository,
    IImageStorageService imageStorageService,
    ILogger<ImageCleanupService> logger) : IImageCleanupService
{
    public async Task DeleteUnusedAsync(IEnumerable<string?> imageUrls)
    {
        var hashes = imageUrls
            .Select(TryGetHash)
            .OfType<string>()
            .Distinct();

        foreach (var hash in hashes)
        {
            try
            {
                if (!await usageRepository.IsInUseAsync(hash))
                    await imageStorageService.DeleteAsync(hash);
            }
            catch (Exception ex)
            {
                // Зміни в БД вже збережено — залишений файл не повинен ламати запит
                logger.LogWarning(ex, "Не вдалося видалити зображення {Hash}", hash);
            }
        }
    }

    private static string? TryGetHash(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        var match = ImageUrlRegex().Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex("/images/([0-9a-f]{16})/")]
    private static partial Regex ImageUrlRegex();
}
