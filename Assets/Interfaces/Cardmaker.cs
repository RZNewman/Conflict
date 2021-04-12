using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Cardmaker
{
	public GameObject findCardPrefab();

	public void modifyCardAfterCreation(GameObject o);
}
