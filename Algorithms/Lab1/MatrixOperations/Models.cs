namespace Algorithms.Lab1.MatrixOperations;


    public record HeatmapPoint(int N, int M, double TimeMs);

    public class HeatmapDataModel
    {
        public List<HeatmapPoint> Points { get; set; } = new();
    }