using Algorithms.Infrastructure;
using AlgosLabs.ViewModels;
using AlgosLabs.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;

namespace AlgosLabs;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        InitializeDatabase();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };

        base.OnFrameworkInitializationCompleted();
    }

    private static void InitializeDatabase()
    {
        using var db = new AppDbContext();

        // EnsureCreated создает файл benchmark_cache.db и все таблицы, если их нет
        db.Database.EnsureCreated();

        // Включаем WAL-режим для максимального ускорения записи/чтения в SQLite
        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    }
}