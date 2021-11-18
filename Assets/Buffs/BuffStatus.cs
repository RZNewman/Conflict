using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Status;

public class BuffStatus : Buff
{
    public Effects effects;

	public override string toDesc(bool isPrefab)
	{

        string desc = effects.toString();
		desc += descSuffix(isPrefab);
		return desc;
	}
}
