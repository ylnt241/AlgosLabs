using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgosLabs.ViewModels;

public class ChartWindowViewModel : ObservableObject
{
    public ChartWindowViewModel(ChartDataModel chartData)
    {
        ChartData = chartData;
    }

    public ChartDataModel ChartData { get; }
}