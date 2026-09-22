using Algorithms.Interfaces;

namespace Algorithms.Lab1.VectorOperations;

public class Timsort : IAlgorithm<double[]>
{
    public string Name => "Сортировка: Timsort (Array.Sort)";
    public string Id => "timsort";
    public int MaxN => 50000;
    public void Execute(double[] v, int step)
    {
        var data = (double[])v.Clone();

        // Стандартная реализация сортировки из стандартной библиотеки .NET
        // (аналог sorted() в Python / Timsort)
        Array.Sort(data);

        var output = data;
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}