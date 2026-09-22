namespace Algorithms.Lab1.PowerOperations;

public class IterativePowerAlgorithm : BasePowerAlgorithm
{
    public override string Id => "power-iterative";
    public override string Name => "Простой итеративный";

    public override void Execute(PowerData data, int step)
    {
        StepCounter = 0;
        var result = 1.0;

        for (var i = 0; i < data.N; i++)
        {
            StepCounter++; // Подсчитываем умножение
            result *= data.X;
        }
    }
}