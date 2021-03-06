using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class Card : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, TeamOwnership
{
    protected GameObject cardTemplatePre;
    protected GameObject cardBody;

    public GameObject sourceCardmaker;
    [SyncVar]
    protected string sourceCardmakerPath;

    [SyncVar]
    public int team;

    protected GameManager gm;
    [SyncVar]
    public string targetHand = "PlayerHand";

    [SyncVar]
    public int resourceCost;

    [SyncVar]
    public bool costsMaterial=false;


    bool inspecting = false;

    protected void populateTemplate()
    {

        
        Cardmaker mkr = sourceCardmaker.GetComponent<Cardmaker>();
        cardBody = Instantiate(mkr.findCardTemplate(), transform);
        cardBody.GetComponent<CardUI>().populateSelf(mkr, true);
        resourceCost = mkr.resourceCost;
        colorSelection();
    }

    [Server]
    public virtual void setCardmaker(Cardmaker c) 
    {
        //Debug.Log(c);
        sourceCardmaker = c.gameObject;
    }
    

    public abstract void playCard(Tile target);
    // Start is called before the first frame update
    protected virtual void Start()
    {
		if (isClient)
		{
            sourceCardmaker = (GameObject)Resources.Load(sourceCardmakerPath, typeof(GameObject));
            sourceCardmaker.GetComponent<Cardmaker>().register();

            populateTemplate();
            gm= GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            if(gm.clientTeam == team)
			{
                RectTransform tr = GetComponent<RectTransform>();
                tr.SetParent(GameObject.FindGameObjectWithTag(targetHand).transform, false);
                
                transform.localPosition = Vector3.zero;
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponentInParent<RectTransform>());
            }
            
        }
        
    }
    //public void moveToHand();
    bool hovered = false;
    float maxheight = Screen.height*0.10f;
    //float timeToMax = 0.4f;
    float currentTime = 0;
    // Update is called once per frame
    protected virtual void Update()
    {
        //Debug.Log(hovered);
		if (hovered)
		{
            currentTime += Time.deltaTime;
           
			if (currentTime> GameConstants.hoverInspectTime)
			{
                currentTime = GameConstants.hoverInspectTime;
				if (!inspecting)
				{
                    inspect();
                    inspecting = true;

                }
                
			}
		}
		else
		{
           
            currentTime -= Time.deltaTime;
            if (currentTime < 0)
            {
                currentTime = 0;
            }

			if (inspecting)
			{
                gm.clientPlayer.cardUnInspect(sourceCardmaker.gameObject);
                inspecting = false;
            }

        }
        
        cardBody.transform.localPosition = new Vector3(0, maxheight*(currentTime/ GameConstants.hoverInspectTime), 0);
        
    }

    protected virtual void inspect()
	{
        gm.clientPlayer.cardInspect(sourceCardmaker.gameObject, CardInspector.inspectType.cardmakerPre);
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
        gm.clientPlayer.cardClick(this);
	}

	public int getTeam()
	{
        return team;
	}
    public enum selectType
	{
        none,
        playable,
        active
	}
    selectType sType = selectType.none;
    public void setSelection(selectType t)
    {

        sType = t;
        colorSelection();



    }
    void colorSelection()
	{
		if (cardBody)
		{
            CardUI cui = cardBody.GetComponent<CardUI>();
            switch (sType)
            {
                case selectType.none:
                    cui.select(false, Color.black);
                    break;
                case selectType.playable:
                    cui.select(true, Color.yellow);
                    break;
                case selectType.active:
                    cui.select(true, Color.green);
                    break;

            }
        }
        
    }


    [TargetRpc]
    public void TargetSetRule(NetworkConnection con, Targeting.Rule[] r)
    {
        GetComponent<Targeting>().rules = r;
    }

}
