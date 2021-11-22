using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static StatBlock;
using UnityEditor;
using System.Linq;
using static Card;

public class PlayerGhost : NetworkBehaviour, TeamOwnership
{
	enum targetState
	{
        Free,
        Unit,
        Card,
        Ability
	}
    targetState state = targetState.Free;
    InputHandler inp;
    StatHandler st;
    //selected unit for actions
    Unit unitCurrent;
    TileSelector tselect;

    Card cardCurrent;
    OrdSqUI abilityCurrent;

    Unit hoverUnitCurrent;
    float hoverUnitTime=0;
    //float hoverUnitTimeMax = 1f;
    bool inspecting = false;
    CardInspector inspect;
    TurnIndicatorUI UIturn;
    UnitAbilityUI abilPanel;

    [SyncVar(hook = nameof(refreshResourceUI))]
    int currentResources = 1;
    [SyncVar(hook = nameof(refreshResourceUI))]
    int currentfragments = 0;
    [SyncVar(hook = nameof(refreshResourceUI))]
    int currentMaterials = 0;
    [SyncVar(hook = nameof(refreshResourceUI))]
    int currentShards = 0;

    [SyncVar(hook = nameof(refreshResourceUI))]
    int currentCards = 0;
    //[SyncVar(hook =nameof(alignDirection))]
    public int teamIndex= -1;

    GameManager gm;
    public GameObject cameraPre;
    //Client only, set by server, enables interaction
    bool myTurn = false;

    public bool isTurn
	{
		get
		{
            return myTurn;
		}
	}

    #region cards
    public Deck loadedDeck;
    public Deck playableCards;
	List<GameObject> MainDeck = new List<GameObject>();
    List<GameObject> Structures = new List<GameObject>();

    [HideInInspector]
    public bool isMulligan = true;
    List<Card> toMulligan = new List<Card>();
    public void cardClick(Card c)
	{
		if (isMulligan)
		{
			if (toMulligan.Contains(c))
			{
                c.setSelection(selectType.none);
                toMulligan.Remove(c);
			}
			else
			{
                c.setSelection(selectType.active);
                toMulligan.Add(c);
            }
		}
		else if (myTurn)
		{
            //Debug.Log(cost);
            if (c == cardCurrent)
            {
                state = targetState.Free;
                setTargetCard(null);
            }
            else if (canAfford(c))
            {
                state = targetState.Card;
                //setTargetAbility(null);
                setTargetUnit(null);
                setTargetCard(c);
            }
        }
        

	}
    bool canAfford(Card c)
	{
        return (
                    !c.costsMaterial && c.resourceCost <= currentResources
                    ||
                    c.costsMaterial && c.resourceCost <= currentMaterials
                )

                && c.resourceCost <= st.getStat(StatType.resourceSpend);
	}
    public void mulligan()
	{
		if (isMulligan)
		{
            CmdMulligan(toMulligan.Select(x => x.netId).ToList());
		}
	}

    public void cardInspect(GameObject body, CardInspector.inspectType type, Dictionary<StatType, float> stats = null)
	{
        inspect.inspect(body,type,2, stats);
	}
    public void cardUnInspect(GameObject body)
    {
        inspect.uninspect(body);
    }
 //   public void drawCardsOnTurn()
	//{

 //       drawCards(st.getStat(StatType.cardShardIncome) + 1);

	//}

