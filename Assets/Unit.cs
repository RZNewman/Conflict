using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static StatBlock;
using static Targeting;
using System.Linq;

public class Unit : Cardmaker, TeamOwnership, PseudoDestroy
{
    //Collider col;
    GameManager gm;
    

    public enum unitType
	{
        structure,
        light,
        heavy,
        flying
	}
    public unitType type;
    
    [HideInInspector]
    public Tile loc;
    StatHandler st;
    [SyncVar]
    public int teamIndex;

    
    
    StatHandler.Refresh reUI;
    [SyncVar(hook = nameof(hookRefreshUI))]
    int currentMovement;
    [SyncVar(hook = nameof(hookRefreshUI))]
    int currentHealth;
    [SyncVar(hook = nameof(hookRefreshUI))]
    int currentAttacks;
    [SyncVar(hook = nameof(hookRefreshUI))]
    int currentCasts;
    [SyncVar(hook = nameof(hookRefreshUIEffects))]
    Status.Effects status = new Status.Effects();


    bool hasMoved = false;
	#region init
	// Start is called before the first frame update
	void Start()
    {
        //col = GetComponent<Collider>();
        st = GetComponent<StatHandler>();

		if (isServer)
		{
            spawnAbilitites();
            spawnAuras();
            status.movement = true;
            status.attacking = true;
            status.casting = true;
            status.damage = true;
        }
        if (isClient)
		{
            reUI = refreshUI;
            st.addRefresh(reUI);
            teamRotation();
            visibility(visType.none,visType.off);
            //teamColor();
        }
        
        //Debug.Log("started");
    } 
    public enum visType
	{
        none,
        off,
        on
        
	}
    visType currentVisibility = visType.none;
    public void visibility(visType current, visType target)
	{
        //Debug.Log(current.ToString() + " - " + target.ToString());
        if(currentVisibility == current)
		{
            bool shouldVis = false;
            if(target == visType.off)
			{
                shouldVis = false;
                

            }
            else if(target == visType.on)
			{
                shouldVis = true;
			}
            currentVisibility = target;
            foreach (MeshRenderer rend in GetComponentsInChildren<MeshRenderer>())
            {
                rend.enabled = shouldVis;
            }
        }
        
    }
    void hookRefreshUI(int oldVal, int newVal)
	{
        refreshUI();
	}
    void hookRefreshUIEffects(Status.Effects oldVal, Status.Effects newVal)
    {
        refreshUI();
    }
    public void refreshUI()
	{
		if (loc)
		{
            loc.refeshUI();
        }
        
	}
    void teamRotation()
	{
        transform.rotation = Quaternion.LookRotation(GameManager.dirs[teamIndex]);
    }
    public void teamColor()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        Outline o = GetComponent<Outline>();
        o.enabled = true;
        if(teamIndex == gm.clientTeam)
		{
            o.OutlineColor = GameColors.ally;
		}
		else
		{
            o.OutlineColor = GameColors.enemy;
        }
    }
    [Server]
    public void initialize(int team)
	{
        teamIndex = team;
        
		st = GetComponent<StatHandler>();
		st.initialize();

        
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        
        currentHealth=stat.getStat(StatType.health);
        currentMovement= 0;
        currentAttacks= 0;
        currentCasts = 0;
        gm.teamUnitUpstreamStats(st, teamIndex);
        teamRotation();
        //teamColor();

        
        //refresh();
    }
    [Server]
    public void lateInit()
	{
        if (st.getBool(StatType.charge))
        {
            refresh();
        }
    }
    [Client]
    public override void register() //prefab
	{
        if (!NetworkClient.prefabs.ContainsValue(gameObject))
        {
            NetworkClient.RegisterPrefab(gameObject);
            foreach (GameObject aPre in abilitiesPre)
            {
                aPre.GetComponent<AbilityRoot>().register();
            }
            foreach(GameObject auPre in aurasPre)
			{
                auPre.GetComponent<Aura>().register();
                
            }
        }
    }
	#endregion
	public void refresh()
	{
        currentMovement= st.getStat(StatType.moveSpeed);
		if (currentMovement < 0)
		{
            currentMovement = 0;
		}
        currentAttacks= 1;
        currentCasts = 1;
        currentHealth += stat.getStat(StatType.regen);
        int maxHP = stat.getStat(StatType.health);
		if (currentHealth > maxHP)
		{
            currentHealth = maxHP;
		}
        hasMoved = false;
    }
	#region anim
	float moveLerpMax;
    float moveLerpCurrent;
    Vector3 moveLerpTarget;
    Vector3 moveLerpOrigin;

    float attackLerpMax;
    float attackLerpCurrent;
    Vector3 attackLerpTarget;
    float attackLerpThresh = 0.45f;
    public void moveToLoc(float time)
	{
        moveLerpMax = time;
        moveLerpCurrent = time;
        moveLerpOrigin = transform.position;
        //moveLerpTarget = new Vector3(loc.transform.position.x, transform.position.y, loc.transform.position.z);
        moveLerpTarget = loc.positionOcc;
    }
    public void attackInDir(Tile t, float time)
	{
        moveLerpCurrent = 0;
        attackLerpMax = time;
        attackLerpCurrent = time;
        Vector3 diff = (t.transform.position - loc.positionOcc);
        diff.y = 0;
        attackLerpTarget = diff.normalized *0.5f;
	}

    #endregion
    #region buffs
    public List<GameObject> aurasPre;
    List<Buff> buffs = new List<Buff>();
    Dictionary<Aura,Buff> aurasRecieved = new Dictionary<Aura, Buff>();
    [HideInInspector]
    public List<Aura> aurasEmitted = new List<Aura>();

    
    List<Status.Effects> effects = new List<Status.Effects>();
    public void addBuff(Buff b)
	{
        b.initailize(this);
        //Debug.Log(b);
        buffs.Add(b);
        b.setTerminate(removeBuff);

		if (b.GetComponent<BuffStat>())
		{
            StatHandler buffStats = b.GetComponent<StatHandler>();
            Dictionary<StatType, float> buffDict = buffStats.export();
            foreach (StatType t in buffDict.Keys)
            {
                if (t == StatType.health)
                {
                    currentHealth += Mathf.FloorToInt(buffDict[t]);
                }
                //else if (t == StatType.moveSpeed)
                //{
                //	currentMovement += Mathf.FloorToInt(buffDict[t]);
                //}
            }

            st.addUpstream(buffStats);
            checkAlive();
        }
        Aura au = b.GetComponent<Aura>();
        if (au)
		{
            aurasEmitted.Add(au);
            au.updateLocation(loc);
        }
        BuffStatus eff = b.GetComponent<BuffStatus>();
        if (eff)
        {
            effects.Add(eff.effects);
            refreshStatus();
        }
        //Debug.Log(st.getStat(StatType.attack));
    }
    [Server]
    public void removeBuff(Buff b)
    {
        buffs.Remove(b);
        if (b.GetComponent<BuffStat>())
        {

            StatHandler buffStats = b.GetComponent<StatHandler>();
            Dictionary<StatType, float> buffDict = buffStats.export();
            foreach (StatType t in buffDict.Keys)
            {
                if (t == StatType.health)
                {
                    currentHealth -= Mathf.FloorToInt(buffDict[t]);
                }
                //else if (t == StatType.moveSpeed)
                //{
                //    currentMovement -= Mathf.FloorToInt(buffDict[t]);
                //}
            }

            buffStats.removeDownstream(st);
            checkAlive();
        }
        Aura au = b.GetComponent<Aura>();
        if (au)
        {
            aurasEmitted.Remove(au);
        }
        BuffStatus eff = b.GetComponent<BuffStatus>();
		if (eff)
		{
            effects.Remove(eff.effects);
            refreshStatus();
        }

    }
    public int equipmentCount()
	{
        int count = 0;
        foreach(Buff b in buffs)
		{
			if (b.GetComponent<Buff>().isEquipment)
			{
                count += 1;
			}
		}

        return count;
	}
    void refreshStatus()
	{
        status = Status.getEffects(effects);
	}
    public void addAura(Aura a)
	{
		if (!aurasRecieved.ContainsKey(a))
		{
            aurasRecieved.Add(a, tryEnterAura(a));
        }
        
	}
    public void removeAura(Aura a)
    {
		if (aurasRecieved[a])
		{
            gm.delayedDestroy(aurasRecieved[a].gameObject);
		}
        aurasRecieved.Remove(a);
    }
    public void updateAuras(List<Aura> newAuras)
    {

        List<Aura> toRemove = new List<Aura>();
        List<Aura> newToUnit = new List<Aura>(newAuras);
        foreach (Aura a in aurasRecieved.Keys)
        {
            if (!newAuras.Contains(a))
            {
                toRemove.Add(a);
            }
            else
            {
                newToUnit.Remove(a);
            }
        }
        foreach (Aura a in toRemove)
        {
            removeAura(a);
        }
        foreach (Aura a in newToUnit)
        {
            addAura(a);
        }

        
    }
    Buff tryEnterAura(Aura a)
	{

        return a.tryEnterAura(loc, this);
	}
    void spawnAuras()
	{
        foreach(GameObject aura in aurasPre)
		{
            createAura(aura);
		}
	}

    public void createAura(GameObject o)
    {
        GameObject au = Instantiate(o, transform);
        Aura aura = au.GetComponent<Aura>();
        aura.setTeam(teamIndex);
        addBuff(aura);
        NetworkServer.Spawn(au);
        aura.RpcAssignParent(netId);

    }
    public void moveAuras()
	{
        foreach(Aura a in aurasEmitted)
		{
            a.updateLocation(loc);
		}
	}

    #endregion
    #region abilities
    public List<GameObject> abilitiesPre;
    [HideInInspector]
    public List<AbilityRoot> abilities = new List<AbilityRoot>();

    public List<AbilityRoot> castable
	{
        get {
            return abilities.Where(ab => ab.trigger == AbilityRoot.TriggerType.none).ToList();
		}
	}
    [Server]
    void spawnAbilitites()
	{
        foreach(GameObject o in abilitiesPre)
		{
            createAbility(o);
		}
	}
    public GameObject createAbility(GameObject o)
	{
        GameObject ab = Instantiate(o, transform);
        AbilityRoot ord = ab.GetComponent<AbilityRoot>();
        ord.provideName(o.name);
        ord.caster = this;
        Ability abil = ab.GetComponent<Ability>();
        abil.initialize();
        NetworkIdentity netI = ab.GetComponent<NetworkIdentity>();
        abilities.Add(ord);
        NetworkServer.Spawn(ab);
        //TODO abilities of units on board will not be registered
        RpcParentAbility(netI.netId);
        return ab;
    }
    [ClientRpc]
    void RpcParentAbility(uint abID)
	{
		if (NetworkIdentity.spawned.ContainsKey(abID))
		{
            NetworkIdentity ab = NetworkIdentity.spawned[abID];
            ab.transform.parent = transform;
            ab.transform.localPosition = Vector3.zero;
            AbilityRoot o = ab.GetComponent<AbilityRoot>();
            o.caster = this;
            if (!isServer)
			{
                abilities.Add(o);
            }
			if (loc)
			{
                loc.refeshUI();
            }
            
        }
	}
    public void removeAbility(AbilityRoot o)
	{
        abilities.Remove(o);

	}
    [Server]
    void eventAbilitites(AbilityRoot.TriggerType t, Tile target)
	{
        foreach(AbilityRoot ab in abilities)
		{
            if(ab.trigger == t)
			{
                ab.eventAbil(target, teamIndex, loc);
			}
		}
	}
    #endregion

    // Update is called once per frame
    void Update()
    {
		if (isServerOnly)
		{
			if (loc)
			{
                transform.position = loc.positionOcc;

            }

            return;
        }

		if (attackLerpCurrent > 0)
		{
            attackLerpCurrent -= Time.deltaTime;
            float prg = attackLerpMax - attackLerpCurrent;
            Vector3 start = loc.positionOcc;
            Vector3 backTar = start - attackLerpTarget;
            Vector3 frontTar = start + attackLerpTarget;

            if (prg < attackLerpThresh)
			{                
                transform.position = Vector3.LerpUnclamped(start, backTar, prg / attackLerpThresh);
            }
            else if (attackLerpCurrent < attackLerpThresh)
			{
                
                transform.position = Vector3.LerpUnclamped(start, frontTar, attackLerpCurrent / attackLerpThresh);
            }
			else
			{
                transform.position = Vector3.LerpUnclamped(backTar, frontTar, (prg-attackLerpThresh) / (1-(2*attackLerpThresh)));
            }

        }
		else if (moveLerpCurrent > 0)
		{
            moveLerpCurrent -= Time.deltaTime;
            float prg = moveLerpMax - moveLerpCurrent;
            transform.position = Vector3.LerpUnclamped(moveLerpOrigin, moveLerpTarget, prg / moveLerpMax);
		}
    }
	#region getNset
	public StatHandler stat
	{
		get
		{
			//return st;
			if (st)
			{
				return st;
			}
			else
			{
				st = GetComponent<StatHandler>();
				return st;
			}

		}
	}
    public float height
	{
		get
		{
            float h = GetComponent<Collider>().bounds.extents.y;
            if(type == unitType.flying)
			{
                h += 0.3f;
			}
            return h;
   //         if (col)
			//{
   //             return col.bounds.extents.y;
   //         }
			//else
			//{
   //             col = GetComponent<Collider>();
   //             return col.bounds.extents.y;

   //         }
            
		}
	}
    public int moveRemaining
	{
		get
		{
            return currentMovement;
		}
	}
    public bool canMove
	{
		get
		{
            return alive && status.movement;
		}
	}
    public bool canMoveVisual
    {
        get
        {
            return currentMovement > 0  && canMove && gm && gm.whosTurn == teamIndex;
        }
    }
    public bool canAttack
	{
		get
		{
            return alive && currentAttacks > 0 && st.getStat(StatType.attack) > 0 && status.attacking;
        }
	}
    public bool canAttackVisual
    {
        get
        {
            return canAttack && gm && gm.whosTurn == teamIndex;
        }
    }
    public bool canCast
    {
        get
        {
            return alive && currentCasts > 0 && status.casting;
        }
    }
    public bool canCastVisual
    {
        get
        {
            return canCast && castable.Count>0 && gm && gm.whosTurn == teamIndex;
        }
    }
    public bool isDamaged
	{
		get
		{
            return currentHealth < st.getStat(StatType.health);
		}
	}
    public bool isAlive
	{
		get
		{
            return alive;
		}
	}
    public int getHeath()
    {
        return currentHealth;
    }
    public int getMove()
    {
        return currentMovement;
    }
    public int getMoveActionable()
	{
        return canMove ? currentMovement : 0;
	}
    public Status.Effects getStatus()
	{
        return status;
	}
    public int getTeam()
    {
        return teamIndex;
    }
    public bool isStructure {
		get
		{
            return type == unitType.structure;
		}
    }
    #endregion
    #region server Actions
    [Server]
    public void cast()
    {
        currentCasts -= 1;
        if (hasMoved && !st.getBool(StatType.agile))
        {
            currentMovement = 0;
        }
    }
    [Server]
    public void move(int dist)
	{
        currentMovement-=dist ;
        hasMoved = true;
	}
    [Server]
    public void attack(Unit tar, int range, bool didBypass)
	{
        currentAttacks--; 
		if (hasMoved && !st.getBool(StatType.agile))
		{
            currentMovement= 0;
		}
        int remainingHP = dealDamage(tar);
        if(remainingHP <= 0 && st.getBool(StatType.bloodlust))
		{
            currentAttacks++;
		}
        tar.getSlowed(st.getStat(StatType.slow));
		if (st.getBool(StatType.cleave))
		{
            tryCleave(tar);
		}
        //Retal OFF
        //tar.tryRetaliation(this, range, didBypass);
	}
    void tryCleave(Unit tar)
	{
        foreach(Tile t in tar.loc.tilesSideways((tar.loc.transform.position - loc.transform.position).normalized))
		{
            //Debug.Log(t);
			if (t.getOccupant() && t.getOccupant().teamIndex != teamIndex)
			{
                t.getOccupant().takeDamage(st.getStat(StatType.cleave), st.getBool(StatType.piercing), damageSource.attack);
			}
		}
	}
    public enum damageSource
	{
        attack,
        retaliation,
        ability
	}
    int dealDamage(Unit tar)
	{
        return tar.takeDamage(st.getStat(StatType.attack),st.getBool(StatType.piercing), damageSource.attack);
	}
    public void getSlowed(int slow)
	{
        currentMovement -= slow;
        if(currentMovement < 0)
		{
            currentMovement = 0;
		}
	}
    public void addAttacks(int attacks)
	{
        currentAttacks += attacks;

    }
 //   void tryRetaliation(Unit tar, int range, bool didBypass)
	//{
 //       int maxRange = st.getStat(StatType.range);
	//	if (range < 1)
	//	{
 //           range = 1;
	//	}
 //       if (maxRange >= range && (!didBypass || st.getBool(StatType.bypass))){
 //           dealRetailation(tar);
 //       }
        
	//}
 //   void dealRetailation(Unit tar)
 //   {
 //       tar.takeDamage(st.getStat(StatType.attack), st.getBool(StatType.piercing), damageSource.retaliation);
 //   }
    [Server]
    public int takeDamage(int d,bool piercing, damageSource type)
	{
		if (status.damage)
		{
            if (!piercing/*!source.getBool(StatType.piercing)*/)
            {
                d -= st.getStat(StatType.armor);

            }
            if (type == damageSource.retaliation)
            {
                d -= st.getStat(StatType.overwhelm);
            }
            if (d < 0)
            {
                d = 0;
            }
            currentHealth -= d;
        }
		
        checkAlive();
        return currentHealth;
		//else
		//{
  //          loc.refeshUI();
		//}
	}
    void checkAlive()
	{
        if (currentHealth <= 0)
        {
            //Destroy(gameObject);
            killSelf();
        }
    }
    bool alive = true;
    public void killSelf()
	{
		if (alive)
		{
            alive = false;
            gm.delayedDestroy(gameObject);
            eventAbilitites(AbilityRoot.TriggerType.onDeath, loc);
            
            
        }
        
    }
    public void heal(int h)
	{
        currentHealth += h;
        int max = st.getStat(StatType.health);
        if (currentHealth > max)
		{
            currentHealth = max;

        }
	}
    public void setHealth(int h)
    {
        currentHealth = h;

    }
    public void changeMovement(int m)
    {
        currentMovement += m;
        
        if (currentMovement < 0)
        {
            currentMovement = 0;

        }
    }
    #endregion

    public void PDestroy(bool isSev)
	{
		if (loc)
		{
            loc.occExit();
        }
		if (!isSev)
		{
            //TODO error here (When the unit dies in the same action it is created, such as placing it in a choking cloud)
            st.removeRefresh(reUI);
		}
        visibility(visType.on, visType.off);
	}
	//   public override void OnStopClient()
	//{

	//       if (isClientOnly)
	//       {
	//           st.removeRefresh(reUI);

	//           //temp
	//           Destroy(gameObject);
	//       }
	//}
	#region cardmaker
	public override GameObject findCardPrefab()
	{
        return (GameObject)Resources.Load("DynamicUnitCard", typeof(GameObject));

    }
    public override GameObject findCardTemplate()
    {
        return (GameObject)Resources.Load("UnitCardPre", typeof(GameObject));

    }
    public override Color getColor()
    {
		if (isStructure)
		{
            return GameColors.structure;
		}
        return Color.white;

    }
    protected override int getOrderType()
	{
        if (isStructure)
        {
            return 4;
        }
        return 1;
    }

	public override void modifyCardAfterCreation(GameObject o)
	{
        UnitCard card = o.GetComponent<UnitCard>();
        card.setCardmaker(this);
        card.costsMaterial = isStructure;
        Dictionary<StatType,float> tempS = GetComponent<StatHandler>().prefabStats();
        bool frontline = tempS.ContainsKey(StatType.frontline) && tempS[StatType.frontline] > 0;
        if (isStructure || frontline)
		{
            Targeting tar = o.GetComponent<Targeting>();
            Rule[] mod = new Rule[tar.rules.Length];
            for(int i =0; i<tar.rules.Length; i++)
			{
                mod[i] = tar.rules[i];
                if(mod[i].type == TargetRule.isDeployable)
				{
                    mod[i].value = (int)DeplolableRules.frontline; //Can Deploy outside of home row
				}
			}

            tar.rules = mod;
        }
		if (isStructure)
		{
            Targeting tar = o.GetComponent<Targeting>();
            Rule[] mod = new Rule[tar.rules.Length + 1];
            for (int i = 0; i < tar.rules.Length; i++)
            {
                mod[i] = tar.rules[i];
                
            }
            //Adds the foundation requirements to the structure placing
            int addOn = tempS.ContainsKey(StatType.addOn) ? (int)tempS[StatType.addOn] : 0;
            Rule found = new Rule(TargetRule.foundation, false, 1 + addOn);
            mod[mod.Length - 1] = found;
            tar.rules = mod;


            //Sets the target hand when spawning to be the sideboard -MOVED TO playerGhost
            //card.targetHand = "StructureBoard";
        }
	}
	#endregion
}
