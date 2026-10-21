using System;
using System.Linq;
using AlgosLabs.ViewModels;
using Avalonia.Controls;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Palettes;

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

        if (DataContext is ChartWindowViewModel vm) PlotData(vm);
    }

    private void PlotData(ChartWindowViewModel vm)
    {
        var data = vm.ChartData;
        var plotControl = this.FindControl<AvaPlot>("MyPlot");
        if (plotControl == null) return;

        plotControl.Plot.Clear();
        plotControl.Plot.Legend.ManualItems.Clear();

        // Цвет серии выбираем по индексу: эмпирика и теория должны совпадать.
        var palette = new Category10();

        for (var i = 0; i < data.SeriesList.Count; i++)
        {
            var series = data.SeriesList[i];
            var seriesColor = palette.GetColor(i);

            // Измеренные точки.
            var xs = series.Points.Select(p => (double)p.N).ToArray();
            var ys = series.Points.Select(p => p.AverageTimeMs).ToArray();

            var empiricalScatter = plotControl.Plot.Add.Scatter(xs, ys);
            empiricalScatter.LegendText = series.Approximation == null
                ? series.AlgorithmName
                : $"{series.AlgorithmName} (Эмпирические)";
            empiricalScatter.LineWidth = 2;
            empiricalScatter.MarkerSize = 6;
            empiricalScatter.Color = seriesColor;

            // Теоретическая кривая — тем же цветом, но пунктиром.
            if (series.Approximation != null)
            {
                var approx = series.Approximation;
                var approxX = approx.TheoreticalPoints.Select(p => p.X).ToArray();
                var approxY = approx.TheoreticalPoints.Select(p => p.Y).ToArray();

                var approxScatter = plotControl.Plot.Add.Scatter(approxX, approxY);
                approxScatter.LegendText = $"{series.AlgorithmName} (Теория: C={approx.C:E2}, MSE={approx.Mse:E4})";
                approxScatter.LineWidth = 2.5f;
                approxScatter.LineStyle.Pattern = LinePattern.Dashed;
                approxScatter.Color = seriesColor;
                approxScatter.MarkerSize = 0;
            }
        }

        plotControl.Plot.Title(vm.Title);
        plotControl.Plot.XLabel(vm.XLabel);
        plotControl.Plot.YLabel(vm.YLabel);
        plotControl.Plot.ShowLegend();

        plotControl.Refresh();
    }
}