    [Server]
    public void drawCards(int cards)
    {
        for (int i = 0; i < cards; i++)
		{
            if (MainDeck.Count > 0)
            {
                Cardmaker mkr = MainDeck[0].GetComponent<Cardmaker>();
                MainDeck.RemoveAt(0);
                spawnCard(mkr, "PlayerHand");
                
            }
        }

        currentCards = MainDeck.Count;
    }
    public void returnCardToDeck(Card c)
	{
        //Debug.Log(c);
        //Debug.Log(c.sourceCardmaker);
        addCard(c.sourceCardmaker);
        //gm.delayedDestroy(c.gameObject);
        Destroy(c.gameObject);
        
	}
    public void addCard(GameObject cPrefab)
	{
        int i = Mathf.FloorToInt(Random.Range(0, MainDeck.Count));
        MainDeck.Insert(i, cPrefab);
        currentCards = MainDeck.Count;
	}
    [Server]
    public void createStructureSideboard()
	{
        foreach(GameObject struPre in Structures)
		{
            Cardmaker mkr = struPre.GetComponent<Cardmaker>();
            //Two per instance
            spawnCard(mkr, "StructureBoard");

        }
	}
    [Client]
    void findClientDeck()
	{
        GameObject exp;
        exp = GameObject.FindGameObjectWithTag("DeckExport");
		if (exp)
		{
            //exp.GetComponent<DeckExport>().printDeck();
            Dictionary<int, int>[] deck = exp.GetComponent<DeckExport>().getDeck();
            CmdSubmitDeck(DeckRW.writeDeck(deck[0],deck[1]));
		}
    }
    [Command]
    //void CmdSubmitDeck(Dictionary<int, int> deckMaine, Dictionary<int, int> deckStrc )
    void CmdSubmitDeck(string deckStr)
    {
        Dictionary<int, int>[] deck = DeckRW.readDeck(deckStr);

        
        bool checkDeck(Dictionary<int,int> deckL, bool isMain)
		{
            int totalCount =0;
            foreach(int card in deckL.Keys)
			{
				if (card < 0)
				{
                    return false;
				}
                if((isMain && card>=playableCards.main.Count) || (!isMain && card >= playableCards.structures.Count))
				{
                    return false;
				}
                if(deckL[card] > GameConstants.maxCardDuplicateLimit)
				{
                    return false;
				}
                totalCount += deckL[card];
			}
            if(totalCount!= (isMain? GameConstants.mainDeckSize:GameConstants.structureDeckSize) )
			{
                return false;
			}
            return true;
        }
        if(! checkDeck(deck[0],true) || ! checkDeck(deck[1], false))
		{
            return;
		}




        loadedDeck = ScriptableObject.CreateInstance<Deck>();
        loadedDeck.main = deckLookup(deck[0], true);
        loadedDeck.structures = deckLookup(deck[1], false);

        
    }
    List<GameObject> deckLookup(Dictionary<int, int> deckCounts, bool isMain)
	{
        List<GameObject> deck = new List<GameObject>();
        foreach(int i in deckCounts.Keys)
		{
            for(int j=0; j<deckCounts[i]; j++)
			{
				if (isMain)
				{
                    deck.Add(playableCards.main[i]);

                }
				else
				{
                    deck.Add(playableCards.structures[i]);
                }
                
			}
		}

        return deck;
    }
    [Server]
    public void initDeck()
    {
        //Debug.Log(teamIndex.ToString());
        foreach (GameObject c in loadedDeck.main)
        {
            MainDeck.Add(c);
            //double cards
            //MainDeck.Add(c);
        }
        foreach (GameObject c in loadedDeck.structures)
        {
            Structures.Add(c);
        }
        //MainDeck = loadedDeck.main;
        //Structures = loadedDeck.structures;
        MainDeck.Shuffle();
        currentCards = MainDeck.Count;
    }
    void spawnCard(Cardmaker mkr, string targetHand)
	{
        GameObject c_obj = Instantiate(mkr.findCardPrefab(), transform);
        Card c = c_obj.GetComponent<Card>();
        c.team = teamIndex;
        c.targetHand = targetHand;
        mkr.modifyCardAfterCreation(c_obj);
        
        //TODO: Spawn only for client
        NetworkServer.Spawn(c_obj);
        Targeting tar = c.GetComponent<Targeting>();
        c.TargetSetRule(connectionToClient, tar.rules);
        //TargetClaimCard(connectionToClient, c.netId);
    }


