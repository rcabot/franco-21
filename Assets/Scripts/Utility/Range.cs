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

    public float   end => start + length;
    public bool  InRange(float val) { return length > 0 ? val >= start && val <= end : val <= start && val >= end; }
    public float Clamp(float val) { return length > 0 ? Mathf.Clamp(val, start, end) : Mathf.Clamp(val, end, start); }
    public float RandomValue => length > 0 ? UnityRandom.Range(start, end) : UnityRandom.Range(end, start);
}
