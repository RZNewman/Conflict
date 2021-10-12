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

	public override string toDesc()
	{
		string desc = CardUI.cardText(GetComponent<StatHandler>().prefabStats(), false).Replace('\n', ',');
		desc = "'" + desc + "'";
		if (maxDuration > 0)
		{
			desc += " for " + maxDuration + " round" + (maxDuration > 1 ? "s" : "");
		}
		return desc;

	}
	public void setPrefabStat(StatType t, int val)
	{
		StatHandler st = GetComponent<StatHandler>();
		st.setPrefabStat(t, val);
	}
}
