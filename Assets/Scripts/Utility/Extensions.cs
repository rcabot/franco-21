using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static T PopBack<T>(this List<T> list)
    {
        int last = list.Count - 1;
        T val = list[last];
        list.RemoveAt(last);
        return val;
    }

    public static void QuickRemove<T>(this List<T> list, T value)
    {
        int index = list.IndexOf(value);
        if (index >= 0)
        {
            QuickRemoveAt(list, index);
        }
    }

    public static void QuickRemoveAt<T>(this List<T> list, int index)
    {
        int last = list.Count - 1;
        if (last != index)
        {
            list[index] = list[last];
        }

        list.RemoveAt(last);
    }

    public static void Resize<T>(this List<T> list, int new_size, T default_value = default(T))
    {
        new_size = Mathf.Max(0, new_size);
        if (list.Count > new_size)
        {
            int to_remove = list.Count - new_size;
            list.RemoveRange(list.Count - to_remove, to_remove);
        }
        else
        {
            while (list.Count < new_size)
            {
                list.Add(default_value);
            }
        }
    }

    public static void Resize<T>(this List<T> list, int new_size) where T : ScriptableObject
    {
        new_size = Mathf.Max(0, new_size);
        if (list.Count > new_size)
        {
            int to_remove = list.Count - new_size;
            list.RemoveRange(list.Count - to_remove, to_remove);
        }
        else
        {
            while (list.Count < new_size)
            {
                list.Add(ScriptableObject.CreateInstance<T>());
            }
        }
    }

    public static T Front<T>(this List<T> list) where T : class
    {
        return list.Count > 0 ? list[0] : null;
    }

    public static T Back<T>(this List<T> list) where T : class
    {
        return list.Count > 0 ? list[list.Count - 1] : null;
    }
}

public static class ArrayExtensions
{
    public static T[] Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = value;
        }
        return array;
    }
}

public static class Vector2Extensions
{
    public static Vector2 RotateCw(this Vector2 v, float radians)
    {
        Vector2 result = v;
        result.x = v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians);
        result.y = v.y * Mathf.Sin(radians) + v.y * Mathf.Cos(radians);

        return result;
    }

    public static Vector3 ToVector3(this Vector2 v)
    {
        return new Vector3(v.x, v.y);
    }

    public static Vector3 XZ(this Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static bool Approximately(this Vector2 lhs, Vector2 rhs)
    {
        return Mathf.Approximately(lhs.x, rhs.x) && Mathf.Approximately(lhs.y, rhs.y);
    }

    public static Vector2Int Floor(this Vector2 v)
    {
        return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
    }

    public static Vector2Int Ceil(this Vector2 v)
    {
        return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
    }
}

public static class Vector3Extensions
{
    public static Vector2 XY(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector3 Translated(this Vector3 v, float x, float y, float z)
    {
        return new Vector3(v.x + x, v.y + y, v.z + z);
    }

    public static Vector3 Translated(this Vector3 v, float x, float y)
    {
        return v.Translated(x, y, 0f);
    }

    public static Vector3 Translated(this Vector3 v, Vector2 xy)
    {
        return v.Translated(xy.x, xy.y, 0f);
    }

    public static bool Approximately(this Vector3 lhs, Vector3 rhs)
    {
        return Mathf.Approximately(lhs.x, rhs.x)
            && Mathf.Approximately(lhs.y, rhs.y)
            && Mathf.Approximately(lhs.z, rhs.z);
    }
}

public static class TransformExtensions
{
    public static Vector2 position2D(this Transform t)
    {
        return t.position.XY();
    }
}

public static class RectIntExtensions
{
    public static RectInt Clamp(this RectInt r, Vector2Int min, Vector2Int max)
    {
        Vector2Int rmin = r.min;
        rmin.Clamp(min, max);
        r.min = rmin;

        Vector2Int rmax = r.max;
        rmax.Clamp(min, max);
        r.max = rmax;

        return r;
    }
}

public static class Texture2DExtensions
{
    public static void Fill(this Texture2D t, Color c)
    {
        Color[] pixels = t.GetPixels();
        pixels.Fill(c);
        t.SetPixels(pixels);
    }
}

public static class ComponentExtensions
{
    public static T RequireComponent<T>(this Component o) where T : Component
    {
        T result = o.GetComponent<T>();
        Debug.Assert(result);
        return result;
    }
}
