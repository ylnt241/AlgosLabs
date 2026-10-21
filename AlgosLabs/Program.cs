using System;
using Avalonia;

namespace AlgosLabs;

internal sealed class Program
{
    // До запуска AppMain нельзя трогать Avalonia и SynchronizationContext:
    // платформа ещё не инициализирована.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Этот метод нужен ещё и визуальному дизайнеру — не удалять.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}

internal class AlgorithmRunHistory
{
}