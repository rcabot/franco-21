using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RangeBeginEndAttribute : PropertyAttribute
{
    public float min;
    public float max;

    public RangeBeginEndAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}
