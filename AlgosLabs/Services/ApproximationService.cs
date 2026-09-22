using System;
using System.Collections.Generic;
using Algorithms.Services;

namespace AlgosLabs.Services;

// Типы теоретической сложности
public enum ComplexityType
{
    Constant, // O(1)
    Linear, // O(N)
    Logarithmic, // O(log N)
    Linearithmic, // O(N log N)
    Quadratic, // O(N^2)
    Cubic // O(N^3)
}

public record ApproximationResult(
    ComplexityType Complexity,
    double C,
    double Mse,
    List<(double X, double Y)> TheoreticalPoints
);

public class ApproximationService
{
    /// <summary>
    ///     Функция f(N) в зависимости от выбранного типа сложности
    /// </summary>
    public static double EvaluateComplexityFunction(double n, ComplexityType type)
    {
        return type switch
        {
            ComplexityType.Constant => 1.0,
            ComplexityType.Linear => n,
            ComplexityType.Logarithmic => Math.Log2(n <= 0 ? 1 : n),
            ComplexityType.Linearithmic => n * Math.Log2(n <= 0 ? 1 : n),
            ComplexityType.Quadratic => n * n,
            ComplexityType.Cubic => n * n * n,
            _ => n
        };
    }

    /// <summary>
    ///     Расчет коэффициента C и MSE: MSE = (1/k) * Sum( (T_emp - C * f(N))^2 )
    /// </summary>
    public ApproximationResult Fit(List<BenchmarkResultPoint> points, ComplexityType complexity)
    {
        var k = points.Count;
        if (k == 0) return new ApproximationResult(complexity, 0, 0, new List<(double X, double Y)>());

        double sumNumerator = 0;
        double sumDenominator = 0;

        foreach (var p in points)
        {
            var fn = EvaluateComplexityFunction(p.N, complexity);
            sumNumerator += p.AverageTimeMs * fn;
            sumDenominator += fn * fn;
        }

        // Коэффициент C методом наименьших квадратов
        var c = sumDenominator != 0 ? sumNumerator / sumDenominator : 0;

        // Вычисление MSE и формирование точек кривой
        double sumSquareError = 0;
        var theoreticalPoints = new List<(double X, double Y)>();

        foreach (var p in points)
        {
            var fn = EvaluateComplexityFunction(p.N, complexity);
            var tApprox = c * fn;

            var error = p.AverageTimeMs - tApprox;
            sumSquareError += error * error;

            theoreticalPoints.Add((p.N, tApprox));
        }

        var mse = sumSquareError / k;

        return new ApproximationResult(complexity, c, mse, theoreticalPoints);
    }
}