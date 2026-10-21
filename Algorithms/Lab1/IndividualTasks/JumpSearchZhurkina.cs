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
        // Клонируем массив, чтобы не модифицировать оригинал
        var data = (double[])v.Clone();

        // Сортируем массив (Jump Search работает только на отсортированных данных)
        Array.Sort(data);

        // Ищем медиану (элемент в середине массива)
        var target = data[data.Length / 2];

        // Выполняем поиск
        var resultIndex = JumpSearchCore(data, target);

        // Результат не возвращается, так как интерфейс предназначен для замера времени
        var output = resultIndex;
    }

    private int JumpSearchCore(double[] arr, double target)
    {
        int n = arr.Length;
        if (n == 0) return -1;

        // Оптимальный размер шага = √n
        int jumpStep = (int)Math.Floor(Math.Sqrt(n));
        int prev = 0;

        // Прыгаем по массиву, пока не найдем блок, где может быть целевой элемент
        while (arr[Math.Min(jumpStep, n) - 1] < target)
        {
            prev = jumpStep;
            jumpStep += (int)Math.Floor(Math.Sqrt(n));
            if (prev >= n) return -1;
        }

        // Линейный поиск в найденном блоке
        while (arr[prev] < target)
        {
            prev++;
            if (prev == Math.Min(jumpStep, n)) return -1;
        }

        // Проверяем, нашли ли мы целевой элемент
        if (arr[prev] == target) return prev;

        return -1;
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}