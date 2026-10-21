namespace Algorithms.Lab2.Fractals;

/// <summary>
///     Простой BGRA-буфер: его можно без преобразований отдать в Avalonia WriteableBitmap.
/// </summary>
public sealed class FractalCanvas
{
    // Фон холста — тёмно-синий, чтобы фрактал хорошо читался
    private const byte BackgroundR = 16;
    private const byte BackgroundG = 22;
    private const byte BackgroundB = 36;

    public FractalCanvas(int width, int height)
    {
        Width = width;
        Height = height;
        BgraPixels = new byte[width * height * 4];
        Clear();
    }

    public int Width { get; }

    public int Height { get; }

    public byte[] BgraPixels { get; }

    /// <summary>Заливает холст фоновым цветом.</summary>
    public void Clear()
    {
        for (var i = 0; i < BgraPixels.Length; i += 4)
        {
            BgraPixels[i] = BackgroundB;
            BgraPixels[i + 1] = BackgroundG;
            BgraPixels[i + 2] = BackgroundR;
            BgraPixels[i + 3] = 255;
        }
    }

    /// <summary>Ставит пиксель (координаты вне холста игнорируются).</summary>
    public void SetPixel(int x, int y, byte r, byte g, byte b)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return;

        var index = (y * Width + x) * 4;
        BgraPixels[index] = b;
        BgraPixels[index + 1] = g;
        BgraPixels[index + 2] = r;
        BgraPixels[index + 3] = 255;
    }

    /// <summary>Заливает прямоугольник (дробные координаты округляются до пикселей).</summary>
    public void FillRect(double x, double y, double width, double height, byte r, byte g, byte b)
    {
        var x0 = Math.Clamp((int)Math.Floor(x), 0, Width);
        var y0 = Math.Clamp((int)Math.Floor(y), 0, Height);
        var x1 = Math.Clamp((int)Math.Ceiling(x + width), 0, Width);
        var y1 = Math.Clamp((int)Math.Ceiling(y + height), 0, Height);

        for (var py = y0; py < y1; py++)
            for (var px = x0; px < x1; px++)
                SetPixel(px, py, r, g, b);
    }

    /// <summary>Рисует отрезок по алгоритму Брезенхема.</summary>
    public void DrawLine(double x1, double y1, double x2, double y2, byte r, byte g, byte b)
    {
        var xStart = (int)Math.Round(x1);
        var yStart = (int)Math.Round(y1);
        var xEnd = (int)Math.Round(x2);
        var yEnd = (int)Math.Round(y2);

        var dx = Math.Abs(xEnd - xStart);
        var sx = xStart < xEnd ? 1 : -1;
        var dy = -Math.Abs(yEnd - yStart);
        var sy = yStart < yEnd ? 1 : -1;
        var error = dx + dy;

        while (true)
        {
            SetPixel(xStart, yStart, r, g, b);

            if (xStart == xEnd && yStart == yEnd) break;

            var e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                xStart += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                yStart += sy;
            }
        }
    }
}
