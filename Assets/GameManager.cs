using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    //public readonly SyncDictionary<unit>
    Dictionary<uint, int> teams = new Dictionary<uint, int>();
    public static Vector3[] dirs = new Vector3[] { Vector3.forward, -Vector3.forward, Vector3.right, -Vector3.right };
    int currentTurn = -1;
    [SyncVar (hook =nameof(showLimitIncreace))]
    int roundCounter = 0;


    public PlayerGhost clientPlayer;
    public int clientTeam
    {
        get
        {
            return clientPlayer.teamIndex;
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
    }
    public void initUnits()
    {

        GetComponent<GameGrid>().initialize();
        drawOpeningHand();
        nextTurn();
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
	#region turn control
	void nextTurn()
    {
        //Debug.Log(currentTurn);
        setTeamUI(false);
        int refreshTeam = currentTurn;
        currentTurn += 1;
        if (currentTurn >= teams.Count)
        {
            currentTurn = 0;
            
        }
        refreshUnits(refreshTeam);
        playerRoundEnd(refreshTeam);
        if(currentTurn == 0)
		{
            roundCounter++;
        }
        setTeamUI(true);

    }
    void setTeamUI(bool isTurn)
    {
        if (currentTurn == -1) { return; }

        foreach (uint playerID in teams.Keys)
        {
            if (teams[playerID] == currentTurn)
            {
                PlayerGhost p = NetworkIdentity.spawned[playerID].GetComponent<PlayerGhost>();

                p.TargetSetTurn(p.netIdentity.connectionToClient, isTurn);

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
    public int resource_rate = 1;
    public int card_rate = 2;
    //public int max_income_rate = 4;
    //public int max_cap_rate = 2;
    public static bool[] maxIncome = new bool[]        
    { 
        false, true,
        false, true,
        false, true,
        false, true,
        false, true,
    };
    public static bool[] maxCapacity = new bool[]      
    { 
        false, true, 
        false, true, 
        false, true, 
        false, true, 
        false, true, 
        false, true,
        false, true,
        false, true,
        false, true,
        false, true,
    };
    public static bool[] maxSpendLimit = new bool[]    
    {   
        false, true,
        false, false, true, 
        false, false, true, 
        false, false, true, 
        false, false, true, 
        false, false, true, 
    };
    void roundEndActions(PlayerGhost p)
	{
        int roundInd = roundCounter - 1;
        if((roundCounter+1) % card_rate == 0)
		{
            p.drawCardsOnTurn();
        }
		//if ((roundCounter - 1) % max_cap_rate == 0)
        if(roundInd < maxCapacity.Length && maxCapacity[roundInd])
		{
			p.increaseMaxResources();
		}
        //Debug.Log(roundCounter % resource_rate);
        if (roundCounter % resource_rate == 0)
        {
            p.refreshResources();
        }
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
    static readonly int COUNT_OPENING_HAND=5;
    void drawOpeningHand()
	{
        foreach (uint playerID in teams.Keys)
        {

            PlayerGhost p = NetworkIdentity.spawned[playerID].GetComponent<PlayerGhost>();
            //Debug.Log(team);
            for(int i =0; i < COUNT_OPENING_HAND; i++)
			{
                p.drawCard();
			}
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
    public void refreshUnits(int team)
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
    // Update is called once per frame
    void Update()
    {

    }
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
            attacker.attack(defender, dist, didBypass);
            RpcAttackUnit(unitID, tileID);
        }
    }
    [ClientRpc]
    void RpcAttackUnit(uint unitID, uint tileID)
    {
        if (!NetworkIdentity.spawned.ContainsKey(unitID))
        {
            return;
        }
        Unit attacker = NetworkIdentity.spawned[unitID].GetComponent<Unit>();
        Tile target = NetworkIdentity.spawned[tileID].GetComponent<Tile>();

        attacker.attackInDir(target, 1);
    }
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


        bool walking = !(mover.type == Unit.unitType.flying);


        int dist = origin.distToTile(target, walking, mover.teamIndex, mover.stat.getBool(StatBlock.StatType.ghost));
        //Debug.Log(dist);

        if  (
            mover
            && !target.getOccupant()
            && (target.isWalk || !walking)

            //&& origin.isNeighbor(target)
            //&& mover.moveRemaining > 0 
            && dist > 0
            && mover.moveRemaining >= dist
            
            && (!mover.isStructure || target.isFoundation)
            )
        {
            mover.move(dist);
            transferToTile(mover, target, true);
            RpcMoveUnit(unitID, tileID);
        }
    }
    [ClientRpc]
    void RpcMoveUnit(uint unitID, uint tileID)
    {
        if (!NetworkIdentity.spawned.ContainsKey(unitID))
        {
            return;
        }
        Unit mover = NetworkIdentity.spawned[unitID].GetComponent<Unit>();
        Tile target = NetworkIdentity.spawned[tileID].GetComponent<Tile>();
        transferToTile(mover, target);
    }

    void transferToTile(Unit u, Tile t, bool onServer = false)
    {
        //if ((onServer && isServer) || (!onServer && !isServer))
        if(true)
        {
            Tile origin = u.loc;


            origin.occExit();
            t.occEnter(u);
            //Debug.Log("Move" + isServer + isClient);
            u.moveToLoc(1);
        }

        

    }
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
            && player.getCurrentResources() >= playedCard.resourceCost
            && player.getCurrentSpendLimit() >= playedCard.resourceCost
            && t.evaluate(target, teams[ownerID])
            )
		{
            player.spendResources(playedCard.resourceCost);
            playedCard.playCard(target);
		}
    }
    [Server]
    public void abilityCast(uint ownerID, uint abilID, uint tileID)
    {
        if (!messageCheck(ownerID, abilID, tileID))
        {
            return;
        }
        Ordnance castAbility = NetworkIdentity.spawned[abilID].GetComponent<Ordnance>();
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
            }
            
            
        }
    }
    #endregion
}
