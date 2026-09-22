using Algorithms.Interfaces;

namespace Algorithms.Lab1.VectorOperations;

public class ProductElements : IAlgorithm<double[]>
{
    public string Name => "Произведение элементов";
    public string Id => "product-elements";
    public int MaxN => 50000;

    public void Execute(double[] data, int step)
    {
        double product = 1;
        for (var i = 0; i < data.Length; i++) product *= data[i];

        var result = product;
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}