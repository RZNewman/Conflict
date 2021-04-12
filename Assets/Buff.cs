using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Buff : NetworkBehaviour
{
    public List<GameObject> abilitiesPre;
    List<GameObject> abilities = new List<GameObject>();

    [ClientRpc]
    public void RpcAssignUnit(uint unitID)
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



    public void dispell()
	{
        removeBuff(this);
        foreach(GameObject o in abilities)
		{
            Destroy(o);
		}
        Destroy(gameObject);
    }

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

	
}
