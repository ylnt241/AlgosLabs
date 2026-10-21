namespace Algorithms.Lab2.Fractals;

/// <summary>
///     Палитра для отрисовки фракталов: плавный переход от голубого к фиолетовому.
/// </summary>
public static class FractalColors
{
    private static readonly (byte R, byte G, byte B) Cool = (110, 220, 255);
    private static readonly (byte R, byte G, byte B) Deep = (150, 105, 255);

    /// <summary>Цвет по доле t в диапазоне [0; 1].</summary>
    public static (byte R, byte G, byte B) ByFraction(double t)
    {
        t = double.IsNaN(t) ? 0 : Math.Clamp(t, 0, 1);

        return (
            (byte)(Cool.R + (Deep.R - Cool.R) * t),
            (byte)(Cool.G + (Deep.G - Cool.G) * t),
            (byte)(Cool.B + (Deep.B - Cool.B) * t));
    }

    /// <summary>Цвет по уровню рекурсии (0 — самый верхний уровень).</summary>
    public static (byte R, byte G, byte B) ByLevel(int level, int maxDepth)
        => ByFraction(maxDepth <= 1 ? 0 : (double)level / (maxDepth - 1));
}
