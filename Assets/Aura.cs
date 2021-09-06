using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : Buff
{
    public GameObject buffGiven;

    List<Tile> affectedArea = new List<Tile>();
	Tile center;

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



}
