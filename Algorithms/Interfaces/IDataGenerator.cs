namespace Algorithms;

public interface IDataGenerator<TData>
{
    int MaxN { get; }
    TData Generate(int n);
}