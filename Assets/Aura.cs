using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : Buff
{
    public GameObject buffGiven;

    List<Tile> affectedArea = new List<Tile>();
	Tile center;
	[SyncVar]
	public int teamInd=-1;

	[ClientRpc]
	public override void RpcAssignUnit(uint unitID)
	{
		if (isServer)
		{
			return;
		}
		GameObject u = NetworkIdentity.spawned[unitID].gameObject;
		//Debug.Log("assingd");
		transform.parent = u.transform;
		transform.localPosition = Vector3.zero;


		Unit un = u.GetComponent<Unit>();
		un.aurasEmitted.Add(this);
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
			u.addBuff(b);
			NetworkServer.Spawn(buff);
			b.RpcAssignUnit(u.netId);
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
	
	public string toDesc()
	{
		string desc;

		desc = string.Format("grant '{0}'",
			CardUI.cardText(buffGiven.GetComponent<StatHandler>().prefabStats(),false).Replace('\n', ',')
			);

		

		return desc + " " + buffGiven.GetComponent<Targeting>().targetingDesc(false, true);
	}



}
