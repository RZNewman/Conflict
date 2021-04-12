using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrdSqUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	#region UI part
	public Image cardArt;
    public Text cardCost;
    public GameObject selection;
    void populateArt(Sprite art)
    {
        cardArt.sprite = art;
    }
    void populateCost(string cost)
    {
        cardCost.text = cost;
        cardCost.transform.parent.GetComponent<Image>().color = GameColors.resources;
    }
    public void setSelection(bool isSelected)
    {
        selection.SetActive(isSelected);
    }
    void buildGraphic()
	{
        populateArt(ability.cardArt);
        populateCost(ability.resourceCost.ToString());
        
	}
	#endregion

	#region function part
	GameManager gm;
    public Ordnance ability;
    bool hovered= false;
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
                    gm.clientPlayer.cardInspect(ability.gameObject, CardInspector.inspectType.ability);
                    inspecting = true;

                }

            }
        }
        else
        {

            currentTime =0;

            if (inspecting)
            {
                gm.clientPlayer.cardUnInspect();
                inspecting = false;
            }

        }

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        gm.clientPlayer.abilityClick(this);
    }
	private void Start()
	{
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    public void intialize(Ordnance o)
	{
        ability = o;
        buildGraphic();
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }
    #endregion
}
