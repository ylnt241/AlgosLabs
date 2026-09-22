using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Algorithms.Interfaces;
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

    [ObservableProperty] private int _algorithmStep = 1;
    [ObservableProperty] private bool _isBusy;

    // --- Параметры для обычных 2D бенчмарков ---
    [ObservableProperty] private int _maxN = 1000;
    [ObservableProperty] private int _stepN = 100;

    // --- Добавляем новые свойства для 3D бенчмарка Матриц (N x M) ---
    [ObservableProperty] private int _startN = 50;
    [ObservableProperty] private int _startM = 50;
    [ObservableProperty] private int _maxM = 500;
    [ObservableProperty] private int _stepM = 50;

    public MainViewModel()
    {
        LoadAlgorithmsAutomatically();
    }

    public ObservableCollection<AlgorithmSelectionViewModel> AvailableAlgorithms { get; } = new();

    private void LoadAlgorithmsAutomatically()
    {
        AvailableAlgorithms.Clear();
        var foundAlgorithms = _scanner.FindAllAlgorithms();

        foreach (var algo in foundAlgorithms) 
            AvailableAlgorithms.Add(new AlgorithmSelectionViewModel(algo));
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
}