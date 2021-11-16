using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : Buff
{
    public GameObject buffGiven;

    List<Tile> affectedArea = new List<Tile>();
	Tile center;


	protected override void CallbackRPC(GameObject u)
	{
		if (isClientOnly)
		{
			Unit un = u.GetComponent<Unit>();
			if (un)
			{
				un.aurasEmitted.Add(this);
			}
		}
	}
	[Server]
	public void updateLocation(Tile newCenter)
	{
		center = newCenter;
        List<Tile> newArea = buffGiven.GetComponent<Targeting>().getAuraArea(center);
		List<Tile> newToAura = new List<Tile>(newArea);
        foreach(Tile t in affectedArea)
		{
			if (!newArea.Contains(t))
			{
				t.removeAura(this);
			}
			else
			{
				newToAura.Remove(t);
			}
		}
		foreach (Tile t in newToAura)
		{
			t.addAura(this);
		}
		affectedArea = newArea;
	}
	[Server]
	void removeFromTiles()
	{
		
		foreach (Tile t in affectedArea)
		{
			t.removeAura(this);
		}
	}
	[Server]
	public void bindTile(Tile t)
	{
		t.addAura(this);
	}
	
	public Buff tryEnterAura(Tile t, Unit u)
	{
		if (buffGiven.GetComponent<Targeting>().evaluate(t, teamInd, center)){
			GameObject buff = Instantiate(buffGiven);
			Buff b = buff.GetComponent<Buff>();
			buff.transform.parent = u.transform;
			buff.transform.localPosition = Vector3.zero;
			b.setTeam(teamInd);
			u.addBuff(b);
			NetworkServer.Spawn(buff);
			b.RpcAssignParent(u.netId);
			return b;
		}
		return null;
	}

	public override void register()
	{
		base.register();
		if (!NetworkClient.prefabs.ContainsValue(buffGiven))
		{
			NetworkClient.RegisterPrefab(buffGiven);

		}
	}
	
	public override string toDesc(bool isPrefab)
	{
		string desc;
		int durr;
		if (isPrefab)
		{
			durr = maxDuration;
		}
		else
		{
			durr = currentDuration;
		}
		desc = string.Format("aura: {0}",
			buffGiven.GetComponent<Buff>().toDesc(true)
			);
		desc += " " + buffGiven.GetComponent<Targeting>().targetingDesc(false, true);
		if (maxDuration > 0)
		{
			desc += " for " + durr + " round" + (durr > 1 ? "s" : "");
		}


		return desc;
	}

	protected override void CallbackPD()
	{
		removeFromTiles();
	}



}
