using Algorithms.Interfaces;

namespace Algorithms.Lab1.VectorOperations;

public class BubbleSort : IAlgorithm<double[]>
{
    public string Name => "Сортировка пузырьком";
    public string Id => "bubble-sort";
    public int MaxN => 50000;

    public void Execute(double[] v, int step)
    {
        var data = (double[])v.Clone();
        for (var i = 0; i < data.Length - 1; i++)
        {
            var swapped = false;
            for (var j = 0; j < data.Length - i - 1; j++)
                if (data[j] > data[j + 1])
                {
                    (data[j], data[j + 1]) = (data[j + 1], data[j]);
                    swapped = true;
                }

            if (!swapped) break;
        }

        var output = data;
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}