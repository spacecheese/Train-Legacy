using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splines
{
    static internal class TerrainUtils
    {
        public static Vector2 WorldToTerrainSpace(Terrain terrain, Vector2 vect)
        {
            throw new NotImplementedException();
        }

        public static Rect WorldToTerrainSpace(Terrain terrain, Rect rect)
        {
            Vector2 min = WorldToTerrainSpace(terrain, rect.min);
            Vector2 max = WorldToTerrainSpace(terrain, rect.max);
            Vector2 size = max - min;

            return new Rect(min.x, min.y, size.x, size.y);
        }

        public static Rect CombineRects(IEnumerable<Rect> rects)
        {
            var min = new Vector2(int.MaxValue, int.MaxValue);
            var max = new Vector2(int.MinValue, int.MinValue);

            foreach (var rect in rects)
            {
                min.Set(Math.Min(min.x, rect.x), Math.Min(min.y, rect.y));
                max.Set(Math.Max(min.x, rect.x), Math.Max(min.y, rect.y));
            }

            return new Rect(min, max - min);
        }

        public static Rect BoundsToRect(Bounds bounds)
        {
            return new Rect(
                bounds.min.x, bounds.min.y,
                bounds.size.x, bounds.size.y);
        }
    }
}
