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
        AppDbContext.MigrateLegacyDatabaseIfNeeded();

        using var db = new AppDbContext();

        // Создаёт файл и таблицы, если их ещё нет.
        db.Database.EnsureCreated();

        // WAL ускоряет запись и не блокирует чтение во время замеров.
        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    }
}
