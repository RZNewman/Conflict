using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Cardmaker: NetworkBehaviour 
{
	public int resourceCost;
	public Sprite cardArt;
	public GameObject playEffectPre;
	public abstract GameObject findCardPrefab();

	public abstract void modifyCardAfterCreation(GameObject o);
}
