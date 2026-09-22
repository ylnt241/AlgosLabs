using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Algorithms.Interfaces;
using Algorithms.Lab1.PowerOperations;
using Algorithms.Services;
using AlgosLabs.Services;
using AlgosLabs.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot;

namespace AlgosLabs.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly UniversalBenchmarkService _benchmarkService = new();
    private readonly AlgorithmScanner _scanner = new();
    [ObservableProperty] private int _algorithmStep = 1;
    [ObservableProperty] private double _baseX = 2.0;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _maxM = 500;

    // --- Параметры по-умолчанию для обычных 2D бенчмарков ---
    [ObservableProperty] private int _maxN = 1000;

    [ObservableProperty] private int _powerMaxN = 1000;
    [ObservableProperty] private int _startM = 50;

    // --- Свойства для 3D бенчмарка Матриц (N x M) ---
    [ObservableProperty] private int _startN = 50;
    [ObservableProperty] private int _stepM = 50;
    [ObservableProperty] private int _stepN = 100;

    public MainViewModel()
    {
        LoadAlgorithmsAutomatically();
    }

    // Алгоритмы автоматически парсятся в зависимости от интерфейса который они реализуют.
    public ObservableCollection<AlgorithmSelectionViewModel> AvailableAlgorithms { get; } = new();

    public ObservableCollection<AlgorithmSelectionViewModel> PowerAlgorithmsSelection { get; } = new();

    public ObservableCollection<AlgorithmSelectionViewModel> IndividualAlgorithms { get; } = new();

    private void LoadAlgorithmsAutomatically()
    {
        AvailableAlgorithms.Clear();
        PowerAlgorithmsSelection.Clear();
        IndividualAlgorithms.Clear();

        var foundAlgorithms = _scanner.FindAllAlgorithms();

        // Заполняем алгоритмы степеней
        var powerAlgos = foundAlgorithms
            .OfType<IPowerAlgorithm>()
            .Select(a => new AlgorithmSelectionViewModel(a));

        foreach (var item in powerAlgos) PowerAlgorithmsSelection.Add(item);

        var generalAlgos = foundAlgorithms
            .Where(a => a is not (IPowerAlgorithm or IIndividualAlgorithm))
            .Select(a => new AlgorithmSelectionViewModel(a));

        foreach (var item in generalAlgos) AvailableAlgorithms.Add(item);

        var individualAlgos = foundAlgorithms
            .OfType<IIndividualAlgorithm>()
            .Select(a => new AlgorithmSelectionViewModel(a));

        foreach (var item in individualAlgos) IndividualAlgorithms.Add(item);
    }

    [RelayCommand]
    private async Task RunVectorComparisonAsync()
    {
        var selected = AvailableAlgorithms
            .Where(x => x.IsSelected)
            .Select<AlgorithmSelectionViewModel, IAlgorithm>(x => x.Algorithm)
            .ToList();

        if (!selected.Any())
        {
            IsBusy = false;
            return;
        }

        IsBusy = true;
        try
        {
            var allResults = await Task.Run(async () =>
            {
                var resultsModel = new ChartDataModel();
                resultsModel.SeriesList.Clear();
                var approxService = new ApproximationService();

                foreach (var algo in selected)
                {
                    // Если _benchmarkService.RunDynamicBenchmark асинхронный:
                    var points = await _benchmarkService.RunDynamicBenchmark(
                        algo,
                        MaxN,
                        StepN,
                        AlgorithmStep
                    );

                    var targetComplexity = ComplexityType.Linear;
                    var approxResult = approxService.Fit(points, targetComplexity);

                    resultsModel.SeriesList.Add(new SeriesData(algo.Name, points, approxResult));
                }

                return resultsModel;
            });

            var chartWindow = new ChartWindow();
            chartWindow.DataContext = new ChartWindowViewModel(allResults);
            chartWindow.Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RunMatrix3DBenchmarkAsync()
    {
        IsBusy = true;
        try
        {
            var heatmapData = await Task.Run(() =>
                MatrixBenchmarkService.RunBenchmarkGrid(
                    StartN, MaxN, StepN,
                    StartM, MaxM, StepM
                )
            );
            var heatmapWindow = new HeatmapWindow();
            heatmapWindow.LoadDataAndPlot(heatmapData);
            heatmapWindow.Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RunPowerBenchmarkAsync()
    {
        IsBusy = true;

        var selectedAlgos = PowerAlgorithmsSelection
            .Where(x => x.IsSelected)
            .ToList();

        if (!selectedAlgos.Any())
        {
            IsBusy = false;
            return; // Ничего не выбрано
        }

        try
        {
            var results = new Dictionary<string, List<Coordinates>>();

            await Task.Run(() =>
            {
                foreach (var item in selectedAlgos)
                {
                    var algo = item.Algorithm as BasePowerAlgorithm;
                    if (algo == null) continue;

                    var points = new List<Coordinates>();

                    for (var n = 1; n <= PowerMaxN; n++)
                    {
                        var data = algo.Generate(n);
                        algo.Execute(data, 1);
                        points.Add(new Coordinates(n, algo.StepCounter));
                    }

                    results[algo.Name] = points;
                }
            });
            var window = new PowerChartWindow();
            window.PlotResults(results);
            window.Show();
        }
        finally

        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RunIndividualComparisonAsync()
    {
        var selected = IndividualAlgorithms
            .Where(x => x.IsSelected)
            .Select<AlgorithmSelectionViewModel, IAlgorithm>(x => x.Algorithm)
            .ToList();

        if (!selected.Any())
        {
            IsBusy = false;
            return;
        }

        IsBusy = true;
        try
        {
            var allResults = await Task.Run(async () =>
            {
                var allResults = new ChartDataModel();
                allResults.SeriesList.Clear();
                var approxService = new ApproximationService();

                foreach (var algo in selected)
                {
                    var points = await _benchmarkService.RunDynamicBenchmark(
                        algo,
                        MaxN,
                        StepN,
                        AlgorithmStep
                    );

                    var targetComplexity = ComplexityType.Linear;
                    var approxResult = approxService.Fit(points, targetComplexity);

                    allResults.SeriesList.Add(new SeriesData(algo.Name, points, approxResult));
                }

                return allResults;
            });
            var chartWindow = new ChartWindow();
            chartWindow.DataContext = new ChartWindowViewModel(allResults);
            chartWindow.Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenHistoryAsync()
    {
        var desktop = Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime;

        if (desktop?.MainWindow == null) return;

        var historyWin = new HistoryWindow();
        var result = await historyWin.ShowDialog<bool>(desktop.MainWindow);

        if (result && historyWin.SelectedGroups.Any()) PlotHistoryComparison(historyWin.SelectedGroups);
    }

    private void PlotHistoryComparison(List<BenchmarkHistoryGroup> selectedGroups)
    {
        var chartData = new ChartDataModel();

        foreach (var group in selectedGroups)
        {
            var points = group.Points
                .Select(p => new BenchmarkResultPoint(p.N, p.AverageTimeMs))
                .ToList();

            var seriesName = $"{group.AlgorithmId} ({group.CreatedAt:HH:mm:ss})";
            
            var seriesData = new SeriesData(seriesName, points);

            chartData.SeriesList.Add(seriesData);
        }

        
        var chartVm = new ChartWindowViewModel(chartData);

        var chartWindow = new ChartWindow
        {
            DataContext = chartVm
        };

        chartWindow.Show();
    }
}