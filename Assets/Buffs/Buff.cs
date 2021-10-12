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
    int currentDuration;

    public bool isEquipment;

    protected GameManager gm;
    [SyncVar]
    protected int teamInd = -1;

    public GameObject visualsPre;
    protected GameObject visuals;

    [ClientRpc]
    public void RpcAssignParent(uint parentID)
    {
        
        
        GameObject u = NetworkIdentity.spawned[parentID].gameObject;
        //Debug.Log("assingd");
        transform.parent = u.transform;
        transform.localPosition = Vector3.zero;
		if (visualsPre)
		{
            visuals = Instantiate(visualsPre, u.transform);
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
        
        currentDuration = maxDuration;
    }

    public void tick()
	{
        if (maxDuration > 0)
		{
            currentDuration--;
			if (currentDuration <= 0)
			{
                gm.delayedDestroy(gameObject);
			}
		}
	}

    public abstract string toDesc();
	

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
