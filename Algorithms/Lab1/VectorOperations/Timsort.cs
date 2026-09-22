using System;
using Algorithms.Interfaces;

namespace Algorithms.Lab1.VectorOperations;

public class Timsort : IAlgorithm<double[]>
{
    public string Name => "Timsort";
    public string Id => "tim-sort";
    public int MaxN => 50000;

    private const int MinRun = 32;

    public void Execute(double[] v, int step)
    {
        var data = (double[])v.Clone();
        int n = data.Length;
        if (n < 2) return;

        // Вычисляем минимальный размер «рана» (run)
        int minRun = GetMinRun(MinRun);

        // 1. Сортируем небольшие блоки с помощью сортировки вставками
        for (int i = 0; i < n; i += minRun)
        {
            int end = Math.Min(i + minRun - 1, n - 1);
            InsertionSort(data, i, end);
        }

        // 2. Объединяем отсортированные блоки (слияние)
        for (int size = minRun; size < n; size = 2 * size)
        {
            for (int left = 0; left < n; left += 2 * size)
            {
                int mid = left + size - 1;
                int right = Math.Min(left + 2 * size - 1, n - 1);

                if (mid < right)
                {
                    Merge(data, left, mid, right);
                }
            }
        }
    }

    private static int GetMinRun(int n)
    {
        int r = 0;
        while (n >= 64)
        {
            r |= (n & 1);
            n >>= 1;
        }
        return n + r;
    }

    private static void InsertionSort(double[] arr, int left, int right)
    {
        for (int i = left + 1; i <= right; i++)
        {
            double temp = arr[i];
            int j = i - 1;
            while (j >= left && arr[j] > temp)
            {
                arr[j + 1] = arr[j];
                j--;
            }
            arr[j + 1] = temp;
        }
    }

    private static void Merge(double[] arr, int l, int m, int r)
    {
        int len1 = m - l + 1;
        int len2 = r - m;

        double[] leftArr = new double[len1];
        double[] rightArr = new double[len2];

        Array.Copy(arr, l, leftArr, 0, len1);
        Array.Copy(arr, m + 1, rightArr, 0, len2);

        int i = 0, j = 0, k = l;

        while (i < len1 && j < len2)
        {
            if (leftArr[i] <= rightArr[j])
            {
                arr[k++] = leftArr[i++];
            }
            else
            {
                arr[k++] = rightArr[j++];
            }
        }

        while (i < len1)
        {
            arr[k++] = leftArr[i++];
        }

        while (j < len2)
        {
            arr[k++] = rightArr[j++];
        }
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, n, 10);
        return generator.Generate(n);
    }
}