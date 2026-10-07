using Application.Images.Interfaces;
using Application.Images.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Images;

public class ImageCleanupServiceTests
{
    private const string Used = "aaaaaaaaaaaaaaaa";
    private const string Unused = "bbbbbbbbbbbbbbbb";

    private readonly FakeUsage usage = new();
    private readonly FakeStorage storage = new();
    private readonly ImageCleanupService service;

    public ImageCleanupServiceTests()
    {
        usage.InUse.Add(Used);
        service = new ImageCleanupService(usage, storage, NullLogger<ImageCleanupService>.Instance);
    }

    [Fact]
    public async Task DeletesOnlyHashesWithoutReferences()
    {
        await service.DeleteUnusedAsync([$"/images/{Used}/1200.webp", $"/images/{Unused}/500.webp"]);

        Assert.Equal([Unused], storage.Deleted);
    }

    [Fact]
    public async Task DifferentWidthsOfSameImage_CheckedAndDeletedOnce()
    {
        await service.DeleteUnusedAsync([$"/images/{Unused}/1200.webp", $"/images/{Unused}/500.webp"]);

        Assert.Equal([Unused], storage.Deleted);
        Assert.Equal([Unused], usage.Checked);
    }

    [Fact]
    public async Task IgnoresNullAndForeignUrls()
    {
        await service.DeleteUnusedAsync([null, "", "https://lh3.googleusercontent.com/a/photo.jpg", "/images/../etc/500.webp"]);

        Assert.Empty(usage.Checked);
        Assert.Empty(storage.Deleted);
    }

    [Fact]
    public async Task AbsoluteUrl_IsRecognised()
    {
        await service.DeleteUnusedAsync([$"http://localhost:8080/images/{Unused}/500.webp"]);

        Assert.Equal([Unused], storage.Deleted);
    }

    [Fact]
    public async Task StorageFailure_IsSwallowedAndOtherImagesStillDeleted()
    {
        const string failing = "cccccccccccccccc";
        storage.Failing.Add(failing);

        await service.DeleteUnusedAsync([$"/images/{failing}/500.webp", $"/images/{Unused}/500.webp"]);

        Assert.Equal([Unused], storage.Deleted);
    }

    private class FakeUsage : IImageUsageRepository
    {
        public HashSet<string> InUse { get; } = [];
        public List<string> Checked { get; } = [];

        public Task<bool> IsInUseAsync(string hash)
        {
            Checked.Add(hash);
            return Task.FromResult(InUse.Contains(hash));
        }
    }

    private class FakeStorage : IImageStorageService
    {
        public HashSet<string> Failing { get; } = [];
        public List<string> Deleted { get; } = [];

        public Task DeleteAsync(string hash, CancellationToken ct = default)
        {
            if (Failing.Contains(hash)) throw new IOException("locked");
            Deleted.Add(hash);
            return Task.CompletedTask;
        }

        public Task<ImageUploadResult> SaveAsync(Stream input, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<ImageUploadResult> SaveAsync(Stream input, int[] widths, CancellationToken ct = default) => throw new NotImplementedException();
        public string Enqueue(byte[] bytes, Func<string, IServiceProvider, CancellationToken, Task> onProcessed) => throw new NotImplementedException();
    }
}
