using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckUI : MonoBehaviour, IPointerClickHandler
{
    string deckName;
    public Text nameVis;
    public GameObject selector;

    DeckbuildingUI controller;
    public void assignName(string nameGiven, DeckbuildingUI ct)
	{
        deckName = nameGiven;
        nameVis.text = nameGiven;
        controller = ct;

    }

    public void select(bool s )
    {
        selector.SetActive(s);
    }


	public void OnPointerClick(PointerEventData eventData)
	{
        
        controller.selectDeck(deckName);
    }
}
