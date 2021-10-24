using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatBlock;

public class BuffStat : Buff
{
	public override void initailize(Unit u = null)
	{
		base.initailize(u);
		StatHandler bStats = GetComponent<StatHandler>();
		bStats.initialize();
	}

	public override string toDesc(bool isPrefab)
	{
		Dictionary<StatType, float> sts;
		int durr;
		if (isPrefab)
		{
			sts = GetComponent<StatHandler>().prefabStats();
			durr = maxDuration;
		}
		else
		{
			sts = GetComponent<StatHandler>().export();
			durr = currentDuration;
		}
		string desc = CardUI.cardText(sts, Status.getDefault(), false);
		
		if (maxDuration > 0)
		{
			desc += " for " + durr + " round" + (durr > 1 ? "s" : "");
		}
		return desc;

	}
	public void setPrefabStat(StatType t, int val)
	{
		StatHandler st = GetComponent<StatHandler>();
		st.setPrefabStat(t, val);
	}
}
