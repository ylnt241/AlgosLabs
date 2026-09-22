namespace Algorithms.Lab1.PowerOperations;

public class BinaryAlgorithm : BasePowerAlgorithm
{
    public override string Id => "power_fast";
    public override string Name => "Быстрый бинарный";

    public override void Execute(PowerData data, int step)
    {
        StepCounter = 0;
        _ = ComputeFast(data.X, data.N);
    }

    private double ComputeFast(double x, int n)
    {
        if (n == 0) return 1.0;

        if (n % 2 == 0)
        {
            StepCounter++; // Подсчитываем умножение (half * half)
            var half = ComputeFast(x, n / 2);
            return half * half;
        }

        StepCounter += 2; // Подсчитываем умножения (x * ...)
        return x * ComputeFast(x, n - 1);
    }
}