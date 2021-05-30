using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardCountUI : MonoBehaviour, IPointerClickHandler
{
    public Text costTxt;
    public Text countTxt;
    public Text nameTxt;

    public int cardmakerIndex;
    public int count= 0;
    DeckbuildingUI.deckType type;
    // Start is called before the first frame update
    void Start()
    {

    }
    void setCountText()
	{
        countTxt.text = count.ToString();

    }
    public void setCardmaker(Cardmaker c, int index, DeckbuildingUI.deckType t)
	{
        costTxt.text = c.resourceCost.ToString();
        nameTxt.text = c.name;
        cardmakerIndex = index;
        type = t;
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
                DeckbuildingUI.currentMainDeckSize += trueDelta;
            }
			else
			{
                DeckbuildingUI.currentStrcDeckSize += trueDelta;

            }
            
        }
        
        
        //return false;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void OnPointerClick(PointerEventData eventData)
	{
        incrementCount(-1);
	}
}
