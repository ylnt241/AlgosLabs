using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Algorithms.Interfaces;
using Algorithms.Lab1.PowerOperations;
using Algorithms.Services;
using AlgosLabs.Services;
using AlgosLabs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgosLabs.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly UniversalBenchmarkService _benchmarkService = new();
    private readonly AlgorithmScanner _scanner = new();
    
    // Алгоритмы автоматически парсятся в зависимости от интерфейса который они реализуют.
    public ObservableCollection<AlgorithmSelectionViewModel> AvailableAlgorithms { get; } 
        = new ObservableCollection<AlgorithmSelectionViewModel>();

    public ObservableCollection<AlgorithmSelectionViewModel> PowerAlgorithmsSelection { get; } 
        = new ObservableCollection<AlgorithmSelectionViewModel>();
    [ObservableProperty] private int _algorithmStep = 1;
    [ObservableProperty] private bool _isBusy;

    // --- Параметры по-умолчанию для обычных 2D бенчмарков ---
    [ObservableProperty] private int _maxN = 1000;
    [ObservableProperty] private int _stepN = 100;

    // --- Свойства для 3D бенчмарка Матриц (N x M) ---
    [ObservableProperty] private int _startN = 50;
    [ObservableProperty] private int _startM = 50;
    [ObservableProperty] private int _maxM = 500;
    [ObservableProperty] private int _stepM = 50;

    public MainViewModel()
    {
        LoadAlgorithmsAutomatically();
    }
    
    private void LoadAlgorithmsAutomatically()
    {
        AvailableAlgorithms.Clear();
        PowerAlgorithmsSelection.Clear();

        var foundAlgorithms = _scanner.FindAllAlgorithms();

        // Заполняем алгоритмы степеней
        var powerAlgos = foundAlgorithms
            .OfType<IPowerAlgorithm>()
            .Select(a => new AlgorithmSelectionViewModel(a));

        foreach (var item in powerAlgos)
        {
            PowerAlgorithmsSelection.Add(item);
        }

        // Заполняем остальные алгоритмы
        var generalAlgos = foundAlgorithms
            .Where(a => a is not IPowerAlgorithm)
            .Select(a => new AlgorithmSelectionViewModel(a));

        foreach (var item in generalAlgos)
        {
            AvailableAlgorithms.Add(item);
        }
    }

    [RelayCommand]
    private async Task RunComparisonAsync()
    {
        var selected = AvailableAlgorithms
            .Where(x => x.IsSelected)
            .Select<AlgorithmSelectionViewModel, IAlgorithm>(x => x.Algorithm)
            .ToList();

        if (!selected.Any()) return;

        IsBusy = true;

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

        IsBusy = false;

        var chartWindow = new ChartWindow();
        chartWindow.DataContext = new ChartWindowViewModel(allResults);
        chartWindow.Show();
    }

    [RelayCommand]
    private async Task RunMatrix3DBenchmarkAsync()
    {
        IsBusy = true;

        var heatmapData = await Task.Run(() =>
            MatrixBenchmarkService.RunBenchmarkGrid(
                StartN, MaxN, StepN,
                StartM, MaxM, StepM
            )
        );

        IsBusy = false;

        var heatmapWindow = new HeatmapWindow();
        heatmapWindow.LoadDataAndPlot(heatmapData);
        heatmapWindow.Show();
    }

    [ObservableProperty] private int _powerMaxN = 1000;
    [ObservableProperty] private double _baseX = 2.0;

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

        var results = new Dictionary<string, List<ScottPlot.Coordinates>>();

        await Task.Run(() =>
        {
            foreach (var item in selectedAlgos)
            {
                var algo = item.Algorithm as BasePowerAlgorithm;
                if (algo == null) continue;

                var points = new List<ScottPlot.Coordinates>();

                for (int n = 1; n <= PowerMaxN; n++)
                {
                    var data = algo.Generate(n);
                    algo.Execute(data, step: 1);
                    points.Add(new ScottPlot.Coordinates(n, algo.StepCounter));
                }

                results[algo.Name] = points;
            }
        });

        IsBusy = false;

        var window = new PowerChartWindow();
        window.PlotResults(results);
        window.Show();
    }
}