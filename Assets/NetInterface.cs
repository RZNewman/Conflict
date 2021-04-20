using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class NetInterface : MonoBehaviour
{
    public GameObject main;
    public GameObject tutorial;
    public GameObject next;

    public void connectToBetaAddress()
	{
        NetworkManager nm = FindObjectOfType<NetworkManager>();
        nm.networkAddress = "24.148.76.143";
        nm.StartClient();
    }
    public void startTutorial()
	{
        foreach(NetworkIdentity id in FindObjectsOfType<NetworkIdentity>(true))
		{
            //Debug.Log(id.gameObject);
            id.gameObject.SetActive(true);
		}
        main.SetActive(false);
        tutorial.SetActive(true);
        next.SetActive(true);
	}
}