    #endregion
    #region resources
    //static readonly int RESOURCE_MAX_CAP = 6;
    //static readonly int RESOURCE_INCOME_CAP = 3;
    static readonly float TEMPO_MULTIPLIER = 1f;
    static int TM(int i)
	{
        return Mathf.FloorToInt(i * TEMPO_MULTIPLIER);
	}
    public void refreshResources()
	{
        currentResources += TM(st.getStat(StatType.supplyIncome));
		if (currentResources > TM(st.getStat(StatType.supplyMax)))
		{
            currentResources = TM(st.getStat(StatType.supplyMax));
		}

        currentfragments += st.getStat(StatType.structureFragmentIncome);
		while (currentfragments >= 6)
		{
            currentfragments -= 6;
            currentMaterials += 1;
		}

        currentShards += TM(st.getStat(StatType.cardShardIncome));
        int cardsToDraw = 0;
        while (currentShards >= 6)
        {
            currentShards -= 6;
            cardsToDraw += 1;
        }
		if (cardsToDraw > 0)
		{
            drawCards(cardsToDraw);
		}
    }
    public void increaseMaxResources()
	{
        //if (st.getStat(StatType.resourceMax, true) < RESOURCE_MAX_CAP)
        //{
        //    st.modifyStat(StatType.resourceMax, 1);
        //}
        st.modifyStat(StatType.supplyMax, 1);

    }
    public void increaseIncomeResources()
    {
        //if (st.getStat(StatType.resourceIncome, true) < RESOURCE_INCOME_CAP)
        //{
        //    st.modifyStat(StatType.resourceIncome, 1);
        //}
        st.modifyStat(StatType.supplyIncome, 1);
    }
    public void increaseSpendResources()
	{
        st.modifyStat(StatType.resourceSpend, 1);
    }
    public void increaseIncomeMaterial()
    {
        st.modifyStat(StatType.structureFragmentIncome, 1);
    }
    public int getCurrentResources()
	{
        return currentResources;
	}
    public int getCurrentCards()
    {
        return currentCards;
    }
    public int getCurrentMaterials()
    {
        return currentMaterials;
    }
    public int getCurrentFragments()
    {
        return currentfragments;
    }
    public int getCurrentShards()
    {
        return currentShards;
    }
    public int getCurrentSpendLimit()
	{
        return st.getStat(StatType.resourceSpend);
	}
    public void spendResources(int i, bool isMaterial = false)
	{
		if (isMaterial)
		{
            currentMaterials -= i;
		}
		else
		{
            currentResources -= i;
        }
        updatePlayableCards();

    }
    public void gainResources(int i)
	{
        currentResources += i;
        updatePlayableCards();
    }
    void refreshResourceUI(int oldVal, int newVal)
	{
		if (isLocalPlayer)
		{
            FindObjectOfType<ResourceUI>().refresh();
        }
        
    }
    #endregion
    #region abilitites
    public void abilityClick(OrdSqUI o)
    {
		if (myTurn)
		{
			int cost = o.ability.resourceCost;
			//Debug.Log(cost);
			if (o == abilityCurrent)
			{
				state = targetState.Unit;
				setTargetAbility(null);
			}
			else if (cost <= currentResources && o.ability.hasCharges() && o.ability.caster.canCast)
			{
				state = targetState.Ability;
				//setTargetUnit(null);
                setTargetCard(null);
                setTargetAbility(o);
			}
		}


	}
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        inp = GetComponent<InputHandler>();
        gm = FindObjectOfType<GameManager>();
        st = GetComponent<StatHandler>();

