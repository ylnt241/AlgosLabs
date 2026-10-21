using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Algorithms.Infrastructure;
using Algorithms.Interfaces;
using Algorithms.Lab1.PowerOperations;
using Algorithms.Lab2.Fractals;
using Algorithms.Services;
using AlgosLabs.Services;
using AlgosLabs.Views;
using AlgosLabs.Views.Sections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScottPlot;

namespace AlgosLabs.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly UniversalBenchmarkService _benchmarkService = new();
    private readonly AlgorithmScanner _scanner = new();

    /// <summary>Единственный рекурсивный алгоритм отрисовки фракталов (лаба 2).</summary>
    private readonly IFractalAlgorithm _fractalAlgorithm = new SierpinskiCarpet();

    private const string FractalSectionId = "Lab2.Fractals";
    [ObservableProperty] private int _algorithmStep = 1;
    [ObservableProperty] private double _baseX = 2.0;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _maxM = 500;

    // Значения по умолчанию для обычных 2D-бенчмарков.
    [ObservableProperty] private int _maxN = 1000;

    [ObservableProperty] private int _powerMaxN = 1000;
    [ObservableProperty] private int _startM = 50;

    // Сетка для 3D-бенчмарка матриц (N x M).
    [ObservableProperty] private int _startN = 50;
    [ObservableProperty] private int _stepM = 50;
    [ObservableProperty] private int _stepN = 100;

    public MainViewModel()
    {
        LoadAlgorithmsAutomatically();
        BuildLabs();
    }

    // Списки заполняет сканер — вручную регистрировать алгоритмы не нужно.
    public ObservableCollection<AlgorithmSelectionViewModel> AvailableAlgorithms { get; } = new();

    public ObservableCollection<AlgorithmSelectionViewModel> PowerAlgorithmsSelection { get; } = new();

    public ObservableCollection<AlgorithmSelectionViewModel> IndividualAlgorithms { get; } = new();

    // Навигация: лабы и их разделы.

    /// <summary>
    ///     Все лабы приложения. Чтобы добавить новую лабу, достаточно дописать её в <see cref="BuildLabs" />.
    /// </summary>
    public ObservableCollection<LabViewModel> Labs { get; } = new();

    /// <summary>
    ///     Представления разделов создаются один раз и кэшируются,
    ///     поэтому состояние раздела (выбранные алгоритмы и т.п.) не теряется при переключении.
    /// </summary>
    private readonly Dictionary<string, Control> _sectionViews = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedLab))]
    [NotifyPropertyChangedFor(nameof(HasSelectedSection))]
    [NotifyPropertyChangedFor(nameof(CurrentSectionView))]
    private LabViewModel? _selectedLab;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedSection))]
    [NotifyPropertyChangedFor(nameof(CurrentSectionView))]
    private LabSectionViewModel? _selectedSection;

    /// <summary>Есть ли выбранная лаба (управляет видимостью нижней навигации).</summary>
    public bool HasSelectedLab => SelectedLab is not null;

    /// <summary>Есть ли выбранный раздел (управляет видимостью контента).</summary>
    public bool HasSelectedSection => SelectedSection is not null;

    /// <summary>Представление текущего раздела.</summary>
    public Control? CurrentSectionView =>
        SelectedSection is { } section && _sectionViews.TryGetValue(section.Id, out var view)
            ? view
            : null;

    // Фракталы (лаба 2).

    /// <summary>Рекурсивный алгоритм отрисовки фрактала.</summary>
    public IFractalAlgorithm FractalAlgorithm => _fractalAlgorithm;

    [ObservableProperty] private int _fractalPreviewDepth = 8;
    [ObservableProperty] private int _fractalMaxDepth = 8;
    [ObservableProperty] private int _fractalStep = 1;
    [ObservableProperty] private string _fractalPreviewInfo = "Фрактал ещё не отрисован";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFractalPreview))]
    private WriteableBitmap? _fractalPreview;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanvasSize))]
    private int _canvasSizeIndex = 1;

    /// <summary>Размер холста для предпросмотра и замеров: 256, 512 или 1024 пикселя.</summary>
    public int CanvasSize => CanvasSizeIndex switch
    {
        0 => 256,
        2 => 1024,
        _ => 512
    };

    public bool HasFractalPreview => FractalPreview is not null;

    private void LoadAlgorithmsAutomatically()
    {
        AvailableAlgorithms.Clear();
        PowerAlgorithmsSelection.Clear();
        IndividualAlgorithms.Clear();

        var foundAlgorithms = _scanner.FindAllAlgorithms();

        // Алгоритмы возведения в степень.
        var powerAlgos = foundAlgorithms
            .OfType<IPowerAlgorithm>()
            .Select(a => new AlgorithmSelectionViewModel(a));

        foreach (var item in powerAlgos) PowerAlgorithmsSelection.Add(item);

        var generalAlgos = foundAlgorithms
            .Where(a => a is not (IPowerAlgorithm or IIndividualAlgorithm or IFractalAlgorithm))
            .Select(a => new AlgorithmSelectionViewModel(a));

        foreach (var item in generalAlgos) AvailableAlgorithms.Add(item);

        var individualAlgos = foundAlgorithms
            .OfType<IIndividualAlgorithm>()
            .Select(a => new AlgorithmSelectionViewModel(a));

        foreach (var item in individualAlgos) IndividualAlgorithms.Add(item);
    }

    /// <summary>
    ///     Собирает меню лаб и разделов. Новые лабы и разделы добавляются здесь.
    /// </summary>
    private void BuildLabs()
    {
        // Лаба 1.
        var lab1 = new LabViewModel { Title = "1 Лаба" };
        AddSection(lab1, "Lab1.Vector", "Векторные операции", () => new VectorOperationsSectionView());
        AddSection(lab1, "Lab1.Matrix", "Матрицы", () => new MatrixSectionView());
        AddSection(lab1, "Lab1.Power", "Возведение в степень", () => new PowerSectionView());
        AddSection(lab1, "Lab1.Individual", "Индивидуальное задание", () => new IndividualSectionView());
        Labs.Add(lab1);

        // Лаба 2.
        var lab2 = new LabViewModel { Title = "2 Лаба" };
        AddSection(lab2, FractalSectionId, "Отрисовка фракталов", () => new FractalsSectionView());

        var hanoiViewModel = new HanoiViewModel();
        AddSection(lab2, "Lab2.Hanoi", "Ханойские башни",
            () => new HanoiSectionView { DataContext = hanoiViewModel });

        AddPlaceholderSection(lab2, "Lab2.Individual", "Индивидуальное задание (Чикишев)",
            "Здесь появится индивидуальное задание второй лабы (вариант Чикишева).");
        Labs.Add(lab2);
    }

    /// <summary>Добавляет раздел с готовым представлением.</summary>
    private void AddSection(LabViewModel lab, string id, string title, Func<Control> createView)
    {
        var section = new LabSectionViewModel { Id = id, Title = title };
        lab.Sections.Add(section);

        var view = createView();
        // Если у раздела нет своей VM, используем главную.
        view.DataContext ??= this;
        _sectionViews[id] = view;
    }

    /// <summary>Добавляет раздел-заглушку: пока только визуал, без функциональности.</summary>
    private void AddPlaceholderSection(LabViewModel lab, string id, string title, string description)
    {
        var section = new LabSectionViewModel { Id = id, Title = title, Description = description };
        lab.Sections.Add(section);
        _sectionViews[id] = new PlaceholderSectionView { DataContext = section };
    }

    /// <summary>Выбор лабы: показывает её разделы в нижней навигации и открывает раздел.</summary>
    [RelayCommand]
    private void SelectLab(LabViewModel? lab)
    {
        if (lab is null) return;

        if (SelectedLab is { } previous && !ReferenceEquals(previous, lab)) previous.IsSelected = false;

        SelectedLab = lab;
        lab.IsSelected = true;

        // Возвращаемся к последнему открытому разделу лабы.
        SelectSection(lab.SelectedSection ?? lab.Sections.FirstOrDefault());
    }

    /// <summary>Выбор раздела выбранной лабы.</summary>
    [RelayCommand]
    private void SelectSection(LabSectionViewModel? section)
    {
        if (section is null) return;

        if (SelectedLab?.SelectedSection is { } previous && !ReferenceEquals(previous, section))
            previous.IsSelected = false;

        section.IsSelected = true;

        if (SelectedLab is not null) SelectedLab.SelectedSection = section;

        SelectedSection = section;
    }

    /// <summary>Отрисовывает фрактал в предпросмотре и замеряет время отрисовки.</summary>
    [RelayCommand]
    private async Task RenderFractalPreviewAsync()
    {
        var depth = Math.Clamp(FractalPreviewDepth, 1, FractalAlgorithm.MaxN);
        FractalPreviewDepth = depth;
        var size = CanvasSize;

        IsBusy = true;
        try
        {
            var (canvas, elapsedMs) = await Task.Run(() =>
            {
                var target = new FractalCanvas(size, size);
                var stopwatch = Stopwatch.StartNew();
                FractalAlgorithm.Render(target, depth);
                stopwatch.Stop();
                return (target, stopwatch.Elapsed.TotalMilliseconds);
            });

            FractalPreview = CreateBitmap(canvas);
            FractalPreviewInfo = $"{FractalAlgorithm.Name} · глубина {depth} · {size}×{size} · {elapsedMs:F1} мс";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка отрисовки фрактала: {ex}");
            FractalPreviewInfo = $"Не удалось отрисовать фрактал: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Замеряет время отрисовки фрактала на разной глубине и строит график.</summary>
    [RelayCommand]
    private async Task RunFractalBenchmarkAsync()
    {
        var size = CanvasSize;
        var maxDepth = Math.Clamp(FractalMaxDepth, 1, FractalAlgorithm.MaxN);
        var step = Math.Max(1, FractalStep);
        FractalMaxDepth = maxDepth;

        IsBusy = true;
        try
        {
            var chartData = await Task.Run(() =>
            {
                var model = new ChartDataModel();
                model.SeriesList.Add(new SeriesData(
                    FractalAlgorithm.Name,
                    FractalBenchmarkService.Measure(FractalAlgorithm, size, size, maxDepth, step)));
                return model;
            });

            var chartWindow = new ChartWindow
            {
                DataContext = new ChartWindowViewModel(
                    chartData,
                    "Время рекурсивной отрисовки фрактала",
                    "Глубина рекурсии (N)",
                    "Время отрисовки (мс)")
            };
            chartWindow.Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Копирует BGRA-буфер холста в WriteableBitmap.</summary>
    private static WriteableBitmap CreateBitmap(FractalCanvas canvas)
    {
        var bitmap = new WriteableBitmap(
            new Avalonia.PixelSize(canvas.Width, canvas.Height),
            new Avalonia.Vector(96, 96),
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Opaque);

        using var buffer = bitmap.Lock();
        var rowBytes = canvas.Width * 4;

        for (var y = 0; y < canvas.Height; y++)
            Marshal.Copy(canvas.BgraPixels, y * rowBytes, IntPtr.Add(buffer.Address, y * buffer.RowBytes), rowBytes);

        return bitmap;
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
            return;
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
    
    [RelayCommand]
    private async Task ClearHistoryAndCacheAsync()
    {
        IsBusy = true;
        try
        {
            // Удаление может занять время — уводим в фон.
            await Task.Run(() =>
            {
                using var db = new AppDbContext();
            
                db.CacheEntries.RemoveRange(db.CacheEntries);
                db.SaveChanges();
                
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при очистке истории: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}