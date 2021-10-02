using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public static class RandomUtility
{
    public static T WeightedRandomInterval<T>(IList<T> items, Func<T, float> weight_accessor)
    {
        float total_weight = items.Aggregate(0f, (accum, item) => accum + weight_accessor(item));
        float roll_weight = Random.Range(0f, Mathf.Max(total_weight, 0f));

        float accumulated_weight = 0f;
        foreach (T item in items)
        {
            accumulated_weight += weight_accessor(item);
            if (accumulated_weight >= roll_weight)
            {
                return item;
            }
        }

        //Only possible if weights total zero
        return items[0];
    }
}

public static class MathUtil
{
    public static bool Approximately(Vector2 lhs, Vector2 rhs)
    {
        return Mathf.Approximately(lhs.x, rhs.x) && Mathf.Approximately(lhs.y, rhs.y);
    }
}

public static class SpatialUtil
{
    public static bool LineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        Vector2 temp;
        return LineIntersection(a1, a2, b1, b2, out temp);
    }

    public static bool LineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        var d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
        var v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }

        intersection.x = a1.x + u * (a2.x - a1.x);
        intersection.y = a1.y + u * (a2.y - a1.y);

        return true;
    }

    public static bool IsPointInsidePolygon(IList<Vector2> polygon, Vector2 point)
    {
        if (polygon.Count < 3)
            return false;

        Vector2 origin = polygon.Aggregate(Vector2.zero, (accum, p) => accum + p) / polygon.Count + Vector2.one * 10000f;


        int intersection_count = 0;
        for (int i = 0, end = polygon.Count; i < end; ++i)
        {
            int next_point = (i + 1) % end;
            if (LineIntersection(origin, point, polygon[i], polygon[next_point]))
                ++intersection_count;
        }

        //We're inside the polygon if we have an odd number of intersections
        return intersection_count % 2 == 1;
    }

    public static bool PolygonIntersection(IList<Vector2> polygon, Vector2 line_start, Vector2 line_end)
    {
        for (int i = 0, end = polygon.Count; i < end; ++i)
        {
            int next_point = (i + 1) % end;
            if (LineIntersection(line_start, line_end, polygon[i], polygon[next_point]))
            {
                return true;
            }
        }

        return false;
    }

    public static bool PolygonIntersection(IList<Vector2> polygon, Vector2 line_start, Vector2 line_end, out Vector2 result)
    {
        result = Vector2.zero;

        float best_sqr_distnace = Mathf.Infinity;
        bool has_intersection = false;
        for (int i = 0, end = polygon.Count; i < end; ++i)
        {
            int next_point = (i + 1) % end;
            Vector2 intersection;
            if (LineIntersection(line_start, line_end, polygon[i], polygon[next_point], out intersection))
            {
                has_intersection = true;
                float sqr_distance = (intersection - line_start).sqrMagnitude;
                if (sqr_distance < best_sqr_distnace)
                {
                    best_sqr_distnace = sqr_distance;
                    result = intersection;
                }
            }
        }

        return has_intersection;
    }

    public static bool IsPointInsideEllipse(Vector2 center, float width, float height, Vector2 point)
    {
        float delta_x = (point.x - center.x);
        float delta_y = (point.y - center.y);

        float dx2 = delta_x * delta_x;
        float dy2 = delta_y * delta_y;
        float w2 = width * width;
        float h2 = height * height;

        return (dx2 / w2) + (dy2 / h2) <= 1f;
    }
}
