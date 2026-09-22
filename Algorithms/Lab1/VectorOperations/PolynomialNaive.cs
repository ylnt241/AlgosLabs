using Algorithms.Interfaces;

namespace Algorithms.Lab1.VectorOperations;

public class PolynomialNaive : IAlgorithm<double[]>
{
    private const double X = 1.5;
    public string Name => "Многочлен: прямое вычисление";
    public string Id => "polynomial-naive";
    public int MaxN => 50000;

    public void Execute(double[] v, int step)
    {
        // v содержит коэффициенты многочлена степени n-1:
        // P(x) = v_1 + v_2 x + ... + v_n x^(n-1)
        double result = 0;
        for (var k = 1; k <= v.Length; k++)
        {
            // Наивный способ: вычисляем степень x^(k-1) для каждого члена
            double xPower = 1;
            for (var p = 0; p < k - 1; p++) xPower *= X;

            result += v[k - 1] * xPower;
        }

        var output = result;
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}