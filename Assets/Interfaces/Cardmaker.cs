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
	public abstract GameObject findCardPrefab();

	public abstract void modifyCardAfterCreation(GameObject o);

	public abstract void register();
}
