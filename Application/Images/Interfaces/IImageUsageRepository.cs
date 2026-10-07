namespace Application.Images.Interfaces;

public interface IImageUsageRepository
{
    // Чи посилається хоч один запис (рецепт, крок, продукт, аватар) на зображення з цим хешем
    Task<bool> IsInUseAsync(string hash);
}
