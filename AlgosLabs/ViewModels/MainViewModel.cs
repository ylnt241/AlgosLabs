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
    // Используем правильное имя универсального сервиса
    private readonly UniversalBenchmarkService _benchmarkService = new();
    private readonly AlgorithmScanner _scanner = new();
    [ObservableProperty] private int _algorithmStep = 1;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private int _maxN = 5000;
    [ObservableProperty] private int _stepN = 250;

    public MainViewModel()
    {
        LoadAlgorithmsAutomatically();
    }

    // Свойство должно быть публичным, так как к нему привязывается View
    public ObservableCollection<AlgorithmSelectionViewModel> AvailableAlgorithms { get; } = new();

    private void LoadAlgorithmsAutomatically()
    {
        AvailableAlgorithms.Clear();

        // Сканируем проект и автоматически находим все реализованные алгоритмы
        var foundAlgorithms = _scanner.FindAllAlgorithms();

        foreach (var algo in foundAlgorithms) AvailableAlgorithms.Add(new AlgorithmSelectionViewModel(algo));
    }

    [RelayCommand]
    private async Task RunComparisonAsync()
    {
        // Явно указываем тип в Select, чтобы устранить ошибку вывода типов Enumerable.Select
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

            // Определяем теоретическую сложность (например, Linear для O(N) или Constant для O(1))
            // Можно автоматически выбирать тип на основе алгоритма
            var targetComplexity = ComplexityType.Linear;

            var approxResult = approxService.Fit(points, targetComplexity);

            allResults.SeriesList.Add(new SeriesData(algo.Name, points, approxResult));
        }

        IsBusy = false;

        // Открываем отдельное окно для отображения графиков ScottPlot
        var chartWindow = new ChartWindow();
        chartWindow.DataContext = new ChartWindowViewModel(allResults);
        chartWindow.Show();
    }
}