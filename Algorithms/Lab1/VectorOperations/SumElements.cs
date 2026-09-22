using Algorithms.Interfaces;
namespace Algorithms.Lab1.VectorOperations;

public class SumElements : IAlgorithm<double[]>
{
    public string Name => "Сумма элементов";
    public string Id => "sum-elements";
    public int MaxN => 50000;
    public static double result;
    public void Execute(double[] data, int step)
    {
        double sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += data[i];
        }

        var result = sum;
    }
    
    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}