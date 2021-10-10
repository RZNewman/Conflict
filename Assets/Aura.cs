using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : Buff
{
    public GameObject buffGiven;

    List<Tile> affectedArea = new List<Tile>();
	Tile center;


	[ClientRpc]
	public override void RpcAssignParent(uint parentID)
	{
		
		GameObject u = NetworkIdentity.spawned[parentID].gameObject;
		//Debug.Log("assingd");
		transform.parent = u.transform;
		transform.localPosition = Vector3.zero;
		visuals = Instantiate(visualsPre, u.transform);

		if (isServer)
		{
			return;
		}
		Unit un = u.GetComponent<Unit>();
		if (un)
		{
			un.aurasEmitted.Add(this);
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

	public void register()
	{
		if (!NetworkClient.prefabs.ContainsValue(gameObject))
		{
			NetworkClient.RegisterPrefab(gameObject);

		}
		if (!NetworkClient.prefabs.ContainsValue(buffGiven))
		{
			NetworkClient.RegisterPrefab(buffGiven);

		}
	}
	
	public override string toDesc()
	{
		string desc;

		desc = string.Format("grant {0}",
			buffGiven.GetComponent<Buff>().toDesc()
			);
		desc += " " + buffGiven.GetComponent<Targeting>().targetingDesc(false, true);
		if (maxDuration > 0)
		{
			desc += " for " + maxDuration + " round" + (maxDuration > 1 ? "s" : "");
		}


		return desc;
	}
	public override void PDestroy(bool isSev)
	{
		if (isSev)
		{
			removeBuff(this);
			foreach (GameObject o in abilities)
			{
				gm.delayedDestroy(o);
			}
			removeFromTiles();
		}
		else
		{
			if (visuals)
			{
				Destroy(visuals);
			}
		}

	}



}
