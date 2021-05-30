using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckExport : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    Dictionary<int, int>[] heldDeck;
    public void holdDeck(Dictionary<int, int>[] deck)
	{
        heldDeck = deck;
	}
    public Dictionary<int, int>[] getDeck()
	{
        return heldDeck;
	}
    public void printDeck()
	{
        foreach(KeyValuePair<int, int> i in heldDeck[0])
		{
            Debug.Log(i.Key + " - " + i.Value);
		}
        Debug.Log("Structures");
        foreach (KeyValuePair<int, int> i in heldDeck[0])
        {
            Debug.Log(i.Key + " - " + i.Value);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
