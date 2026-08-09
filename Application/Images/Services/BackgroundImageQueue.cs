using System.Threading.Channels;

namespace Application.Images.Services;

public record ImageVariantJob(
    byte[] Bytes,
    string Hash,
    int[] Widths,
    Func<string, IServiceProvider, CancellationToken, Task> OnProcessed);

public class BackgroundImageQueue
{
    private readonly Channel<ImageVariantJob> _channel = Channel.CreateUnbounded<ImageVariantJob>();
    public void Enqueue(ImageVariantJob job) => _channel.Writer.TryWrite(job);
    public IAsyncEnumerable<ImageVariantJob> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}