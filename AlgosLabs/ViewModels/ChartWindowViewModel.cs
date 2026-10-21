using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgosLabs.ViewModels;

public class ChartWindowViewModel : ObservableObject
{
    public ChartWindowViewModel(
        ChartDataModel chartData,
        string title = "Эмпирические данные vs Аппроксимация T_approx(n) = C·f(n)",
        string xLabel = "Размерность данных (N)",
        string yLabel = "Время выполнения (мс)")
    {
        ChartData = chartData;
        Title = title;
        XLabel = xLabel;
        YLabel = yLabel;
    }

    public ChartDataModel ChartData { get; }

    public string Title { get; }

    public string XLabel { get; }

    public string YLabel { get; }
}
