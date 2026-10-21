using System;
using System.Collections.Generic;
using System.Diagnostics;
using Algorithms.Lab2.Fractals;
using Algorithms.Services;

namespace AlgosLabs.Services;

/// <summary>
///     Замеряет отрисовку фрактала на одной заранее выделенной поверхности,
///     чтобы в замер не попадало создание буфера.
/// </summary>
public static class FractalBenchmarkService
{
    public static List<BenchmarkResultPoint> Measure(
        IFractalAlgorithm algorithm,
        int width,
        int height,
        int maxDepth,
        int step)
    {
        var result = new List<BenchmarkResultPoint>();
        var canvas = new FractalCanvas(width, height);

        var depthLimit = Math.Clamp(maxDepth, 1, algorithm.MaxN);
        var depthStep = Math.Max(1, step);

        // Прогрев JIT, иначе первый замер будет заметно выше остальных.
        algorithm.Render(canvas, Math.Min(2, depthLimit));

        // Несколько прогонов сглаживают шум планировщика.
        const int runs = 3;

        for (var depth = 1; depth <= depthLimit; depth += depthStep)
        {
            var stopwatch = Stopwatch.StartNew();
            for (var run = 0; run < runs; run++)
                algorithm.Render(canvas, depth);
            stopwatch.Stop();

            result.Add(new BenchmarkResultPoint(depth, stopwatch.Elapsed.TotalMilliseconds / runs));
        }

        return result;
    }
}
