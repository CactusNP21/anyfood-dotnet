namespace Application.Images.Interfaces;

public interface IImageCleanupService
{
    // Видаляє з диску зображення за цими URL, якщо на їхній хеш більше ніхто не посилається.
    // Викликати лише після збереження змін у БД.
    Task DeleteUnusedAsync(IEnumerable<string?> imageUrls);
}
