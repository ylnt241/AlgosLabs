using System;
using System.Linq;
using Algorithms.Lab1.MatrixOperations;
using Avalonia.Controls;
using ScottPlot;
using ScottPlot.Colormaps;

namespace AlgosLabs.Views;

public partial class HeatmapWindow : Window
{
    public HeatmapWindow()
    {
        InitializeComponent();
    }

    public void LoadDataAndPlot(HeatmapDataModel data)
    {
        if (data.Points == null || data.Points.Count == 0)
            return;

        // Уникальные N и M задают оси сетки.
        var ns = data.Points.Select(p => p.N).Distinct().OrderBy(x => x).ToArray();
        var ms = data.Points.Select(p => p.M).Distinct().OrderBy(x => x).ToArray();

        var rows = ns.Length;
        var cols = ms.Length;

        if (rows == 0 || cols == 0) return;

        // По N переворачиваем строки, чтобы меньшее значение было внизу.
        var grid = new double[rows, cols];
        foreach (var p in data.Points)
        {
            var r = Array.BinarySearch(ns, p.N);
            var c = Array.BinarySearch(ms, p.M);
            if (r >= 0 && c >= 0)
                grid[rows - 1 - r, c] = p.TimeMs;
        }

        AvaPlot1.Plot.Clear();

        var hm = AvaPlot1.Plot.Add.Heatmap(grid);
        hm.Colormap = new Turbo();

        // Границы осей задаём по реальным N и M, иначе ScottPlot использует индексы.
        double xMin = ms.First();
        double xMax = ms.Last();
        double yMin = ns.First();
        double yMax = ns.Last();

        hm.Extent = new CoordinateRect(xMin, xMax, yMin, yMax);

        AvaPlot1.Plot.Title("Зависимость времени работы от размера матриц (N x M)");
        AvaPlot1.Plot.XLabel("Ось M (Столбцы)");
        AvaPlot1.Plot.YLabel("Ось N (Строки)");

        AvaPlot1.Plot.Add.ColorBar(hm);

        AvaPlot1.Plot.Axes.AutoScale();
        AvaPlot1.Refresh();

        if (StatusText != null)
            StatusText.Text = $"Замеров: {data.Points.Count} | N: [{yMin}..{yMax}], M: [{xMin}..{xMax}]";
    }
}