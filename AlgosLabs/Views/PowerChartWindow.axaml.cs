using System.Collections.Generic;
using Avalonia.Controls;
using ScottPlot;

namespace AlgosLabs.Views;

public partial class PowerChartWindow : Window
{
    public PowerChartWindow()
    {
        InitializeComponent();
    }

    public void PlotResults(Dictionary<string, List<Coordinates>> results)
    {
        PowerPlot.Plot.Clear();

        foreach (var pair in results)
        {
            var name = pair.Key;
            var points = pair.Value;

            var scatter = PowerPlot.Plot.Add.Scatter(points);
            scatter.LegendText = name;


            if (name.ToLowerInvariant().Contains("итеративный"))
            {
                scatter.LineWidth = 4;
                scatter.LinePattern = LinePattern.Dashed;
                scatter.Color = Colors.Blue;
            }
            else if (name.ToLowerInvariant().Contains("рекурсивный"))
            {
                scatter.LineWidth = 4;
                scatter.LinePattern = LinePattern.Solid;
                scatter.Color = Colors.Red;
            }
            else
            {
                // Быстрый бинарный или другие алгоритмы
                scatter.LineWidth = 2;
                scatter.LinePattern = LinePattern.Solid;
                scatter.Color = Colors.Green;
            }
        }

        PowerPlot.Plot.Title("Зависимость количества операций от N");
        PowerPlot.Plot.XLabel("Показатель степени (N)");
        PowerPlot.Plot.YLabel("Количество операций (Шагов)");
        PowerPlot.Plot.ShowLegend();

        PowerPlot.Plot.Axes.AutoScale();
        PowerPlot.Refresh();
    }
}