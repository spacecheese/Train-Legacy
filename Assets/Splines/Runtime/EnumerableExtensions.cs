using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Splines
{
    public static class EnumerableExtensions
    {
        public static Vector3 Average(
            this IEnumerable<Vector3> source)
        {
            return source.Sum() / source.Count();
        }

        public static Vector3 Average<TSource>(
            this IEnumerable<TSource> source, Func<TSource, Vector3> selector)
        {
            return source.Select(selector).Average();
        }

        public static Vector3 Sum(
            this IEnumerable<Vector3> source)
        {
            Vector3 sum = new Vector3();

            foreach (var vec in source)
            {
                sum.x = vec.x + sum.x;
                sum.y = vec.y + sum.y;
                sum.z = vec.z + sum.z;
            }
            return sum;
        }

        public static Vector3 Sum<TSource>(
            this IEnumerable<TSource> source, Func<TSource, Vector3> selector)
        {
            return source.Select(selector).Sum();
        }
    }
}
