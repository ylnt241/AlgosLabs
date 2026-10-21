using Algorithms.Interfaces;
using Algorithms.Lab1.VectorOperations;

namespace Algorithms.Lab1.VectorOperations;

public class JumpSearch : IAlgorithm<double[]>, IIndividualAlgorithm
{
    public string Name => "Поиск с прыжками (Журкина)";
    public string Id => "jump-search";
    public int MaxN => 50000;

    public void Execute(double[] v, int step)
    {
        // Сортируем копию: исходный массив трогать нельзя.
        var data = (double[])v.Clone();

        Array.Sort(data);

        // Медиана заведомо есть в массиве — удобная цель для проверки поиска.
        var target = data[data.Length / 2];

        var resultIndex = JumpSearchCore(data, target);

        // Результат нужен только для того, чтобы компилятор не выбросил поиск.
        var output = resultIndex;
    }

    private int JumpSearchCore(double[] arr, double target)
    {
        int n = arr.Length;
        if (n == 0) return -1;

        // Классический Jump Search: шаг √n, затем линейный проход внутри блока.
        int jumpStep = (int)Math.Floor(Math.Sqrt(n));
        int prev = 0;

        while (arr[Math.Min(jumpStep, n) - 1] < target)
        {
            prev = jumpStep;
            jumpStep += (int)Math.Floor(Math.Sqrt(n));
            if (prev >= n) return -1;
        }

        while (arr[prev] < target)
        {
            prev++;
            if (prev == Math.Min(jumpStep, n)) return -1;
        }

        if (arr[prev] == target) return prev;

        return -1;
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}