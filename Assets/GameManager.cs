using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static ViewPipeline;
using static GameConstants;

public class GameManager : NetworkBehaviour
{
    //public readonly SyncDictionary<unit>
    Dictionary<uint, int> teams = new Dictionary<uint, int>();
    public static Vector3[] dirs = new Vector3[] { Vector3.forward, -Vector3.forward, Vector3.right, -Vector3.right };

    [SyncVar (hook = nameof(turnUIUpdates))]
    int currentTurn = -1;
    [SyncVar (hook =nameof(showLimitIncreace))]
    int roundCounter = 0;

    ViewPipeline pipe;
    List<GameObject> cleanupStaging = new List<GameObject>();
    List<GameObject> cleanup = new List<GameObject>();

    public PlayerGhost clientPlayer;
    public int clientTeam
    {
        get
        {
            return clientPlayer.teamIndex;
        }
    }
    
    public ViewPipeline viewPipe
	{
		get
		{
            return pipe;
		}
	}
    public int whosTurn
	{
		get
		{
            return currentTurn;
		}
	}

    //public static GameManager gm;
    // Start is called before the first frame update
    void Start()
    {

        //if (!gm)
        //{
        //          gm = this;
        //}
        //else
        //{
        //          Destroy(gameObject);
        //}
        pipe = GetComponent<ViewPipeline>();
    }
    public void initUnits()
    {

        GetComponent<GameGrid>().initialize();
        initializeTerrain();
        drawOpeningHand();
        foreach (uint playerID in teams.Keys)
        { 
            TargetShowMulligan(NetworkIdentity.spawned[playerID].connectionToClient,true);

        }
        
    }
    public GameObject mulliganScreen;
    [TargetRpc]
    void TargetShowMulligan(NetworkConnection conn, bool show)
	{
        mulliganScreen.SetActive(show);
		if (!show)
		{
            clientPlayer.isMulligan = false;
		}

    }
    [Client]
    public void localSubmitMulligan()
	{
        clientPlayer.mulligan();
	}
    void initGame()
	{
        drawSideboards();
        firstTurn();
    }
    public void teamUnitUpstreamStats(StatHandler st, int team)
	{
        foreach (uint playerID in teams.Keys)
        {
            if (teams[playerID] == team)
            {
                StatHandler handler = NetworkIdentity.spawned[playerID].GetComponent<StatHandler>();

                handler.addUpstream(st);

            }

        }
    }
    public void delayedDestroy(GameObject o)
	{
        cleanupStaging.Add(o);
        foreach(PseudoDestroy pd in o.GetComponentsInChildren<PseudoDestroy>())
		{
            pd.PDestroy(true);
		}
        //StartCoroutine(sendDestroyLater(0.2f, o.GetComponent<NetworkIdentity>().netId));
        pipe.QueueViewEvent(new ViewEvent(ViewType.objDeath, o.GetComponent<NetworkIdentity>().netId, 0, Time.time));
        if (!isClient)
		{
            o.SetActive(false);
        }
        
	}
    IEnumerator sendDestroyLater(float t, uint id)
    {
        // suspend execution for 5 seconds
        yield return new WaitForSeconds(t);
        pipe.QueueViewEvent(new ViewEvent(ViewType.objDeath, id, 0, Time.time));
    }
    #region turn control
    void firstTurn()
	{
        currentTurn = 0;
        roundCounter = 1;
        foreach (uint playerID in teams.Keys)
        {
            PlayerGhost p = NetworkIdentity.spawned[playerID].GetComponent<PlayerGhost>();
            //p.TargetSetTurn(p.netIdentity.connectionToClient, teams[playerID]==currentTurn);
            if (teams[playerID] == currentTurn)
            {
                pipe.TargetAddViewEvent(p.netIdentity.connectionToClient, new ViewEvent(ViewType.beginTurn, 0, 0, Time.time));
            }
            else
            {
                pipe.TargetAddViewEvent(p.netIdentity.connectionToClient, new ViewEvent(ViewType.endTurn, 0, 0, Time.time));
            }
        }
    }
	void nextTurn()
    {
        //Debug.Log(currentTurn);
        setTeamUI(false);
        foreach(GameObject o in cleanup)
		{
            Destroy(o);
		}
        cleanup = cleanupStaging;
        cleanupStaging = new List<GameObject>();
        int refreshTeam = currentTurn;
        currentTurn += 1;
        if (currentTurn >= teams.Count) //has to change for multiple players ont he same team
        {
            currentTurn = 0;
            
        }
        refreshUnits(refreshTeam);
        playerRoundEnd(refreshTeam);
        tickBuffs(currentTurn);
        pipe.dispatchEvents();
        if(currentTurn == 0)
		{
            roundCounter++;
        }
        setTeamUI(true);

    }
    void setTeamUI(bool isTurn)
    {
        
        if (currentTurn == -1) { return; }
        //if ( teams.Count <= 1) { return; }

        foreach (uint playerID in teams.Keys)
        {
            if (teams[playerID] == currentTurn)
            {
                PlayerGhost p = NetworkIdentity.spawned[playerID].GetComponent<PlayerGhost>();

				//p.TargetSetTurn(p.netIdentity.connectionToClient, isTurn);
				if (isTurn)
				{
                    pipe.TargetAddViewEvent(p.netIdentity.connectionToClient, new ViewEvent(ViewType.beginTurn, 0, 0, Time.time));
				}
				else
				{
                    pipe.TargetAddViewEvent(p.netIdentity.connectionToClient, new ViewEvent(ViewType.endTurn, 0, 0, Time.time));
                }

            }

        }
    }
    void playerRoundEnd(int team)
    {
        if (team == -1) { return; }

        foreach (uint playerID in teams.Keys)
        {
            //Debug.Log(playerID);
            if (teams[playerID] == team)
            {
                PlayerGhost p = NetworkIdentity.spawned[playerID].GetComponent<PlayerGhost>();
                //Debug.Log(team);
                roundEndActions(p);

            }

        }
    }
    //public int max_income_rate = 4;
    //public int max_cap_rate = 2;
    public static bool[] maxIncome = new bool[]        
    { 
        false, true,
        false, true,
        false, true,

    };
    public static bool[] maxCapacity = new bool[]      
    { 
        true, true, 
        true, true, 
        true, true, 
        true, true, 
        true, true, 
        true, true,
        true, true,
        true, true,
        true, true,
        true, true,
        true, true,
    };
    public static bool[] maxSpendLimit = new bool[]    
    {   
        false, true,
        false, false, true, 
        false, false, true, 
        false, false, true, 
        false, false, true,
        false, false, true,
        false, false, true,
    };
    public static bool[] maxFragmentIncome = new bool[]
    {
        false, false,
        false, false, false,
        false, false, true,
        false, false, true,
        false, false, true,
        false, false, true,
        //false, false, true,
    };
    void roundEndActions(PlayerGhost p)
	{
        int roundInd = roundCounter - 1;
        //      if((roundCounter+1) % card_rate == 0)
        //{
        //          p.drawCardsOnTurn();
        //      }
        //if ((roundCounter - 1) % max_cap_rate == 0)
        if (roundInd < maxFragmentIncome.Length && maxFragmentIncome[roundInd])
        {
            p.increaseIncomeMaterial();
        }
        if (roundInd < maxCapacity.Length && maxCapacity[roundInd])
		{
			p.increaseMaxResources();
		}
        //Debug.Log(roundCounter % resource_rate);

        p.refreshResources();

        if(roundInd < maxSpendLimit.Length && maxSpendLimit[roundInd])
		{
            p.increaseSpendResources();
		}

        //if ((roundCounter+ max_income_rate/2) % max_income_rate == 0 && roundCounter > max_income_rate/2)
        if (roundInd < maxIncome.Length && maxIncome[roundInd])
        {
            p.increaseIncomeResources();
        }
    }
    
