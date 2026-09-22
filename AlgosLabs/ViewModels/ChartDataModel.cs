using System.Collections.Generic;
using Algorithms.Services;
using AlgosLabs.Services;

namespace AlgosLabs.ViewModels;

public record SeriesData(
    string AlgorithmName,
    List<BenchmarkResultPoint> Points,
    ApproximationResult? Approximation = null
);

public class ChartDataModel
{
    public List<SeriesData> SeriesList { get; } = new();
}