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
	public override void RpcAssignUnit(uint unitID)
	{
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
	
	public Buff tryEnterAura(Tile t, Unit u)
	{
		if (buffGiven.GetComponent<Targeting>().evaluate(t, u.teamIndex, center)){
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
	
	public string toDesc(bool isPrefab)
	{
		string desc;
		if (isPrefab)
		{
			desc = string.Format("grant '{0}'",
				CardUI.cardText(buffGiven.GetComponent<StatHandler>().prefabStats(), isPrefab,false).Replace('\n', ',')
				);
		}
		else
		{
			desc = string.Format("grant '{0}'",
				CardUI.cardText(buffGiven.GetComponent<StatHandler>().export(), isPrefab, false).Replace('\n', ',')
				);
		}

		

		return desc + " " + buffGiven.GetComponent<Targeting>().targetingDesc(false, true);
	}



}
