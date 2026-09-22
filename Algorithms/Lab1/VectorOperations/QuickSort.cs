using Algorithms.Interfaces;

namespace Algorithms.Lab1.VectorOperations;

public class QuickSort : IAlgorithm<double[]>
{
    public string Name => "Сортировка: быстрая (Quick sort)";
    public string Id => "quick-sort";
    public int MaxN => 50000;
    public void Execute(double[] v, int step)
    {
        var data = (double[])v.Clone();
        QuickSortCore(data, 0, data.Length - 1);
        var output = data;
    }

    private static void QuickSortCore(double[] arr, int low, int high)
    {
        while (low < high)
        {
            int pivotIndex = Partition(arr, low, high);

            // Сначала сортируем меньшую часть (уменьшаем глубину рекурсии)
            if (pivotIndex - low < high - pivotIndex)
            {
                QuickSortCore(arr, low, pivotIndex - 1);
                low = pivotIndex + 1;
            }
            else
            {
                QuickSortCore(arr, pivotIndex + 1, high);
                high = pivotIndex - 1;
            }
        }
    }

    private static int Partition(double[] arr, int low, int high)
    {
        // Опорный элемент — последний (схема Ломуто)
        double pivot = arr[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            if (arr[j] <= pivot)
            {
                i++;
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }

        (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
        return i + 1;
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}