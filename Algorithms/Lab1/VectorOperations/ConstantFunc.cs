using Algorithms.Interfaces;

namespace Algorithms.Lab1.VectorOperations;

public class ConstantFunc : IAlgorithm<double[]>
{
    private int b;
    public string Name => "Постоянная функция";
    public string Id => "constant-function";
    public int MaxN => 50000;

    public void Execute(double[] v, int step)
    {
        b++;
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}