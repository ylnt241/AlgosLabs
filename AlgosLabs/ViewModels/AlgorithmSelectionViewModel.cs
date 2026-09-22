using Algorithms.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgosLabs.ViewModels;

public partial class AlgorithmSelectionViewModel : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public AlgorithmSelectionViewModel(IAlgorithm algorithm)
    {
        Algorithm = algorithm;
    }

    public IAlgorithm Algorithm { get; }
}