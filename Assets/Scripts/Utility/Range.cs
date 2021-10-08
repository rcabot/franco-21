using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

[Serializable]
public struct RangeFloat
{
    public float start;
    public float length;

    public RangeFloat(float start, float length)
    {
        this.start = start;
        this.length = length;
    }

    public float end => start + length;
    public float RandomValue => length > 0 ? UnityRandom.Range(start, end) : UnityRandom.Range(end, start);

    public bool  InRange(float val)
    {
        return length > 0 ? 
              val >= start && val <= end
            : val <= start && val >= end;
    }

    public bool InRangeSqrd(float sqr_val)
    {
        float sqr_start = start * start;
        float sqr_end = end * end;

        return length > 0 ?
              sqr_val >= sqr_start && sqr_val <= sqr_end
            : sqr_val <= start && sqr_val >= sqr_end;
    }
    
    public float SampleNormalised(float normal)
    {
        return start + length * normal;
    }

    public float Clamp(float val)
    {
        return length > 0 ?
              Mathf.Clamp(val, start, end)
            : Mathf.Clamp(val, end, start);
    }
}
