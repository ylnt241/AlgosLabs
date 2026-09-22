namespace Algorithms.Interfaces;

// 1. Необобщенный базовый интерфейс для UI (MainWindowViewModel / ScottPlot)
public interface IAlgorithm
{
    string Id { get; }
    string Name { get; }
    int MaxN { get; }
}

// 2. Обобщенный интерфейс с генерацией и выполнением
public interface IAlgorithm<TData> : IAlgorithm
{
    void Execute(TData data, int step);
    TData Generate(int n);
}