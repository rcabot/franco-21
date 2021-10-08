using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static T PopBack<T>(this IList<T> list)
    {
        int last = list.Count - 1;
        T val = list[last];
        list.RemoveAt(last);
        return val;
    }

    public static T PopFront<T>(this IList<T> list)
    {
        T val = list[0];
        list.RemoveAt(0);
        return val;
    }

    public static void QuickRemove<T>(this IList<T> list, T value)
    {
        int index = list.IndexOf(value);
        if (index >= 0)
        {
            QuickRemoveAt(list, index);
        }
    }

    public static void QuickRemoveAt<T>(this IList<T> list, int index)
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

    public static T Front<T>(this List<T> list)
    {
        return list[0];
    }

    public static T Back<T>(this List<T> list)
    {
        return list[list.Count - 1];
    }

    public static bool Empty<T>(this List<T> list)
    {
        return list.Count == 0;
    }

    //Make sure your list is sorted before you call this
    //Returns the index of the first item in the list that is NOT less than the given item
    public static int LowerBound<T, V>(this IList<T> list, V value) where V : IComparable<T>
    {
        return list.LowerBound(value, (T a, V b) => (b.CompareTo(a) > 0));
    }

    //Make sure your list is sorted before you call this
    //Returns the index of the first item in the list that is NOT less than the given item
    public static int LowerBound<T, V>(this IList<T> list, V value, Func<T, V, bool> less_than_compare)
    {
        int start = 0;
        int item_count = list.Count;
        int iter;
        int step;

        while (item_count > 0)
        {
            iter = start;
            step = item_count / 2;
            iter += step;
            if (less_than_compare(list[iter], value))
            {
                start = ++iter;
                item_count -= step + 1;
            }
            else
            {
                item_count = step;
            }
        }

        return start;
    }

    //Make sure your list is sorted before you call this
    //Returns the index of the first item i nthe list that is greater than the given item
    public static int UpperBound<T, V>(this IList<T> list, V value) where V : IComparable<T>
    {
        return list.UpperBound(value, (V a, T b) => a.CompareTo(b) < 0);
    }

    //Make sure your list is sorted before you call this
    //Returns the index of the first item i nthe list that is greater than the given item
    public static int UpperBound<T, V>(this IList<T> list, V value, Func<V, T, bool> less_than_compare)
    {
        int start = 0;
        int iter;
        int item_count = list.Count;
        int step;

        while (item_count > 0)
        {
            iter = start;
            step = item_count / 2;
            iter += step;
            if (!less_than_compare(value, list[iter]))
            {
                start = ++iter;
                item_count -= step + 1;
            }
            else
            {
                item_count = step;
            }
        }

        return start;
    }

    public static int IndexOf<T>(this IReadOnlyList<T> list, T value) where T : class
    {
        for (int i = 0, end = list.Count - 1; i < end; ++i)
        {
            if (list[i] == value)
                return i;
        }
        return -1;
    }
}

public static class ListHeapExtensions
{
    //Organise a list into a binary heap
    public static void MakeHeap<T>(this IList<T> list, Func<T, T, bool> less_than_compare)
    {
        SortHeap(list, 0, less_than_compare);
    }

    public static T HeapPop<T>(this IList<T> list, Func<T, T, bool> less_than_compare)
    {
        T top = list[0];
        list.QuickRemoveAt(0);
        SortHeap(list, 0, less_than_compare);
        return top;
    }

    public static void HeapPush<T>(this IList<T> list, T val, Func<T, T, bool> less_than_compare)
    {
        list.Add(val);

        int i = list.Count - 1;
        while (i != 0)
        {
            int parent_index = HeapParent(i);
            if (less_than_compare(list[i], list[parent_index]))
            {
                T temp = list[i];
                list[i] = list[parent_index];
                list[parent_index] = temp;
                i = parent_index;
            }
            else
            {
                break;
            }
        }
    }

    private static void SortHeap<T>(IList<T> list, int heap_index, Func<T, T, bool> less_than_compare)
    {
        int heap_size = list.Count;
        int left = HeapLeft(heap_index);
        int right = HeapRight(heap_index);

        int smallest = heap_index;
        if (left < heap_size && less_than_compare(list[left], list[smallest]))
        {
            smallest = left;
        }

        if (right < heap_size && less_than_compare(list[right], list[smallest]))
        {
            smallest = right;
        }

        if (smallest != heap_index)
        {
            T temp = list[heap_index];
            list[heap_index] = list[smallest];
            list[smallest] = temp;
            SortHeap(list, smallest, less_than_compare);
        }
    }

    private static int HeapParent(int heap_index)
    {
        return (heap_index - 1) / 2;
    }

    private static int HeapLeft(int heap_index)
    {
        return 2 * heap_index + 1;
    }

    private static int HeapRight(int heap_index)
    {
        return 2 * heap_index + 2;
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
        result.y = v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians);

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

    public static Vector2 XZ(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
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

    public static Vector4 ToVector4(this Vector3 v, float w)
    {
        return new Vector4(v.x, v.y, v.z, w);
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

public static class GeneralExtensions
{
    public static void Swap<T>(ref this T lhs, ref T rhs) where T : struct
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    public static void Swap<T>(this T lhs, T rhs)
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
}
