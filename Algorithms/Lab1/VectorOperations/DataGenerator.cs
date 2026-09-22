namespace Algorithms.Lab1.VectorOperations;

public class DataGenerator(int? userN, int maxN, int step)
{
    private readonly Random _random = new();

    public int UserN { get; set; } = userN ?? maxN;
    public int MaxN { get; } = maxN;
    public int Step { get; } = step;

    public double[] Generate(int n)
    {
        var vector = new double[n];
        for (var i = 0; i < n; i++) vector[i] = _random.NextDouble() * 100.0;
        return vector;
    }
}