using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Buff : NetworkBehaviour, PseudoDestroy, TeamOwnership
{
    public List<GameObject> abilitiesPre;
    [HideInInspector]
    public List<GameObject> abilities = new List<GameObject>();

    public int maxDuration = 0;
    int currentDuration;

    protected GameManager gm;
    [SyncVar]
    protected int teamInd = -1;

    public GameObject visualsPre;
    protected GameObject visuals;

    [ClientRpc]
    public virtual void RpcAssignParent(uint parentID)
    {
        
        
        GameObject u = NetworkIdentity.spawned[parentID].gameObject;
        //Debug.Log("assingd");
        transform.parent = u.transform;
        transform.localPosition = Vector3.zero;
		if (visualsPre)
		{
            visuals = Instantiate(visualsPre, u.transform);
        }
        

        if (isServer)
        {
            return;
        }
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
    public void initailize(Unit u = null)
	{
        StatHandler bStats = GetComponent<StatHandler>();
        bStats.initialize();

		if (u)
		{
            foreach (GameObject o in abilitiesPre)
            {
                abilities.Add(u.createAbility(o));
            }
        }
        
        currentDuration = maxDuration;
    }

    public void tick()
	{
        Debug.Log(gameObject);
        Debug.Log(maxDuration);
        Debug.Log(currentDuration);
        if (maxDuration > 0)
		{
            currentDuration--;
			if (currentDuration <= 0)
			{
                gm.delayedDestroy(gameObject);
			}
		}
	}

    public virtual string toDesc()
	{
        string desc = CardUI.cardText(GetComponent<StatHandler>().prefabStats(), false).Replace('\n', ',');
        desc = "'" + desc + "'";
		if (maxDuration > 0)
		{
            desc += " for " + maxDuration + " round" +(maxDuration>1? "s":"");
		}
        return desc;

    }

	public virtual void PDestroy(bool isSev)
    {
		if (isSev)
		{
            removeBuff(this);
            foreach (GameObject o in abilities)
            {
                gm.delayedDestroy(o);
            }
        }
		else
		{
            if (visuals)
            {
                Destroy(visuals);
            }
        }
        
		
        
    }

	public int getTeam()
	{
		return teamInd;
	}
}
