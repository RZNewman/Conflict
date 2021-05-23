using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildCard : MonoBehaviour, IPointerClickHandler
{

    Cardmaker mkr;
    int cardmakerIndex;
    DeckbuildingUI controller;

    public void initalize(Cardmaker mk, int i, DeckbuildingUI master)
	{
        cardmakerIndex = i;
        mkr = mk;
        GameObject cardBody = Instantiate(mkr.findCardTemplate(), transform);
        cardBody.GetComponent<CardUI>().populateSelf(mkr, true);
        controller = master;
    }

	public void OnPointerClick(PointerEventData eventData)
	{
        controller.addCard(cardmakerIndex, mkr);
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
