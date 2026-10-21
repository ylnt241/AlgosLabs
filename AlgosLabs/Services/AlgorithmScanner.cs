using System;
using System.Collections.Generic;
using System.Linq;
using Algorithms.Interfaces;

namespace AlgosLabs.Services;

public class AlgorithmScanner
{
    public List<IAlgorithm> FindAllAlgorithms()
    {
        var instances = new List<IAlgorithm>();

        // Собираем реализации из всех загруженных сборок; DistinctBy — от повторной регистрации.
        var algorithmTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => typeof(IAlgorithm).IsAssignableFrom(t) ||
                        t.GetInterfaces().Any(i =>
                            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAlgorithm<>)))
            .DistinctBy(t => t.FullName);

        foreach (var type in algorithmTypes)
            if (Activator.CreateInstance(type) is IAlgorithm algo)
                instances.Add(algo);

        return instances;
    }
}