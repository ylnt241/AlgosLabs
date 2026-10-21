using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algorithms.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AlgosLabs.Services;

public class BenchmarkHistoryGroup
{
    public string AlgorithmId { get; set; } = string.Empty;
    public int MaxN { get; set; }
    public int Step { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PointsCount { get; set; }

    public bool IsSelected { get; set; }

    // Точки, попавшие в этот запуск.
    public List<BenchmarkCacheEntity> Points { get; set; } = new();
}

public class HistoryService
{
    /// <summary>
    ///     Загружает историю экспериментов из SQLite, сгруппированную по алгоритмам и времени запуска
    /// </summary>
    public async Task<List<BenchmarkHistoryGroup>> GetHistoryGroupsAsync()
    {
        using var db = new AppDbContext();

        // Забираем всё, что накопилось в кэше.
        var entries = await db.CacheEntries
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        // Точки одного запуска пишутся подряд, поэтому группируем по алгоритму, шагу и минуте.
        var grouped = entries
            .GroupBy(x => new
            {
                x.AlgorithmId,
                x.Step,
                Timestamp = new DateTime(x.CreatedAt.Year, x.CreatedAt.Month, x.CreatedAt.Day, x.CreatedAt.Hour,
                    x.CreatedAt.Minute, 0)
            })
            .Select(g => new BenchmarkHistoryGroup
            {
                AlgorithmId = g.Key.AlgorithmId,
                Step = g.Key.Step,
                MaxN = g.Max(x => x.N),
                CreatedAt = g.Min(x => x.CreatedAt),
                PointsCount = g.Count(),
                Points = g.OrderBy(x => x.N).ToList()
            })
            .ToList();

        return grouped;
    }
}