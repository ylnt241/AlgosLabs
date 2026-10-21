namespace Algorithms.Interfaces;

// Общая часть: по ней UI строит списки и подписи, не зная типа данных.
public interface IAlgorithm
{
    string Id { get; }
    string Name { get; }
    int MaxN { get; }
}

// Типизированная часть: генерация входных данных и сам замер.
public interface IAlgorithm<TData> : IAlgorithm
{
    void Execute(TData data, int step);
    TData Generate(int n);
}