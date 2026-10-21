using System;
using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Algorithms.Infrastructure;

// Одна измеренная точка. CacheKey собирается из алгоритма, типа данных, N и шага.
public class BenchmarkCacheEntity
{
    public int Id { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public string AlgorithmId { get; set; } = string.Empty;
    public int N { get; set; }
    public int Step { get; set; }
    public double AverageTimeMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Общий контекст SQLite-кэша для всех запусков.
public class AppDbContext : DbContext
{
    private const string DatabaseFileName = "benchmark_cache.db";

    /// <summary>
    ///     Файл лежит рядом с исполняемым файлом, поэтому не зависит от рабочего каталога.
    /// </summary>
    public static string DatabasePath => Path.Combine(AppContext.BaseDirectory, DatabaseFileName);

    public DbSet<BenchmarkCacheEntity> CacheEntries => Set<BenchmarkCacheEntity>();

    /// <summary>
    ///     Раньше SQLite использовала относительный путь, поэтому база попадала в текущий каталог
    ///     (при `dotnet run` — в папку проекта). Переносим уже накопленный кэш к исполняемому файлу.
    /// </summary>
    public static void MigrateLegacyDatabaseIfNeeded()
    {
        var targetPath = DatabasePath;
        if (File.Exists(targetPath)) return;

        var legacyPath = Path.Combine(Directory.GetCurrentDirectory(), DatabaseFileName);
        if (!File.Exists(legacyPath)) return;

        if (string.Equals(
                Path.GetFullPath(legacyPath),
                Path.GetFullPath(targetPath),
                StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            File.Copy(legacyPath, targetPath);

            foreach (var suffix in new[] { "-wal", "-shm" })
            {
                var legacySidecar = legacyPath + suffix;
                if (File.Exists(legacySidecar) && !File.Exists(targetPath + suffix))
                    File.Copy(legacySidecar, targetPath + suffix);
            }

            // Старое место после переноса больше не нужно.
            foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
            {
                var legacyFile = legacyPath + suffix;
                if (File.Exists(legacyFile)) File.Delete(legacyFile);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Не удалось перенести старую базу кэша: {ex.Message}");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Один и тот же замер не должен попадать в кэш дважды.
        modelBuilder.Entity<BenchmarkCacheEntity>()
            .HasIndex(e => e.CacheKey)
            .IsUnique();
    }
}
