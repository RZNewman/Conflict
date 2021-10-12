using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Status;

public class BuffStatus : Buff
{
    public Effects effects;

	public override string toDesc()
	{
		string desc = effects.toString();
		if (maxDuration > 0)
		{
			desc += " for " + maxDuration + " round" + (maxDuration > 1 ? "s" : "");
		}
		return desc;
	}
}