    void drawOpeningHand()
	{
        foreach (uint playerID in teams.Keys)
        {
            
            PlayerGhost p = NetworkIdentity.spawned[playerID].GetComponent<PlayerGhost>();
            p.initDeck();
            //Debug.Log(team);
            p.drawCards(cardCountOpeningHand);



        }
    }
    void drawSideboards()
    {
        foreach (uint playerID in teams.Keys)
        {

            PlayerGhost p = NetworkIdentity.spawned[playerID].GetComponent<PlayerGhost>();

            p.createStructureSideboard();


        }
    }
    public void endTurn(uint ownerID)
    {
        if (teams[ownerID] != currentTurn)
        {
            return;
        }
        //Debug.Log(currentTurn);
        nextTurn();


    }
    List<uint> playersMulligan = new List<uint>();
    public void mulligan(uint ownerID, List<uint> cardIDs)
	{
        //Debug.Log(ownerID);
        if (!NetworkIdentity.spawned.ContainsKey(ownerID))
        {
            return;
        }
		if (playersMulligan.Contains(ownerID))
		{
            return;
        }
        foreach (uint id in cardIDs)
		{
            //Debug.Log(id);
            if (!NetworkIdentity.spawned.ContainsKey(id) )
            {
                return;
            }
            Card c = NetworkIdentity.spawned[id].GetComponent<Card>();
            if (!c)
            {
                return;
            }
            if (c.team != teams[ownerID])
            {
                return;
            }

        }
        PlayerGhost p = NetworkIdentity.spawned[ownerID].GetComponent<PlayerGhost>();
        //set p not mulligan
        p.drawCards(cardIDs.Count);
        foreach (uint id in cardIDs)
        {
            p.returnCardToDeck(NetworkIdentity.spawned[id].GetComponent<Card>());
        }
        playersMulligan.Add(ownerID);
        TargetShowMulligan(p.connectionToClient, false);
        if(playersMulligan.Count == teams.Count)
		{
            initGame();
		}

    }
    [Server]
    void refreshUnits(int team)
    {
        if (team == -1) { return; }

        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            if (u.teamIndex == team)
            {
                u.refresh();
            }
            
        }
    }
    [Server]
    void tickBuffs(int team)
    {
        if (team == -1) { return; }

        foreach (Buff b in FindObjectsOfType<Buff>())
        {
            if (b.getTeam() == team)
            {
                b.tick();
            }

        }
    }
    [Client]
    void turnUIUpdates(int oldVal, int newVal)
	{
        foreach (Unit u in FindObjectsOfType<Unit>())
        {
            u.refreshUI();

        }
    }
    void showLimitIncreace(int lastRound, int thisRound)
	{
        int roundInd = thisRound - 1;
        ResourceUI rui = FindObjectOfType<ResourceUI>();
        rui.setLimitIncrease(roundInd < maxSpendLimit.Length ? maxSpendLimit[roundInd] : false);
        rui.setMaxIncrease(roundInd < maxCapacity.Length ? maxCapacity[roundInd] : false);

    }
	#endregion
	public bool registerTeam(uint playerID, out int team)
    {
        team = -1;
        if (!teams.ContainsKey(playerID))
        {
            team = teams.Count;
            teams.Add(playerID, team);

            return true;
        }
        return false;
    }
    public delegate void callOnPlayers(PlayerGhost p);

    public void delegateToTeam(callOnPlayers call, int teamInd)
	{
        foreach (uint playerID in teams.Keys)
        {
            if (teams[playerID] == teamInd)
            {
                call(NetworkIdentity.spawned[playerID].GetComponent<PlayerGhost>());



            }

        }
    }
    #region auras
    public GameObject ForestAuraPre;
    public GameObject HillAuraPre;
    public GameObject LaunchpadAuraPre;

    void initializeTerrain()
	{
        GameObject auForest = Instantiate(ForestAuraPre, transform);
        Aura auraForest = auForest.GetComponent<Aura>();
        GameObject auHill = Instantiate(HillAuraPre, transform);
        Aura auraHill = auHill.GetComponent<Aura>();
        GameObject auLaunchpad = Instantiate(LaunchpadAuraPre, transform);
        Aura auraLaunchpad = auLaunchpad.GetComponent<Aura>();

        foreach(Tile t in GetComponent<GameGrid>().allTiles)
		{
			switch (t.type)
			{
                case Tile.terrainType.hill:
                    auraHill.bindTile(t);
                    break;
                case Tile.terrainType.forest:
                    auraForest.bindTile(t);
                    break;
                case Tile.terrainType.launchpad:
                    auraLaunchpad.bindTile(t);
                    break;
            }
		}

        NetworkServer.Spawn(auForest);
        NetworkServer.Spawn(auHill);
        NetworkServer.Spawn(auLaunchpad);

    }
    #endregion

	#region client messages
	[Server]
    bool messageCheck(uint ownerID, uint selectedID, uint tileID)
    {
        if (!NetworkIdentity.spawned.ContainsKey(ownerID))
        {
            return false;
        }
        if (!NetworkIdentity.spawned.ContainsKey(selectedID))
        {
            return false;
        }
        if (!NetworkIdentity.spawned.ContainsKey(tileID))
        {
            return false;
        }
        NetworkIdentity selection = NetworkIdentity.spawned[selectedID];
        TeamOwnership u = selection.GetComponent<TeamOwnership>();
        if (teams[ownerID] != u.getTeam())
        {
            return false;
        }
        if (teams[ownerID] != currentTurn)
        {
            return false;
        }
        return true;
    }
	#region attack
	[Server]
    public void attack(uint ownerID, uint unitID, uint tileID)
    {
        if (!messageCheck(ownerID, unitID, tileID))
        {
            return;
        }
        Unit attacker = NetworkIdentity.spawned[unitID].GetComponent<Unit>();
        Tile origin = attacker.loc;
        Tile target = NetworkIdentity.spawned[tileID].GetComponent<Tile>();
        Unit defender = target.getOccupant();

        StatHandler aStats = attacker.GetComponent<StatHandler>();
        bool didBypass;
        int dist = origin.rangeToTile(target,aStats.getBool(StatBlock.StatType.bypass), out didBypass);
        int range = aStats.getStat(StatBlock.StatType.range);
        if (range < 1)
		{
            range = 1;
		}

        //Debug.Log(dist + "   -  " + range);

        if (
            attacker
            && defender

            //&& origin.isNeighbor(target) 
            && dist > 0
            && range >= dist

            && attacker.canAttack 
            && attacker.teamIndex != defender.teamIndex
            )
        {
            pipe.QueueViewEvent(new ViewEvent(ViewType.unitAttack, unitID, tileID, Time.time));
            attacker.attack(defender, dist, didBypass);
            pipe.dispatchEvents();
            //RpcAttackUnit(unitID, tileID);

        }
    }
    [Client]
    public void clientAttackUnit(Unit attacker, Tile target)
    {

        attacker.attackInDir(target, clientViewpiplelineActionTime);
    }
	#endregion
	#region move
	[Server]
    public void move(uint ownerID, uint unitID, uint tileID)
    {
        if (!messageCheck(ownerID, unitID, tileID))
        {
            return;
        }

        Unit mover = NetworkIdentity.spawned[unitID].GetComponent<Unit>();
        Tile origin = mover.loc;
        Tile target = NetworkIdentity.spawned[tileID].GetComponent<Tile>();



        int dist = origin.distToTile(target, mover.type, mover.teamIndex, mover.stat.getBool(StatBlock.StatType.ghost));
        //Debug.Log(dist);

        if  (
            mover
            && !target.getOccupant()
            && dist != -1

            //&& origin.isNeighbor(target)
            //&& mover.moveRemaining > 0 
            //&& dist > 0
            && mover.moveRemaining >= dist
            && mover.canMove
            && (!mover.isStructure || target.isFoundation)
            )
        {
            mover.move(dist);
            serverMove(mover,target);
            pipe.dispatchEvents();
        }
    }
    //[ClientRpc]
    //void RpcMoveUnit(uint unitID, uint tileID)
    //{
    //    if (!NetworkIdentity.spawned.ContainsKey(unitID))
    //    {
    //        return;
    //    }
    //    Unit mover = NetworkIdentity.spawned[unitID].GetComponent<Unit>();
    //    Tile target = NetworkIdentity.spawned[tileID].GetComponent<Tile>();
    //    transferToTile(mover, target);
    //}
    [Server]
    public void serverMove(Unit mover, Tile target)
	{
        transferToTile(mover, target, true);
        
        //RpcMoveUnit(unitID, tileID);
        pipe.QueueViewEvent(new ViewEvent(ViewType.unitMove, mover.netId, target.netId, Time.time));
    }

    public void transferToTile(Unit u, Tile t, bool onServer = false)
    {
        //if (!onServer || !isClient)
        if(onServer || !isServer)
        {
            Tile origin = u.loc;


            origin.occExit();
            t.occEnter(u);
            //Debug.Log("Move" + isServer + isClient);
            u.moveToLoc(clientViewpiplelineActionTime);
        }

        

    }
	#endregion
	#region cardPlay
	[Server]
    public void cardPlay(uint ownerID, uint cardID, uint tileID)
    {
        if (!messageCheck(ownerID, cardID, tileID))
        {
            return;
        }
        Card playedCard = NetworkIdentity.spawned[cardID].GetComponent<Card>();
        PlayerGhost player = NetworkIdentity.spawned[ownerID].GetComponent<PlayerGhost>();
        Targeting t = playedCard.GetComponent<Targeting>();
        Tile target = NetworkIdentity.spawned[tileID].GetComponent<Tile>();


        if (
            playedCard
            && (
                !playedCard.costsMaterial && player.getCurrentResources() >= playedCard.resourceCost 
                ||
                playedCard.costsMaterial && player.getCurrentMaterials() >= playedCard.resourceCost
            )
            && player.getCurrentSpendLimit() >= playedCard.resourceCost
            && t.evaluate(target, teams[ownerID])
            )
		{
            player.spendResources(playedCard.resourceCost, playedCard.costsMaterial);
            playedCard.playCard(target);
            delayedDestroy(playedCard.gameObject);
            pipe.dispatchEvents();
		}
    }
    [Server]
    public void abilityCast(uint ownerID, uint abilID, uint tileID)
    {
        if (!messageCheck(ownerID, abilID, tileID))
        {
            return;
        }
        AbilityRoot castAbility = NetworkIdentity.spawned[abilID].GetComponent<AbilityRoot>();
        PlayerGhost player = NetworkIdentity.spawned[ownerID].GetComponent<PlayerGhost>();
        Targeting t = castAbility.GetComponent<Targeting>();
        Tile target = NetworkIdentity.spawned[tileID].GetComponent<Tile>();


        if (
            castAbility
            && player.getCurrentResources() >= castAbility.resourceCost
            && castAbility.caster.canCast
            //&& t.evaluate(target, teams[ownerID], castAbility.caster.loc) //Inside cast now
            )
        {
            if(castAbility.castAbil(target, castAbility.getTeam(), castAbility.caster.loc))
			{
                player.spendResources(castAbility.resourceCost);
                
                pipe.dispatchEvents();
            }
            


        }
    }
	#endregion
	#endregion
}
