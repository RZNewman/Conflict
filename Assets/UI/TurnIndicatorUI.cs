using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnIndicatorUI : MonoBehaviour
{
    public GameObject yours;
    public GameObject theirs;

	private void Start()
	{
		
	}

	public void setTurn(bool yourTurn)
	{
		yours.SetActive(yourTurn);
		theirs.SetActive(!yourTurn);
	}

}
