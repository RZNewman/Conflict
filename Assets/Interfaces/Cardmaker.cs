using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Cardmaker: NetworkBehaviour 
{
	public int resourceCost;
	public Sprite cardArt;
	public GameObject playEffectPre;


	[SyncVar]
	public string originalName = "";

	public void provideName(string name)
	{
		if (name != "")
		{
			originalName = name;
		}
		else
		{
			if (originalName == "")
			{
				originalName = "MISS";
			}
		}
	}

	public string getOrder()
	{
		return resourceCost.ToString("D2") + getOrderType().ToString("D2") + name;
	}
	protected abstract int getOrderType();
	public abstract GameObject findCardPrefab();
	public abstract GameObject findCardTemplate();

	public abstract Color getColor();

	public abstract void modifyCardAfterCreation(GameObject o);

	public abstract void register();
}
