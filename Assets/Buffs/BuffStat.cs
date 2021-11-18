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


		string desc = GetComponent<StatHandler>().toDesc(isPrefab,false);


		desc += descSuffix(isPrefab);
		return desc;

	}
	public void setPrefabStat(StatType t, int val)
	{
		StatHandler st = GetComponent<StatHandler>();
		st.setPrefabStat(t, val);
	}
}
