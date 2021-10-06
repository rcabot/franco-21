using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

static class CatmullRom
{
    const float CentripetalAlpha = 0.5f;

    public static void SmoothPath(IList<Vector3> base_path, IList<Vector3> smoothed_path, float smooth_distance)
    {
        int total_points = base_path.Count;
        if (Mathf.Approximately(0f, smooth_distance) || smooth_distance < 0f || total_points < 3)
        {
            //Nothing to smooth
            foreach (Vector3 v in base_path)
            {
                smoothed_path.Add(v);
            }
            return;
        }

        //Extrapolate cap control points to allow the full curve to be smoothed
        Vector3 start_cap = ExtrapolatePoint(base_path[0], base_path[1]);
        Vector3 end_cap = ExtrapolatePoint(base_path[total_points - 1], base_path[total_points - 2]);

        //Start segment
        SmoothSegment(start_cap, base_path[0], base_path[1], base_path[2], smoothed_path, smooth_distance);

        //Main Body
        int last_control_point = total_points - 3;
        for (int i = 0; i < last_control_point; ++i)
        {
            SmoothSegment(base_path[i], base_path[i + 1], base_path[i + 2], base_path[i + 3], smoothed_path, smooth_distance);
        }

        //End Segment
        SmoothSegment(base_path[total_points - 3], base_path[total_points - 2], base_path[total_points - 1], end_cap, smoothed_path, smooth_distance);

        //Find waypoint
        smoothed_path.Add(base_path[total_points - 1]);
    }

    private static Vector3 ExtrapolatePoint(Vector3 from, Vector3 to)
    {
        return from + (from - to).normalized;
    }

    private static void SmoothSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, IList<Vector3> smoothed_path, float smooth_distance)
    {
        //Add the start
        smoothed_path.Add(p1);

        float distance12 = (p2 - p1).magnitude;
        int waypoints_to_add = Mathf.FloorToInt(distance12 / smooth_distance);

        if (waypoints_to_add > 0)
        {
            //Catmull-Rom spline equations... See wikipedia
            //https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline

            float t0 = 0f;
            float t1 = GetT(t0, p0, p1);
            float t2 = GetT(t1, p1, p2);
            float t3 = GetT(t2, p2, p3);

            float point_t = (t2 - t1) / waypoints_to_add;
            for (int i = 1; i <= waypoints_to_add; ++i) // 1 <= to ensure t is always at least point_t
            {
                float t = t1 + point_t * i;
                Vector3 a1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
                Vector3 a2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
                Vector3 a3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;
                Vector3 b1 = (t2 - t) / (t2 - t0) * a1 + (t - t0) / (t2 - t0) * a2;
                Vector3 b2 = (t3 - t) / (t3 - t1) * a2 + (t - t1) / (t3 - t1) * a3;

                smoothed_path.Add((t2 - t) / (t2 - t1) * b1 + (t - t1) / (t2 - t1) * b2);
            }
        }
    }

    private static float GetT(float t, Vector3 p0, Vector3 p1)
    {
        float a = Mathf.Pow((p1.x - p0.x), 2.0f) + Mathf.Pow((p1.y - p0.y), 2.0f) + Mathf.Pow((p1.z - p0.z), 2.0f);
        float b = Mathf.Pow(a, CentripetalAlpha * 0.5f);

        return (b + t);
    }
}
