using System;
using System.Globalization;
using AlgosLabs.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AlgosLabs.Controls;

/// <summary>
///     Ручная отрисовка трёх стержней с дисками.
///     При изменении <see cref="State" /> Avalonia сама вызывает <see cref="Render" />.
/// </summary>
public class HanoiTowersControl : Control
{
    public static readonly StyledProperty<HanoiState?> StateProperty =
        AvaloniaProperty.Register<HanoiTowersControl, HanoiState?>(nameof(State));

    static HanoiTowersControl()
    {
        AffectsRender<HanoiTowersControl>(StateProperty);
    }

    public HanoiState? State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var state = State;
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (state is null || state.DiskCount <= 0 || width < 60 || height < 60) return;

        const double horizontalPadding = 18;
        const double bottomPadding = 34;
        const double topPadding = 18;
        const double rodThickness = 5;
        const double baseHeight = 7;

        var columnWidth = (width - horizontalPadding * 2) / HanoiState.RodCount;
        var baseY = height - bottomPadding;
        var rodTop = topPadding;
        var usableHeight = Math.Max(20, baseY - rodTop - 6);

        var rodPen = new Pen(new SolidColorBrush(Color.FromRgb(0x4C, 0x5E, 0x7E)), rodThickness);
        var basePen = new Pen(new SolidColorBrush(Color.FromRgb(0x5E, 0x72, 0x94)), baseHeight);
        var labelBrush = new SolidColorBrush(Color.FromRgb(0xAA, 0xBA, 0xD4));

        var centers = new double[HanoiState.RodCount];
        for (var i = 0; i < HanoiState.RodCount; i++)
            centers[i] = horizontalPadding + columnWidth * (i + 0.5);

        // Основания и вертикальные стержни
        for (var i = 0; i < HanoiState.RodCount; i++)
        {
            var halfBase = columnWidth * 0.38;
            context.DrawLine(
                basePen,
                new Point(centers[i] - halfBase, baseY),
                new Point(centers[i] + halfBase, baseY));

            context.DrawLine(
                rodPen,
                new Point(centers[i], baseY - baseHeight / 2),
                new Point(centers[i], rodTop));
        }

        // Диски: от основания вверх
        var maxDiskWidth = columnWidth * 0.82;
        var minDiskWidth = Math.Max(12, maxDiskWidth * 0.34);
        var diskHeight = Math.Min(28, Math.Max(7, usableHeight / state.DiskCount * 0.82));
        const double diskGap = 1;

        for (var rod = 0; rod < HanoiState.RodCount; rod++)
        {
            var disks = state.Rods[rod];

            for (var level = 0; level < disks.Length; level++)
            {
                var disk = disks[level];
                var t = state.DiskCount <= 1 ? 1.0 : (disk - 1.0) / (state.DiskCount - 1);
                var diskWidth = minDiskWidth + (maxDiskWidth - minDiskWidth) * t;

                var y = baseY - baseHeight / 2 - (level + 1) * (diskHeight + diskGap);
                var rect = new Rect(centers[rod] - diskWidth / 2, y, diskWidth, diskHeight);

                context.DrawRectangle(
                    new SolidColorBrush(DiskColor(t)),
                    null,
                    rect,
                    diskHeight * 0.28,
                    diskHeight * 0.28);

                if (diskHeight < 13) continue;

                var number = new FormattedText(
                    disk.ToString(CultureInfo.InvariantCulture),
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    Math.Min(12, diskHeight * 0.62),
                    Brushes.White);

                context.DrawText(
                    number,
                    new Point(centers[rod] - number.Width / 2, y + (diskHeight - number.Height) / 2));
            }
        }

        // Подписи стержней A, B, C
        var labelTypeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold);

        for (var i = 0; i < HanoiState.RodCount; i++)
        {
            var label = new FormattedText(
                HanoiState.RodName(i),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                labelTypeface,
                13,
                labelBrush);

            context.DrawText(label, new Point(centers[i] - label.Width / 2, baseY + 8));
        }
    }

    /// <summary>Цвет диска: плавный переход от бирюзового к фиолетовому.</summary>
    private static Color DiskColor(double t) => FromHsv(190 + 120 * t, 0.62, 0.95);

    private static Color FromHsv(double hue, double saturation, double value)
    {
        hue = (hue % 360 + 360) % 360;

        var c = value * saturation;
        var x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
        var m = value - c;

        var (r, g, b) = hue switch
        {
            < 60 => (c, x, 0.0),
            < 120 => (x, c, 0.0),
            < 180 => (0.0, c, x),
            < 240 => (0.0, x, c),
            < 300 => (x, 0.0, c),
            _ => (c, 0.0, x)
        };

        return Color.FromRgb(
            (byte)Math.Round((r + m) * 255),
            (byte)Math.Round((g + m) * 255),
            (byte)Math.Round((b + m) * 255));
    }
}
