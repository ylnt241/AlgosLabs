using Algorithms.Interfaces;
using Algorithms.Lab1.VectorOperations;

namespace Algorithms.Lab1.IndividualTasks;

public class QuickSelectChikishev : IAlgorithm<double[]>, IIndividualAlgorithm
{
    public string Id => "quick-select";
    public string Name => "Quick Select (Чикишев)";
    public int MaxN => 2000;

    public void Execute(double[] data, int step)
    {
        if (data == null || data.Length == 0)
            return;

        // Делаем копию, чтобы измерения времени на разных прогонах 
        // проходили на неотсортированных данных
        var array = new double[data.Length];
        Array.Copy(data, array, data.Length);

        // Поиск k-й порядковой статистики (по умолчанию — медиана)
        var k = array.Length / 2;
        QuickSelect(array, 0, array.Length - 1, k, step);
    }

    public double[] Generate(int n)
    {
        var generator = new DataGenerator(null, MaxN, 10);
        return generator.Generate(n);
    }

    private static double QuickSelect(double[] array, int left, int right, int k, int step)
    {
        while (left <= right)
        {
            if (left == right)
                return array[left];

            var pivotIndex = Partition(array, left, right, step);

            if (k == pivotIndex)
                return array[k];

            if (k < pivotIndex)
                right = pivotIndex - 1;
            else
                left = pivotIndex + 1;
        }

        return array[left];
    }

    private static int Partition(double[] array, int left, int right, int step)
    {
        // Выбираем опорный элемент
        var pivotIndex = left + (right - left) / 2;
        var pivotValue = array[pivotIndex];

        // Переносим pivot в конец
        (array[pivotIndex], array[right]) = (array[right], array[pivotIndex]);

        var storeIndex = left;

        for (var i = left; i < right; i += step)
            if (array[i] < pivotValue)
            {
                (array[storeIndex], array[i]) = (array[i], array[storeIndex]);
                storeIndex++;
            }

        // Возвращаем pivot на итоговое место
        (array[storeIndex], array[right]) = (array[right], array[storeIndex]);
        return storeIndex;
    }
}