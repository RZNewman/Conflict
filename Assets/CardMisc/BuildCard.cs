using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    Cardmaker mkr;
    int cardmakerIndex;
    DeckbuildingUI.deckType type;
    DeckbuildingUI controller;
    GameObject cardBody;

    public void initalize(Cardmaker mk, int i, DeckbuildingUI master, DeckbuildingUI.deckType t)
	{
        cardmakerIndex = i;
        mkr = mk;
        cardBody = Instantiate(mkr.findCardTemplate(), transform);
        cardBody.GetComponent<CardUI>().populateSelf(mkr, true);
        controller = master;
        type = t;
    }

	public void OnPointerClick(PointerEventData eventData)
	{
        controller.addCard(cardmakerIndex, mkr, type);
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    bool hovered = false;
    bool inspecting = false;
    float currentTime = 0;
    void Update()
    {
        //Debug.Log(hovered);
        if (hovered)
        {
            currentTime += Time.deltaTime;

            if (currentTime > GameConstants.hoverInspectTime)
            {
                currentTime = GameConstants.hoverInspectTime;
                if (!inspecting)
                {
                    StatHandler blk = mkr.GetComponent<StatHandler>();
                    controller.cardInspect(mkr.gameObject, blk ? blk.prefabStats() : null);
                    inspecting = true;

                }

            }
        }
        else
        {

            currentTime = 0;

            if (inspecting)
            {
                controller.cardUnInspect(mkr.gameObject);
                inspecting = false;
            }

        }

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }
}
