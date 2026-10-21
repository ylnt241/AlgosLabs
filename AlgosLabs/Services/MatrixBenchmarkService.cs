using System;
using System.Diagnostics;
using Algorithms.Lab1.MatrixOperations;

namespace AlgosLabs.Services;

public class MatrixBenchmarkService
{
    private static readonly Random _random = new();

    // Случайная матрица rows x cols.
    public static double[,] GenerateMatrix(int rows, int cols)
    {
        var matrix = new double[rows, cols];
        for (var i = 0; i < rows; i++)
        for (var j = 0; j < cols; j++)
            matrix[i, j] = _random.NextDouble() * 100.0;

        return matrix;
    }

    public static double MeasureMultiplyTime(int n, int m, int k = 100)
    {
        // A: n x k, B: k x m, C: n x m.
        var a = GenerateMatrix(n, k);
        var b = GenerateMatrix(k, m);
        var c = new double[n, m];

        var sw = Stopwatch.StartNew();

        for (var i = 0; i < n; i++)
        for (var j = 0; j < m; j++)
        {
            double sum = 0;
            for (var p = 0; p < k; p++) sum += a[i, p] * b[p, j];
            c[i, j] = sum;
        }

        sw.Stop();
        return sw.Elapsed.TotalMilliseconds;
    }

    // Проходим сетку N x M и сохраняем время каждой точки.
    public static HeatmapDataModel RunBenchmarkGrid(int startN, int maxN, int stepN, int startM, int maxM, int stepM)
    {
        var model = new HeatmapDataModel();

        for (var n = startN; n <= maxN; n += stepN)
        for (var m = startM; m <= maxM; m += stepM)
        {
            // Прогрев, чтобы JIT не влиял на первый замер.
            MeasureMultiplyTime(10, 10, 10);

            var time = MeasureMultiplyTime(n, m);
            model.Points.Add(new HeatmapPoint(n, m, time));
        }

        return model;
    }
}