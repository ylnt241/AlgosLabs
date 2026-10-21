namespace Algorithms.Lab2.Fractals;

/// <summary>
///     Ковёр Серпинского: рекурсивно делит квадрат на 9 частей и вырезает центральную.
/// </summary>
public sealed class SierpinskiCarpet : FractalAlgorithmBase
{
    public override string Id => "fractal-sierpinski-carpet";
    public override string Name => "Ковёр Серпинского";
    public override int MaxN => 8;

    public override string Description =>
        "Квадрат рекурсивно делится на 9 равных частей, центральная часть вырезается. " +
        "На каждом уровне остаётся 8 квадратов.";

    protected override void Draw(FractalCanvas canvas, int depth)
    {
        var size = Math.Min(canvas.Width, canvas.Height);
        var startX = (canvas.Width - size) / 2.0;
        var startY = (canvas.Height - size) / 2.0;

        DrawSquare(canvas, startX, startY, size, depth);
    }

    private static void DrawSquare(FractalCanvas canvas, double x, double y, double size, int depth)
    {
        if (depth <= 0)
        {
            // Цвет зависит от положения квадрата — фрактал получает плавный градиент
            var t = canvas.Width <= 0 ? 0 : (x + y) / (2.0 * canvas.Width);
            var (r, g, b) = FractalColors.ByFraction(t);
            canvas.FillRect(x, y, size, size, r, g, b);
            return;
        }

        var cell = size / 3.0;

        for (var row = 0; row < 3; row++)
        for (var col = 0; col < 3; col++)
        {
            if (row == 1 && col == 1) continue; // центральный квадрат вырезается

            DrawSquare(canvas, x + col * cell, y + row * cell, cell, depth - 1);
        }
    }
}
