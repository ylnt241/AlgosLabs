using System.Collections.Generic;

namespace Algorithms.Lab2.Hanoi;

/// <summary>Один ход: какой диск и с какого стержня на какой нужно переместить.</summary>
public readonly record struct HanoiMove(int Disk, int From, int To);

/// <summary>
///     Классическое рекурсивное решение задачи «Ханойские башни» для трёх стержней.
///     Стержни нумеруются с нуля: 0 — начальный (A), 1 — вспомогательный (B), 2 — целевой (C).
/// </summary>
public static class TowerOfHanoi
{
    public const int RodCount = 3;

    /// <summary>Возвращает последовательность ходов для переноса <paramref name="diskCount" /> дисков.</summary>
    public static List<HanoiMove> Solve(int diskCount)
    {
        var moves = new List<HanoiMove>();
        if (diskCount > 0) Build(diskCount, 0, 2, 1, moves);
        return moves;
    }

    private static void Build(int diskCount, int from, int to, int auxiliary, List<HanoiMove> moves)
    {
        if (diskCount <= 0) return;

        Build(diskCount - 1, from, auxiliary, to, moves);
        moves.Add(new HanoiMove(diskCount, from, to));
        Build(diskCount - 1, auxiliary, to, from, moves);
    }
}
