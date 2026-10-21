namespace Algorithms.Lab2.Fractals;

/// <summary>
///     Общая обвязка фракталов: очистка холста и ограничение глубины.
///     Сам узор рисует <see cref="Draw" />.
/// </summary>
public abstract class FractalAlgorithmBase : IFractalAlgorithm
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract int MaxN { get; }
    public abstract string Description { get; }

    public void Render(FractalCanvas canvas, int depth)
    {
        canvas.Clear();
        Draw(canvas, Math.Clamp(depth, 1, MaxN));
    }

    /// <summary>Рекурсивная отрисовка фрактала на чистом холсте.</summary>
    protected abstract void Draw(FractalCanvas canvas, int depth);
}
