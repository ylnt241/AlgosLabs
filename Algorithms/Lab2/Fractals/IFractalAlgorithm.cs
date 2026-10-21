using Algorithms.Interfaces;

namespace Algorithms.Lab2.Fractals;

/// <summary>
///     Фракталы не используют Generate/Execute: у них своя точка входа — Render.
/// </summary>
public interface IFractalAlgorithm : IAlgorithm
{
    /// <summary>Короткое описание фрактала для интерфейса.</summary>
    string Description { get; }

    /// <summary>Отрисовывает фрактал заданной глубины рекурсии на холсте.</summary>
    void Render(FractalCanvas canvas, int depth);
}
