using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Status;

public class BuffStatus : Buff
{
    public Effects effects;

	public override string toDesc(bool isPrefab)
	{
        int durr;
        if (isPrefab)
        {
            durr = maxDuration;
        }
        else
        {
            durr = currentDuration;
        }
        string desc = effects.toString();
		if (maxDuration > 0)
		{
			desc += " for " + durr + " round" + (durr > 1 ? "s" : "");
		}
		return desc;
	}
}
