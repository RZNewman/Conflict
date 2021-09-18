using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Buff : NetworkBehaviour, PseudoDestroy
{
    public List<GameObject> abilitiesPre;
    public List<GameObject> abilities = new List<GameObject>();

    [ClientRpc]
    public virtual void RpcAssignUnit(uint unitID)
    {
        GameObject u = NetworkIdentity.spawned[unitID].gameObject;
        //Debug.Log("assingd");
        transform.parent = u.transform;
        transform.localPosition = Vector3.zero;
    }
    public delegate void Termination(Buff b);
    void doNothing(Buff b)
	{
        return;
	}

    Termination removeBuff;


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
        
    }
    public void initailize(Unit u)
	{
        StatHandler bStats = GetComponent<StatHandler>();
        bStats.initialize();

        foreach (GameObject o in abilitiesPre)
        {
            abilities.Add(u.createAbility(o));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PDestroy()
	{
        removeBuff(this);
        foreach (GameObject o in abilities)
        {
            GameManager gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            gm.delayedDestroy(o);
        }
    }
}
