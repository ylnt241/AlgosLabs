using System;
using System.Collections.Generic;
using System.Linq;
using Algorithms.Interfaces;

// Укажите ваш namespace для IAlgorithm

namespace AlgosLabs.Services;

public class AlgorithmScanner
{
    public List<IAlgorithm> FindAllAlgorithms()
    {
        var instances = new List<IAlgorithm>();

        // Ищем типы и убираем дубликаты по полному имени класса
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