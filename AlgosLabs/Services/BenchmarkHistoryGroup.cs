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

    // Список всех точек этого прогона
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

        // 1. Получаем все записи из кэша БД
        var entries = await db.CacheEntries
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        // 2. Группируем записи по AlgorithmId и дате/времени (округляем до минуты/секунды)
        // Чтобы объединить точки одного запуска N = 10..MaxN в одну группу
        var grouped = entries
            .GroupBy(x => new
            {
                x.AlgorithmId,
                x.Step,
                // Группируем по минуте запуска, чтобы точки одного теста попали в одну группу
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