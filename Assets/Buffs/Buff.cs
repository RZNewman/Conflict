using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static Targeting;

public abstract class Buff : Cardmaker, PseudoDestroy, TeamOwnership
{
    public Unit.unitType type;
    // Start is called before the first frame update

    public int maxDuration = 0;
    protected int currentDuration;

    public bool isEquipment;

    protected GameManager gm;
    [SyncVar]
    protected int teamInd = -1;

    public GameObject visualsPre;
    protected GameObject visuals;

    //is null for area auras
    protected Unit carrier;

    [ClientRpc]
    public void RpcAssignParent(uint parentID)
    {
        
        
        GameObject u = NetworkIdentity.spawned[parentID].gameObject;
        //Debug.Log("assingd");
        transform.parent = u.transform;
        transform.localPosition = Vector3.zero;
		if (visualsPre)
        {
            visuals = new GameObject("Scaler");
            visuals.tag = "BuffScaler";
            visuals.transform.SetParent(u.transform);
            visuals.transform.localPosition = Vector3.zero;
            visuals.transform.localScale = Operations.VectorDiv( Vector3.one, u.transform.lossyScale);
            
            Instantiate(visualsPre, visuals.transform);
        }


        CallbackRPC(u);
    }

    protected virtual void CallbackRPC(GameObject u) { 
        //do nothing
    }
    public delegate void Termination(Buff b);
    void doNothing(Buff b)
	{
        return;
	}

    protected Termination removeBuff;


    public void setTerminate(Termination t)
	{
        removeBuff = t;
	}
   
	// Start is called before the first frame update
	void Start()
    {
        if(removeBuff == null)
		{
            removeBuff = doNothing;
        }
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    public void setTeam(int t)
	{
        teamInd = t;
	}
    public virtual void initailize(Unit u = null)
	{
        carrier = u;
        currentDuration = maxDuration;
    }

    public void tick()
	{
        CallbackTick();
        if (maxDuration > 0)
		{
            currentDuration--;
			if (currentDuration <= 0)
			{
                gm.delayedDestroy(gameObject);
			}
		}
	}

    public abstract string toDesc(bool isPrefab);
	

    protected string descSuffix(bool isPrefab)
	{
        string desc = "";
        int durr;
        if (isPrefab)
        {
            durr = maxDuration;
        }
        else
        {
            durr = currentDuration;
        }
        if (maxDuration > 0)
        {
            desc += " for " + durr + " round" + (durr > 1 ? "s" : "");
        }
        return desc;
    }
	public virtual void PDestroy(bool isSev)
    {
		if (isSev)
		{
            removeBuff(this);
            CallbackPD();
        }
		else
		{
            if (visuals)
            {
                Destroy(visuals);
            }
        }
        
		
        
    }
    protected virtual void CallbackPD()
	{
        //do nothing
	}
    protected virtual void CallbackTick()
    {
        //do nothing
    }

    [Client]
    public override void register() //prefab
    {
        if (!NetworkClient.prefabs.ContainsValue(gameObject))
        {
            NetworkClient.RegisterPrefab(gameObject);

        }
    }

	public int getTeam()
	{
		return teamInd;
	}



    public override GameObject findCardPrefab()
    {
        return (GameObject)Resources.Load("DynamicEquipCard", typeof(GameObject));
    }
    public override GameObject findCardTemplate()
    {
        return (GameObject)Resources.Load("EquipCardPre", typeof(GameObject));

    }
    public override Color getColor()
    {

        return GameColors.equipment;

    }
    protected override int getOrderType()
    {
        return 2;
    }

    public override void modifyCardAfterCreation(GameObject o)
    {
        EquipCard card = o.GetComponent<EquipCard>();
        card.setCardmaker(this);
        Targeting tar = o.GetComponent<Targeting>();
        Rule[] mod = new Rule[tar.rules.Length];
        for (int i = 0; i < tar.rules.Length; i++)
        {
            mod[i] = tar.rules[i];
            if (mod[i].type == TargetRule.unitType)
            {
                mod[i].value = (int)type;
            }
        }

        tar.rules = mod;
    }
}
