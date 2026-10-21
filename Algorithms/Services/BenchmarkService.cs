using System.Diagnostics;
using Algorithms.Infrastructure;
using Algorithms.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Algorithms.Services;

public record BenchmarkResultPoint(int N, double AverageTimeMs);

public class UniversalBenchmarkService
{
    /// <summary>
    ///     Прогоняет алгоритм, тип данных которого становится известен только в рантайме.
    /// </summary>
    public async Task<List<BenchmarkResultPoint>> RunDynamicBenchmark(
        IAlgorithm algorithm,
        int maxN,
        int stepN,
        int algorithmStep)
    {
        // IAlgorithm<T> приходится искать рефлексией: конкретный T здесь неизвестен.
        var interfaceType = Array.Find(algorithm.GetType().GetInterfaces(),
            i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAlgorithm<>));

        if (interfaceType == null)
            throw new InvalidOperationException($"Алгоритм '{algorithm.Name}' не реализует IAlgorithm<TData>.");

        var dataType = interfaceType.GetGenericArguments()[0];

        // Подставляем найденный тип в обобщённый метод и вызываем его.
        var method = typeof(UniversalBenchmarkService)
            .GetMethod(nameof(RunBenchmarkAsync))!
            .MakeGenericMethod(dataType);

        var task = (Task<List<BenchmarkResultPoint>>)method.Invoke(this, new object?[]
        {
            algorithm,
            maxN,
            stepN,
            algorithmStep,
            null // прогресс в этом сценарии не нужен
        })!;

        return await task;
    }

    /// <summary>
    ///     Замер по сетке N с кэшированием уже посчитанных точек.
    /// </summary>
    public async Task<List<BenchmarkResultPoint>> RunBenchmarkAsync<TData>(
        IAlgorithm<TData> algorithm,
        int maxN,
        int stepN,
        int algorithmStep,
        IProgress<double>? progress = null)
    {
        WarmupAlgorithm(algorithm, algorithmStep);

        var results = new List<BenchmarkResultPoint>();
        var totalSteps = (maxN - 1) / stepN + 1;
        var currentStepCount = 0;

        for (var n = 1; n <= maxN; n += stepN)
        {
            var cacheKey = $"{algorithm.Id}_{typeof(TData).Name}_N{n}_Step{algorithmStep}";

            // Сначала смотрим в кэш, чтобы не мерить одно и то же повторно.
            var averageMs = await GetFromCacheAsync(cacheKey)
                            ?? await MeasureAndSaveAsync(algorithm, cacheKey, n, algorithmStep);

            results.Add(new BenchmarkResultPoint(n, averageMs));

            currentStepCount++;
            progress?.Report((double)currentStepCount / totalSteps * 100);
        }

        return results;
    }

    private static void WarmupAlgorithm<TData>(IAlgorithm<TData> algorithm, int algorithmStep)
    {
        try
        {
            // Холостые прогоны, чтобы JIT не попал в первый замер.
            var warmupData = algorithm.Generate(10);
            for (var i = 0; i < 5; i++) algorithm.Execute(warmupData, algorithmStep);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch
        {
            // Прогрев не должен ломать основной запуск.
        }
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

        // Несколько итераций внутри одного замера сглаживают шум ОС.
        var innerLoops = 5;
        var runs = new double[5];

        for (var run = 0; run < 5; run++)
        {
            // Вход готовим под текущий N.
            var inputData = algorithm.Generate(n);

            // Мусор убираем до секундомера, иначе он попадёт в измерение.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < innerLoops; i++) algorithm.Execute(inputData, algorithmStep);
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