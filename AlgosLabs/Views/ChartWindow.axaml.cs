using System;
using System.Linq;
using AlgosLabs.ViewModels;
using Avalonia.Controls;
using ScottPlot;
using ScottPlot.Avalonia;

namespace AlgosLabs.Views;

public partial class ChartWindow : Window
{
    public ChartWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ChartWindowViewModel vm) PlotData(vm.ChartData);
    }

    private void PlotData(ChartDataModel data)
    {
        var plotControl = this.FindControl<AvaPlot>("MyPlot");
        if (plotControl == null) return;

        plotControl.Plot.Clear();
        plotControl.Plot.Legend.ManualItems.Clear();
        Console.WriteLine($"Количество серий в ChartData: {data.SeriesList.Count}");
        foreach (var series in data.SeriesList)
        {
            Console.WriteLine($"Серия: {series.AlgorithmName}, точек: {series.Points.Count}");
            // 1. Отрисовка эмпирических данных (точки + сплошная линия)
            var xs = series.Points.Select(p => (double)p.N).ToArray();
            var ys = series.Points.Select(p => p.AverageTimeMs).ToArray();

            var empiricalScatter = plotControl.Plot.Add.Scatter(xs, ys);
            empiricalScatter.LegendText = $"{series.AlgorithmName} (Эмпирические)";
            empiricalScatter.LineWidth = 2;
            empiricalScatter.MarkerSize = 6;

            // 2. Отрисовка аппроксимирующей кривой T_approx(n) = C * f(n)
            if (series.Approximation != null)
            {
                var approx = series.Approximation;
                var approxX = approx.TheoreticalPoints.Select(p => p.X).ToArray();
                var approxY = approx.TheoreticalPoints.Select(p => p.Y).ToArray();

                var approxScatter = plotControl.Plot.Add.Scatter(approxX, approxY);

                // Форматируем легенду с C и MSE
                approxScatter.LegendText = $"{series.AlgorithmName} (Теория: C={approx.C:E2}, MSE={approx.Mse:E4})";
                approxScatter.LineWidth = 3.5f;
                approxScatter.LineStyle.Pattern = LinePattern.Dashed; // Пунктирная линия
                approxScatter.Color = empiricalScatter.Color; // Тот же цвет, что и у оригинала
                approxScatter.MarkerSize = 0; // Без маркеров
            }
        }

        plotControl.Plot.Title("Эмпирические данные vs Аппроксимация T_approx(n) = C·f(n)");
        plotControl.Plot.XLabel("Размерность данных (N)");
        plotControl.Plot.YLabel("Время выполнения (мс)");
        plotControl.Plot.ShowLegend();

        plotControl.Refresh();
    }
}