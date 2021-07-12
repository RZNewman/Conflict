using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardCountUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Text costTxt;
    public Text countTxt;
    public Text nameTxt;

    public int cardmakerIndex;
    public int count= 0;
    DeckbuildingUI.deckType type;

    DeckbuildingUI controller;
    Cardmaker mkr;
    // Start is called before the first frame update
    void Start()
    {

    }
    void setCountText()
	{
        countTxt.text = count.ToString();

    }
    public string getOrder()
    {
        return mkr.getOrder();
    }
    public void setCardmaker(Cardmaker c, int index, DeckbuildingUI.deckType t, DeckbuildingUI ct)
	{
        costTxt.text = c.resourceCost.ToString();
        nameTxt.text = c.name;
        cardmakerIndex = index;
        type = t;
        controller = ct;
        mkr = c;
        GetComponent<Image>().color = c.getColor();
	}
    public void incrementCount(int delta)
	{
        //Debug.Log(delta);
        if(count+delta<= GameConstants.maxCardDuplicateLimit)
		{
            count += delta;
            if (count <= 0)
            {
                
                if (inspecting)
                {
                    controller.cardUnInspect(mkr.gameObject);
                    inspecting = false;
                }
                Destroy(gameObject);
                //return true;

            }
            setCountText();
            int trueDelta = delta;
			if (count < 0)
			{
                trueDelta -= count;
			}
            if (type == DeckbuildingUI.deckType.main)
            {
                controller.currentMainDeckSize += trueDelta;
            }
			else
			{
                controller.currentStrcDeckSize += trueDelta;

            }
            controller.deckSizeUpdate();
        }
        
        
        //return false;
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
                    controller.cardInspect(mkr.gameObject,blk? blk.prefabStats(): null );
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

    public void OnPointerClick(PointerEventData eventData)
	{
        incrementCount(-1);
	}
}
