namespace Algorithms.Lab1.PowerOperations;

public class RecursivePowerAlgorithm : BasePowerAlgorithm
{
    public override string Id => "power-recursive";
    public override string Name => "Рекурсивный";

    public override void Execute(PowerData data, int step)
    {
        StepCounter = 0;
        _ = ComputeRecursive(data.X, data.N);
    }

    private double ComputeRecursive(double x, int n)
    {
        if (n == 0) return 1.0;

        StepCounter++; 
        return x * ComputeRecursive(x, n - 1);
    }
}