using System.Collections;
using UnityEngine;

[System.Serializable]
public struct SwipeCounterData
{
    private int swipeCounter;
    private int limit;

    [Tooltip("Until this limit, gravity shall affect the snapped drill bit")]
    public int gravityOnThreshold;

    public SwipeCounterData(int swipeCounter, int limit)
    {
        this.swipeCounter = swipeCounter;
        this.limit = limit;
        this.gravityOnThreshold = limit / 2;
    }

    public void IncreaseCounter()
    {
        if (swipeCounter < limit)
        {
            swipeCounter++;
        }
    }

    public void DecreaseCounter()
    {
        if (swipeCounter > -limit)
        {
            swipeCounter--;
        }
    }

    public int GetCounter()
    {
        return swipeCounter;
    }

    public int GetGravityOnThreshold()
    {
        return gravityOnThreshold;
    }

    public int GetUpperLimit()
    {
        return limit;
    }

    public int GetLowerLimit()
    {
        return -limit;
    }
}
