
using UnityEngine;

[System.Serializable]
public class Interval
{
    // inclusive start, exclusive end
    public int start, end;

    // interval overlapping at max
    private int max = 12;

    public Interval(int start, int end, int max)
    {
        this.start = start % max;
        this.end = end % max;
        this.max = max;
    }

    public bool Contains(int index)
    {
        if (max == 0) max = 1;

        int nEnd = (end-1 - start) % max;
        int nIndex = (index - start) % max;

        return nIndex <= nEnd;
    }
}
[System.Serializable]
public class IntegerInterval : Interval
{
    public int value;

    public IntegerInterval(int value, int start, int end, int max) : base(start,end,max)
    {
        this.value = value;
    }
}
