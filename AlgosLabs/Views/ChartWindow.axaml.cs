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

    // 1. Выбираем палитру цветов ScottPlot (например, Category10)
    var palette = new ScottPlot.Palettes.Category10();

    for (int i = 0; i < data.SeriesList.Count; i++)
    {
        var series = data.SeriesList[i];

        // 2. Получаем динамический цвет для текущей серии по индексу
        ScottPlot.Color seriesColor = palette.GetColor(i);

        // --- Эмпирические данные ---
        var xs = series.Points.Select(p => (double)p.N).ToArray();
        var ys = series.Points.Select(p => p.AverageTimeMs).ToArray();

        var empiricalScatter = plotControl.Plot.Add.Scatter(xs, ys);
        empiricalScatter.LegendText = $"{series.AlgorithmName} (Эмпирические)";
        empiricalScatter.LineWidth = 2;
        empiricalScatter.MarkerSize = 6;
        empiricalScatter.Color = seriesColor; // Применяем динамический цвет

        // --- Теоретическая кривая ---
        if (series.Approximation != null)
        {
            var approx = series.Approximation;
            var approxX = approx.TheoreticalPoints.Select(p => p.X).ToArray();
            var approxY = approx.TheoreticalPoints.Select(p => p.Y).ToArray();

            var approxScatter = plotControl.Plot.Add.Scatter(approxX, approxY);
            approxScatter.LegendText = $"{series.AlgorithmName} (Теория: C={approx.C:E2}, MSE={approx.Mse:E4})";
            approxScatter.LineWidth = 2.5f;
            approxScatter.LineStyle.Pattern = LinePattern.Dashed;
            approxScatter.Color = seriesColor; // Применяем ТТОТ ЖЕ динамический цвет
            approxScatter.MarkerSize = 0;
        }
    }

    plotControl.Plot.Title("Эмпирические данные vs Аппроксимация T_approx(n) = C·f(n)");
    plotControl.Plot.XLabel("Размерность данных (N)");
    plotControl.Plot.YLabel("Время выполнения (мс)");
    plotControl.Plot.ShowLegend();

    plotControl.Refresh();
}
}