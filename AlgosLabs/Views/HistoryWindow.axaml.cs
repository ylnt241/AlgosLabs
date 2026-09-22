using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AlgosLabs.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AlgosLabs.Views;

public partial class HistoryWindow : Window
{
    private readonly HistoryService _historyService = new();

    public HistoryWindow()
    {
        InitializeComponent();
        LoadHistoryAsync();
    }

    public List<BenchmarkHistoryGroup> SelectedGroups { get; private set; } = new();

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void LoadHistoryAsync()
    {
        try
        {
            var history = await _historyService.GetHistoryGroupsAsync();

            var grid = this.FindControl<DataGrid>("HistoryGrid");
            if (grid != null) grid.ItemsSource = history;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки истории из БД: {ex.Message}");
        }
    }

    public void OnCompareClick(object? sender, RoutedEventArgs e)
    {
        var grid = this.FindControl<DataGrid>("HistoryGrid");
        if (grid?.ItemsSource is List<BenchmarkHistoryGroup> items)
            SelectedGroups = items.Where(x => x.IsSelected).ToList();

        Close(true);
    }

    public void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}