using System.Collections.Generic;

namespace AlgosLabs.Models;

/// <summary>
///     Снимок положения башни: диски на каждом стержне перечислены снизу вверх.
/// </summary>
public sealed class HanoiState
{
    public const int RodCount = 3;

    public HanoiState(int diskCount, IReadOnlyList<int[]> rods, int stepIndex)
    {
        DiskCount = diskCount;
        Rods = rods;
        StepIndex = stepIndex;
    }

    /// <summary>Общее количество дисков (N).</summary>
    public int DiskCount { get; }

    /// <summary>Содержимое стержней: массивы размеров дисков снизу вверх.</summary>
    public IReadOnlyList<int[]> Rods { get; }

    /// <summary>Количество уже выполненных ходов.</summary>
    public int StepIndex { get; }

    /// <summary>Буквенное имя стержня: A, B или C.</summary>
    public static string RodName(int index) => ((char)('A' + index)).ToString();
}
