using Algorithms.Interfaces;

namespace Algorithms.Lab1.PowerOperations;

public abstract class BasePowerAlgorithm : IAlgorithm<PowerData>, IPowerAlgorithm
{
    // В свойство StepCounter накапливаем количество элементарных операций
    public long StepCounter { get; protected set; }
    public abstract string Id { get; }
    public abstract string Name { get; }
    public virtual int MaxN { get; } = 10000;

    public PowerData Generate(int n)
    {
        return new PowerData(2.0, n);
    }

    public abstract void Execute(PowerData data, int step);
}