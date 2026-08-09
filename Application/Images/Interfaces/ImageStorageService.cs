namespace Application.Images.Interfaces;

public interface IImageStorageService
{
    Task<ImageUploadResult> SaveAsync(Stream input, CancellationToken ct = default);
    Task DeleteAsync(string hash, CancellationToken ct = default);

    string Enqueue(
        byte[] bytes,
        Func<string, IServiceProvider, CancellationToken, Task> onProcessed);
}

public record ImageUploadResult(string Hash, IReadOnlyDictionary<int, string> VariantUrls);