using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DIReflection.Runtime
{
  internal static class HelperExtensions
  {
    public static IEnumerable<TSource> TopologicalSort<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> dependencySelector)
    {
      List<TSource> sourceCollection = new List<TSource>(source);
      int vertexCount = sourceCollection.Count();
      List<int>[] graph = new List<int>[vertexCount];
      bool[] visited = new bool[vertexCount];
      List<TSource> result = new List<TSource>(vertexCount);

      int vertex = 0;
      foreach (TSource element in sourceCollection)
      {
        graph[vertex] = dependencySelector(element)
          .Select(node => sourceCollection.IndexOf(node))
          .ToList();
        vertex++;
      }

      for (int i = 0; i < vertexCount; i++)
        if (!visited[i])
          DFS(i);

      return result;

      void DFS(int v)
      {
        visited[v] = true;
        int related = graph[v].Count();
        for (int i = 0; i < related; i++)
        {
          int next = graph[v][i];
          if (!visited[next])
            DFS(next);
        }

        result.Add(sourceCollection[v]);
      }
    }

    public static IEnumerable<TSource> PartialOrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
    {
      List<TSource> list = new List<TSource>(source);
      while (list.Count > 0)
      {
        TSource minimum = default(TSource);
        TKey minimumKey = default(TKey);
        foreach (TSource s in list)
        {
          TKey k = keySelector(s);
          minimum = s;
          minimumKey = k;
          break;
        }

        foreach (TSource s in list)
        {
          TKey k = keySelector(s);
          if (comparer.Compare(k, minimumKey) < 0)
          {
            minimum = s;
            minimumKey = k;
          }
        }

        yield return minimum;
        list.Remove(minimum);
      }
    }

    public static List<T> ToListImplicit<T>(this ICollection collection) where T : class
    {
      var list = new List<T>();
      foreach (object element in collection) list.Add(element as T);

      return list;
    }
  }
}