		if (isServer)
		{
            st.initialize();
            if(!gm.registerTeam(netId, out teamIndex))
			{
                Debug.LogError("Registraion error");
			}
			else
			{
                //initDeck();
                RpcAssignTeam( teamIndex);
            }
            
        }
		if (!isServer)
		{
            alignDir();
            
        }
		if (isLocalPlayer)
		{

            Instantiate(cameraPre, transform);
            StatHandler.Refresh reUI = FindObjectOfType<ResourceUI>().refresh;
            st.addRefresh(reUI);
            reUI = updatePlayableCards;
            st.addRefresh(reUI);
            inspect = FindObjectOfType<CardInspector>();
            UIturn = FindObjectOfType<TurnIndicatorUI>();
            abilPanel = FindObjectOfType<UnitAbilityUI>();
            tselect = GetComponent<TileSelector>();
            findClientDeck();
            //Debug.Log("Refresh added");
        }
    }
    


    [ClientRpc]
    void RpcAssignTeam(int newTeam)
	{
        teamIndex = newTeam;
		if (isLocalPlayer)
		{
            alignDir();
        }
        



    }
    void alignDir()
	{
        if (teamIndex>-1 && gm)
		{
            gm.clientPlayer = this;
            st.runRefresh();

            Quaternion rot = Quaternion.LookRotation(GameManager.dirs[teamIndex]);
            transform.rotation = rot;
            float height = transform.position.y;
            Vector3 dest = gm.transform.position - transform.forward * 2;
            dest.y = height;
            transform.position = dest;
            gm.GetComponent<GameGrid>().setUIDir(rot);
        }
	}


    public void setTurn( bool isTurn)
    {
        myTurn = isTurn;
		if (!isTurn)
		{
            //setTargetAbility(null);
            setTargetUnit(null);
            setTargetCard(null);
            state = targetState.Free;
            
        }
        updatePlayableCards();
        UIturn.setTurn(isTurn);

    }

    void updatePlayableCards()
	{
        foreach(Card c in FindObjectsOfType<Card>())
		{
            updateCardSelection(c);

        }
	}
    void updateCardSelection(Card c)
	{
        if (!myTurn)
        {
            c.setSelection(selectType.none);
        }
        else
        {
            if (canAfford(c))
            {
                c.setSelection(selectType.playable);
            }
            else
            {
                c.setSelection(selectType.none);
            }
        }
    }
    bool isFixating = false;
    Vector3 preFixPosition;
    // Update is called once per frame
    void Update()
    {
		if (!isLocalPlayer)
		{
            return;
		}
        inp.updateInputs();
		if (myTurn)
		{
            if (inp.endTurn)
            {
                CmdEndTurn();
            }
            else if (inp.click && inp.target)
            {
                //Debug.Log(inp.target.name);
                Unit u;
                switch (state)
                {
                    case targetState.Free:
                        u = inp.target.getOccupant();
                        if (u && u.teamIndex == teamIndex)
                        {
                            setTargetUnit(u);
                            state = targetState.Unit;
                            //Debug.Log("Unit");
                        }
                        break;
                    case targetState.Unit:
                        //Debug.Log("Move");
                        u = inp.target.getOccupant();
                        if (u)
                        {
                            //gm.attack(unitCurrent, inp.target);
                            if (u.teamIndex == teamIndex)
                            {
                                if(u == unitCurrent)
								{
                                    state = targetState.Free;
                                    setTargetUnit(null);
                                }
								else
								{
                                    setTargetUnit(u);
                                }
                                
                            }
                            else
                            {
                                CmdPawnAttack(unitCurrent.netId, inp.target.netId);
                                state = targetState.Free;
                                setTargetUnit(null);
                            }

                        }
                        else
                        {
							if (inp.target.getTerrainWalkCost(unitCurrent.type) != -1)
							{
                                //gm.move(unitCurrent, inp.target);
                                CmdPawnMove(unitCurrent.netId, inp.target.netId);
                                state = targetState.Free;
                                setTargetUnit(null);
                            }
                            
                        }


                        break;
                    case targetState.Card:
						//if (!cardCurrent)
						//{

						//}
						if (cardCurrent.GetComponent<Targeting>().evaluate(inp.target,teamIndex))
						{
                            CmdCardPlay(cardCurrent.netId, inp.target.netId);
                            state = targetState.Free;
                            setTargetCard(null);

                            
                        }
                        //else
                        //{
                        //     state = targetState.Free;
                        //     setTargetCard(null);
                        // }
                        break;
                    case targetState.Ability:
                        if (abilityCurrent.ability.GetComponent<Targeting>().evaluate(inp.target, teamIndex, unitCurrent.loc))
                        {
                            CmdAbilityCast(abilityCurrent.ability.GetComponent<NetworkIdentity>().netId, inp.target.netId);
                            state = targetState.Unit;
                            setTargetAbility(null);

                        }

                        break;
                    default:
                        Debug.LogError("not implemented");
                        break;
                }
            }
            else if (inp.cancel)
			{
				switch (state)
				{
                    case targetState.Free:
                        //do nothing
                        break;
                    case targetState.Unit:
                        state = targetState.Free;
                        setTargetUnit(null);
                        break;
                    case targetState.Card:
                        state = targetState.Free;
                        setTargetCard(null);
                        break;
                    case targetState.Ability:
                        state = targetState.Unit;
                        setTargetAbility(null);
                        break;
                }
			}
            
        }
        
        //Inspect unit on board
        if (true/*!myTurn || state == targetState.Free*/)
        {
            if (inp.target && inp.target.getOccupant())
            {
                if (hoverUnitCurrent && hoverUnitCurrent == inp.target.getOccupant())
                {
                    if (!inspecting)
                    {
                        hoverUnitTime += Time.deltaTime;
                        if (hoverUnitTime > GameConstants.hoverInspectTime)
                        {
                            inspecting = true;
                            inspect.inspect(hoverUnitCurrent.gameObject, CardInspector.inspectType.cardmaker,3);
                            //tilesSelectedHover = inp.target.select(true);
                            tselect.select(inp.target, true);
                        }

                    }

                }
                else
                {

                    uninspect( inp.target.getOccupant());
                    
                    

                }
            }
            else
            {
                uninspect(null);

            }
        }

        Vector3 camForward = transform.GetChild(0).forward;

        //TODO Error Here (Client is prolly still loading the GM, doesnt have its pipe yet)
        if (gm.viewPipe && gm.viewPipe.isFixating)
		{
			if (!isFixating)
			{
                preFixPosition = transform.position;
                isFixating = true;
			}
            transform.position = gm.viewPipe.fixation - camForward * Mathf.Abs((transform.position.y - gm.viewPipe.fixation.y)/ camForward.y); 

        }
		else
		{
			if (isFixating)
			{
                transform.position = preFixPosition;
                isFixating = false;
			}
            transform.position += (transform.right * inp.pan.x + transform.forward * inp.pan.y) * transform.position.y;
        }
        
        transform.position += camForward * inp.zoom;
    }
    void uninspect(Unit newInspect)
	{
        if (inspecting)
        {
            inspecting = false;
            inspect.uninspect(hoverUnitCurrent.gameObject);
            tselect.deselect(true);
        }

        hoverUnitCurrent = newInspect;
        hoverUnitTime = 0;
        

    }
    void setTargetUnit(Unit u)
	{
        if(unitCurrent != null)
		{
            tselect.deselect(false);
            abilPanel.clearAll();
		}
        if(u != null)
		{
            tselect.select(u.loc,false);
            foreach(AbilityRoot o in u.castable)
			{
                //Debug.Log(o);
                abilPanel.addAbility(o);
			}
        }
        unitCurrent = u;
	}
    void setTargetCard(Card c)
    {
        if (cardCurrent != null)
        {
            tselect.deselect(false);
            updateCardSelection(cardCurrent);

        }
        if (c != null)
        {
            tselect.select(null, false, c.GetComponent<Targeting>(),teamIndex);
            c.setSelection(selectType.active);
        }
        cardCurrent = c;
    }
    void setTargetAbility(OrdSqUI sq)
    {
        //Debug.Log(sq);
        if (abilityCurrent != null)
        {
            abilityCurrent.setSelection(false);
            tselect.deselect(false);
        }
        if (sq != null)
        {
            sq.setSelection(true);
        }
        abilityCurrent = sq;
        if(abilityCurrent == null)
		{
			if (unitCurrent && unitCurrent.isAlive)
			{
                tselect.select(unitCurrent.loc, false);

            }
			else
			{
                state = targetState.Free;
            }
            
        }
		else
		{
            tselect.deselect(false);
            tselect.select(unitCurrent.loc, false, abilityCurrent.ability.GetComponent<Targeting>(), teamIndex);

        }
    }
    public void checkUnitSelectionDeath(GameObject o)
	{
        //Debug.Log(o);
        if(unitCurrent?.gameObject == o)
		{
            setTargetUnit(null);
		}
	}
    [Command]
    void CmdPawnMove(uint unitID, uint tileID)
	{        
        gm.move(netId ,unitID, tileID);
	}

    [Command]
    void CmdPawnAttack(uint unitID, uint tileID)
    {
        gm.attack(netId,unitID, tileID);
    }
    [Command]
    void CmdCardPlay(uint cardID, uint tileID)
	{
        gm.cardPlay(netId, cardID, tileID);
	}
    [Command]
    void CmdAbilityCast(uint abilID, uint tileID)
    {
        gm.abilityCast(netId, abilID, tileID);
    }
    [Command]
    void CmdEndTurn()
	{
        gm.endTurn(netId);
        
	}
    [Command]
    void CmdMulligan(List<uint> cardIDs)
	{
        gm.mulligan(netId, cardIDs);
	}

	public int getTeam()
	{
        return teamIndex;

    }
}
