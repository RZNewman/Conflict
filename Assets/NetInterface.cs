using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.UI;

public class NetInterface : MonoBehaviour
{
    public GameObject main;
    public GameObject tutorial;
    public GameObject next;
    public InputField inp;


	private void Start()
	{
        setBetaAddress();
	}
	public void connectToAddress()
	{
        NetworkManager nm = FindObjectOfType<NetworkManager>();
        nm.networkAddress = inp.text;
        Debug.Log(nm.networkAddress);
        nm.StartClient();

    }
    public void setBetaAddress()
    {
        inp.text = ConfReader.Value("serverip");

    }
    
    public void Host()
    {
        NetworkManager nm = FindObjectOfType<NetworkManager>();
        nm.StartHost();

    }
    public void localClient()
    {
        NetworkManager nm = FindObjectOfType<NetworkManager>();
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
    public void endTutorial()
	{
        main.SetActive(true);
        tutorial.SetActive(false);
        next.SetActive(false);
    }
}
