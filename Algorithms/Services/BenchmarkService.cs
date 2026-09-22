using System.Diagnostics;
using Algorithms.Infrastructure;
using Algorithms.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Algorithms.Services;

public record BenchmarkResultPoint(int N, double AverageTimeMs);

public class UniversalBenchmarkService
{
    /// <summary>
    ///     Динамический запуск любого алгоритма через Reflection
    /// </summary>
    public async Task<List<BenchmarkResultPoint>> RunDynamicBenchmark(
        IAlgorithm algorithm,
        int maxN,
        int stepN,
        int algorithmStep)
    {
        // 1. Ищем интерфейс IAlgorithm<TData>
        var interfaceType = Array.Find(algorithm.GetType().GetInterfaces(),
            i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAlgorithm<>));

        if (interfaceType == null)
            throw new InvalidOperationException($"Алгоритм '{algorithm.Name}' не реализует IAlgorithm<TData>.");

        var dataType = interfaceType.GetGenericArguments()[0];

        // 2. Находим универсальный метод RunBenchmarkAsync<TData>
        var method = typeof(UniversalBenchmarkService)
            .GetMethod(nameof(RunBenchmarkAsync))!
            .MakeGenericMethod(dataType);

        // 3. Вызываем с 5 параметрами (без стороннего генератора!)
        var task = (Task<List<BenchmarkResultPoint>>)method.Invoke(this, new object?[]
        {
            algorithm,
            maxN,
            stepN,
            algorithmStep,
            null // progress
        })!;

        return await task;
    }

    /// <summary>
    ///     Основной универсальный замер
    /// </summary>
    public async Task<List<BenchmarkResultPoint>> RunBenchmarkAsync<TData>(
        IAlgorithm<TData> algorithm,
        int maxN,
        int stepN,
        int algorithmStep,
        IProgress<double>? progress = null)
    {
        var results = new List<BenchmarkResultPoint>();
        var totalSteps = (maxN - 1) / stepN + 1;
        var currentStepCount = 0;

        for (var n = 1; n <= maxN; n += stepN)
        {
            var cacheKey = $"{algorithm.Id}_{typeof(TData).Name}_N{n}_Step{algorithmStep}";

            // 1. Проверяем кэш в SQLite
            var averageMs = await GetFromCacheAsync(cacheKey)
                            ?? await MeasureAndSaveAsync(algorithm, cacheKey, n, algorithmStep);

            results.Add(new BenchmarkResultPoint(n, averageMs));

            currentStepCount++;
            progress?.Report((double)currentStepCount / totalSteps * 100);
        }

        return results;
    }

    private async Task<double?> GetFromCacheAsync(string cacheKey)
    {
        using var db = new AppDbContext();
        var entry = await db.CacheEntries.FirstOrDefaultAsync(x => x.CacheKey == cacheKey);
        return entry?.AverageTimeMs;
    }

    private async Task<double> MeasureAndSaveAsync<TData>(
        IAlgorithm<TData> algorithm,
        string cacheKey,
        int n,
        int algorithmStep)
    {
        var generateMethod = algorithm.GetType().GetMethod("Generate", new[] { typeof(int) });
    
        // Количество итераций для сглаживания системного шума
        int innerLoops = 2;
        var runs = new double[5];

        for (var run = 0; run < 5; run++)
        {
// Вызываем Generate(n) с текущим N, либо дефолтный Generate()
            TData inputData = algorithm.Generate(n);
// Прогрев JIT (Warmup)
            algorithm.Execute(inputData, algorithmStep);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < innerLoops; i++)
            {
                algorithm.Execute(inputData, algorithmStep);
            } 
            sw.Stop();
            runs[run] = sw.Elapsed.TotalMilliseconds / innerLoops;
        }

        var averageMs = runs.Average();

        using (var db = new AppDbContext())
        {
            db.CacheEntries.Add(new BenchmarkCacheEntity
            {
                CacheKey = cacheKey,
                AlgorithmId = algorithm.Id,
                N = n,
                Step = algorithmStep,
                AverageTimeMs = averageMs
            });

            await db.SaveChangesAsync();
        }

        return averageMs;
    }
}