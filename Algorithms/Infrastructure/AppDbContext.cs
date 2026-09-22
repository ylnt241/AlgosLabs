using Microsoft.EntityFrameworkCore;

namespace Algorithms.Infrastructure;

// Модель одной записи кэша в БД
public class BenchmarkCacheEntity
{
    public int Id { get; set; }
    public string CacheKey { get; set; } = string.Empty; // Уникальный хэш параметров
    public string AlgorithmId { get; set; } = string.Empty; // Идентификатор алгоритма
    public int N { get; set; } // Размерность вектора
    public int Step { get; set; } // Шаг прохода
    public double AverageTimeMs { get; set; } // Среднее время (мс)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Контекст базы данных
public class AppDbContext : DbContext
{
    public DbSet<BenchmarkCacheEntity> CacheEntries => Set<BenchmarkCacheEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // База данных будет создана в папке с исполняемым файлом
        optionsBuilder.UseSqlite("Data Source=benchmark_cache.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Создаем уникальный индекс по CacheKey для молниеносного поиска
        modelBuilder.Entity<BenchmarkCacheEntity>()
            .HasIndex(e => e.CacheKey)
            .IsUnique();
    }
}