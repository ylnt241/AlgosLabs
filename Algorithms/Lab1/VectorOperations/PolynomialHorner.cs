using Algorithms.Interfaces;

namespace Algorithms.Lab1.VectorOperations;

public class PolynomialHorner : IAlgorithm<double[]>
{
    private const double X = 1.5;
    public string Name => "Многочлен: метод Горнера";
    public string Id => "polynomial-horner";
    public int MaxN => 50000;

    public void Execute(double[] v, int step)
    {
        // v содержит коэффициенты многочлена степени n-1.
        // Метод Горнера:
        // P(x) = v_1 + x(v_2 + x(v_3 + ... + x * v_n))
        var result = v[^1];
        for (var k = v.Length - 2; k >= 0; k--) result = result * X + v[k];

        var output = result;
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}