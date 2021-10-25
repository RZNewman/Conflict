using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static SelectorUI;
using UnityEditor;
using static Unit;
using Priority_Queue;

public class Tile : NetworkBehaviour
{
	#region neigh
	public enum neighDir
    {
        up,
        down,
        right,
        left,
    }
    static Vector3[] neighVec = {
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 0),
        new Vector3(0, 0, -1),
        new Vector3(-1, 0, 0),
    };
    Tile[] neigh = new Tile[4];
    public void link()
    {
        for (int i = 0; i < neigh.Length; i++)
        {
            RaycastHit hit;
            Vector3 gridPos = transform.position;
            gridPos.y = 0;
            if (Physics.Raycast(gridPos + neighVec[i] + Vector3.down, Vector3.up, out hit, 2, LayerMask.GetMask("Tiles")))
            {
                neigh[i] = hit.transform.GetComponent<Tile>();
            }
        }
    }
    public bool isNeighbor(Tile t)
	{
        foreach(Tile n in neigh)
	    {
            if(t == n)
		    {
                return true;
		    }
	    }
        return false;
	}
    public List<Tile> getNeightbors()
	{
        List<Tile> ns = new List<Tile>();
        foreach (Tile n in neigh)
        {
			if (n)
			{
                ns.Add(n);
			}
        }
        return ns;
    }
    
    int dirIndex(Tile other)
	{
        Vector3 dir = other.transform.position-transform.position;
        dir.y = 0;
        dir.Normalize();
        int index = -1;
        for (int i = 0; i < neighVec.Length; i++)
        {

            if (dir == neighVec[i])
            {
                index = i;
            }
        }

        return index;
    }
    #endregion
    #region unitInit
    
    public Vector3 positionOcc
	{
		get
		{
            if (occupant)
            {
                return transform.position + transform.up * occupant.height + transform.up * tileHeight;
            }
            return transform.position;
        }
	}
    public void findUnit()
	{
        RaycastHit hit;
        Vector3 gridPos = transform.position;
        gridPos.y = 0;
        if (Physics.Raycast(gridPos + Vector3.down, Vector3.up, out hit, 3, LayerMask.GetMask("Units")))
        {
            Unit u = hit.collider.GetComponent<Unit>();
            u.initialize(u.teamIndex);
            u.provideName("");
            RpcAssignUnit(u.netId);
            assignUnit(u);
            
        }
    }
    public void assignUnit(Unit u)
    {
        occEnter(u);
        u.teamColor();
        u.visibility(Unit.visType.none, Unit.visType.on);
        u.visibility(Unit.visType.off, Unit.visType.on);
        alignOcc();
		if (isServer)
		{
            u.lateInit();
		}
    }
    public void alignOcc()
    {
        if (occupant)
        {
            occupant.transform.position = positionOcc;
        }
    }
    [ClientRpc]
    public void RpcAssignUnit(uint unitID)
	{
        Unit u = NetworkIdentity.spawned[unitID].GetComponent<Unit>();
        //Debug.Log("assingd");
        assignUnit(u);

    }

    
    #endregion
    Unit occupant;
    Collider col;

    float baseHeight;
    TileUI unitUI;
    public int teamDeploy = -1;
    public bool isFoundation = false;
    
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        baseHeight = col.bounds.extents.y;
        unitUI = transform.GetChild(0).GetComponent<TileUI>();
        //Debug.Log(unitUI);
        //colorSelf();
    }

    float tileHeight
	{
		get
		{
            return baseHeight + terrainHeight;
		}
	}

    List<Aura> auras = new List<Aura>();

    public void addAura(Aura a)
	{
        auras.Add(a);
        if (occupant)
        {
            occupant.addAura(a);
        }
    }

    public void removeAura(Aura a)
    {
        auras.Remove(a);
        if (occupant)
        {
            occupant.removeAura(a);
        }
    }

    void checkAuras()
	{
        if (occupant)
        {
            occupant.updateAuras(auras);
        }
    }



	#region terrian
    public enum terrainType
	{
        normal,
        mountain,
        wall,
        smoke,
        hill,
        forest,
        launchpad
	}
    //TODO Rpc visuals
    [SyncVar]
    public terrainType type;
    terrainType instancedType= terrainType.normal;
    GameObject terrrainObj;
	


	public Material baseMat;
    public Material foundationMat;
    public GameObject tileObj;
    public GameObject moutainPre;
    public GameObject wallPre;
    public GameObject smokePre;
    public GameObject hillPre;
    public GameObject forestPre;
    public GameObject launchpadPre;


    public void checkTypeVis(bool callEditor =false)
	{
		if (type != instancedType)
		{
            instancedType = type;
            List<GameObject> tObjs = getVisuals();
            if (tObjs.Count > 0)
			{
                foreach(GameObject o in tObjs)
				{
                    if (callEditor)
                    {
                        DestroyImmediate(o);
                    }
                    else
                    {
                        Destroy(o);
                    }
                }
                
                
			}
            GameObject pre = getTypePre();
			if (pre)
			{
                //if (callEditor)
                //{
                //    tileObj = Instantiate(pre,transform.position,Quaternion.identity);
                //}
                //else
                //{
                //    tileObj = Instantiate(pre, gameObject.transform);
                //}
                terrrainObj = Instantiate(pre, gameObject.transform);


            }
			else
			{
                terrrainObj = null;

            }
            
		}
	}

	public void resetState()
	{
        instancedType = terrainType.normal;
	}
    
    List<GameObject> getVisuals()
	{
        List<GameObject> children = new List<GameObject>();
        foreach (Transform t in transform)
		{
            if (t.tag == "TileVis"){
                children.Add(t.gameObject);
			}
		}
        return children;
	}



	GameObject getTypePre()
	{
		switch (type)
		{
            case terrainType.mountain:
                return moutainPre;
            case terrainType.smoke:
                return smokePre;
            case terrainType.wall:
                return wallPre;
            case terrainType.hill:
                return hillPre;
            case terrainType.forest:
                return forestPre;
            case terrainType.launchpad:
                return launchpadPre;
            default:
                return null;
		}
	}
    public int getTerrainWalkCost(unitType u)
    {
        switch (type, u)
        {
            case (terrainType.normal, var _):
                return 1;
            case (terrainType.smoke, var _):
                return 1;
            case (terrainType.launchpad, var _):
                return 1;     
            case (terrainType.hill, unitType.flying):
                return 1;
            case (terrainType.hill, unitType.light):
                return 2;
            case (terrainType.hill, unitType.heavy):
                return 3;
            case (terrainType.forest, unitType.flying):
                return 1;
            case (terrainType.forest, unitType.light):
                return 2;
            case (terrainType.forest, unitType.heavy):
                return 3;
            case (terrainType.mountain, unitType.flying):
                return 1;
            case (terrainType.wall, unitType.flying):
                return 1;
            default:
                return -1;
        }
    }
    float terrainHeight
	{
		get
		{
			switch (type)
			{
                case terrainType.mountain:
                    return 0.5f;
                case terrainType.wall:
                    return 0.3f;
                case terrainType.hill:
                    return 0.3f;
                default:
                    return 0;
			}
		}
	}


 //   public bool isWalk
	//{
	//	get
	//	{
 //           switch (type){
 //               case terrainType.wall:
 //               case terrainType.mountain:
 //                   return false;
 //               default:
 //                   return true;
	//		}
	//	}
	//}
    public bool isSight
    {
        get
        {
            switch (type)
            {
                case terrainType.smoke:
                case terrainType.mountain:
                    return false;
                default:
                    return true;
            }
        }
    }
    #endregion

    public Unit getOccupant()
	{
        return occupant;
	}

    public void occExit()
	{
        occupant = null;
        unitUI.deactivate();

        
	}
    public void occEnter(Unit o)
	{
        
        //Debug.Log(this);
        occupant = o;
        occupant.loc = this;
        //Debug.Log(occupant);
        //Debug.Log(unitUI);
        unitUI.activate(occupant);

		if (isServer)
		{
            o.moveAuras();
            checkAuras();
        }
        
        
    }
    
    public void refeshUI()
	{
        unitUI.refresh();
	}
    public void setUIDir(Quaternion rot)
	{
        unitUI.transform.localRotation = rot;
	}

    
	private void OnDestroy()
	{
		if (occupant)
		{
            occupant.loc = null;
		}
	}

    #region tile Searching
    public List<GameObject> select(bool isHover)
    {
        List<GameObject> selected = new List<GameObject>();
        List<Tile> searched = new List<Tile>();
		if (!isHover)
		{
            selected.Add(unitUI.select(SelectType.active,isHover));
            
        }
        searched.Add(this);
        

		if (occupant)
		{
            List<Tile> moveSelect = tilesInMove(occupant.getMoveActionable(), occupant.type , occupant.teamIndex,occupant.stat.getBool(StatBlock.StatType.ghost));
            foreach (Tile t in moveSelect)
            {
                selected.Add(t.unitUI.select(SelectType.move,isHover));
            }
            searched.AddRange(moveSelect);

			if (occupant.canAttack)
			{
                List<Tile> attackSelect = tilesInAttack(occupant.stat.getStat(StatBlock.StatType.range), occupant.stat.getBool(StatBlock.StatType.bypass), occupant.teamIndex);
                foreach (Tile t in attackSelect)
                {
                    selected.Add(t.unitUI.select(SelectType.attack, isHover));
                }
                searched.AddRange(attackSelect);

                void threatFind(Tile t)
				{
                    List<Tile> threat = t.tilesInRange(occupant.stat.getStat(StatBlock.StatType.range), occupant.stat.getBool(StatBlock.StatType.bypass));

                    foreach (Tile t2 in threat)
                    {
                        if (!searched.Contains(t2))
                        {
                            
                            selected.Add(t2.unitUI.select(SelectType.threat,isHover));
                            searched.Add(t2);
                        }
                    }
                }

                threatFind(this);
                foreach (Tile t in moveSelect)
                {
                    threatFind(t);
                }

            }

		}
        return selected;
    }
    public GameObject selectAbility()
	{
        return unitUI.select(SelectType.ability, false);
	}
 //   public void deselect()
	//{
 //       unitUI.deselect();
	//}
    List<Tile> tilesInMove(int dist, unitType uType, int team, bool ghost)
	{
        List<Tile> selected = new List<Tile>();
        Queue<Tile> search = new Queue<Tile>();
        Dictionary<Tile, int> found = new Dictionary<Tile, int>();
        search.Enqueue(this);
        found.Add(this, 0);

        while (search.Count > 0)
        {
            Tile t = search.Dequeue();
            if (
                !t.getOccupant() 
                && found[t] <= dist 
                && (!(uType == unitType.structure) || t.isFoundation)
                )
			{
                selected.Add(t);
			}
            if (
                (!t.getOccupant() || t.getOccupant().teamIndex == team || ghost) 
                && (!(uType == unitType.structure) || t.isFoundation)
                )
            {
                foreach (Tile n in t.neigh)
                {
                    if (n)
                    {
                        int connectionDistance = n.getTerrainWalkCost(uType);
                        int neighDist = found[t] + connectionDistance;
                        bool canProgress = connectionDistance != -1 && neighDist <= dist;
						if (canProgress)
						{
                            if (!found.ContainsKey(n))
                            {
                                search.Enqueue(n);
                                found.Add(n, neighDist);
                            }
							else
							{
                                if(found[n] > neighDist)
								{
                                    found[n] = neighDist;
									if (!search.Contains(n))
									{
                                        search.Enqueue(n);
									}
								}
							}


                            
                        }
                        
                        
                    }
                }
            }

        }
        //selected.Remove(this);
        
        return selected;
    }
    public int distToTile(Tile target, unitType uType, int team, bool ghost)
	{
		if (this == target)
		{
            return 0;
		}
        SimplePriorityQueue<Tile, int> search = new SimplePriorityQueue<Tile, int>();
        Dictionary<Tile, int> found = new Dictionary<Tile, int>();
        int h(Tile t)
		{
            return Mathf.RoundToInt(
                Mathf.Abs( t.transform.position.x - target.transform.position.x)
                +
                Mathf.Abs(t.transform.position.z - target.transform.position.z)
                );
		}
        int e(Tile t)
		{
            return found[t] + h(t);
		}

        //Queue<Tile> search = new Queue<Tile>();
        found.Add(this, 0);
        search.Enqueue(this,e(this));
        
		while (search.Count > 0)
		{
            Tile t = search.Dequeue();
			if (!t.getOccupant() || t.getOccupant().teamIndex == team || ghost) 
			{
                foreach (Tile n in t.neigh)
                {
                    if (n)
                    {
                        
                        int connectionDistance = n.getTerrainWalkCost(uType);
                        int neighDist = found[t] + connectionDistance;
                        bool canProgress = connectionDistance != -1;

                        if (n == target)
                        {
                            return canProgress? neighDist : -1;
                        }

                        if (canProgress)
                        {
                            if (!found.ContainsKey(n))
                            {
                                found.Add(n, neighDist);
                                search.Enqueue(n, e(n));
                                
                            }
                            else
                            {
                                if (found[n] > neighDist)
                                {
                                    found[n] = neighDist;
                                    if (!search.Contains(n))
                                    {
                                        search.Enqueue(n, e(n));
                                    }
									else
									{
                                        search.UpdatePriority(n, e(n));
									}
                                }
                            }



                        }
                    }
                }
            }

		}

        return -1;
	}
    List<Tile> tilesInAttack(int range, bool bypass, int team)
	{
        List<Tile> selected = new List<Tile>();
        foreach(Tile currentTile in tilesInRange(range, bypass))
		{
            if (currentTile.getOccupant() && currentTile.getOccupant().teamIndex != team)
            {
                selected.Add(currentTile);
                //break;
            }
        }

        return selected;
    }
    public List<Tile> tilesInRange(int range, bool bypass)
	{
        List<Tile> selected = new List<Tile>();
        for(int dirInd = 0; dirInd < neigh.Length; dirInd++)
		{
            Tile currentTile = neigh[dirInd];
            int currentRange = 1;
            while (currentTile != null && currentRange <= range)
            {
                selected.Add(currentTile);
                //Debug.Log(currentTile);

                if (!currentTile.isSight || currentTile.getOccupant())
                {
                    if (!bypass)
                    {
                        break;
                    }

                }
                currentTile = currentTile.neigh[dirInd];
                currentRange++;
            }
        }

        return selected;
    }
    public List<Tile> tilesSideways(Vector3 dir)
	{
        List<Tile> selected = new List<Tile>();

        int index = -1;
            
        for(int i=0; i<neighVec.Length; i++)
		{
            if( neighVec[i] == dir)
			{
                index = i;
                break;

			}
		}
        if(index == -1)
		{
            return selected;
		}
        //Debug.Log("Contains");
        int left = Operations.mod(index+1, neighVec.Length);
        int right = Operations.mod(index - 1, neighVec.Length);

        Tile test = neigh[left];
		if (test)
		{
            selected.Add(test);
        }
        test = neigh[right];
        if (test)
        {
            selected.Add(test);
        }

        return selected;
    }
    public int rangeToTile(Tile target, bool bypass, out bool didBypass)
	{
        didBypass = false;
        int index = dirIndex(target);
        //Debug.Log("Dir index" + index);
        if(index == -1)
		{
            return -1;
		}
        Tile currentTile = neigh[index];
        int currentRange = 1;
        while(currentTile != null) {
            //Debug.Log(currentTile);
            if(currentTile == target)
			{
                return currentRange;
			}
            if(currentTile.getOccupant() || !currentTile.isSight)
			{
				if (bypass)
				{
                    didBypass = true;
                }
                else
                {
                    return -1;
                }

            }
            currentTile = currentTile.neigh[index];
            currentRange++;
        }
        return -1;

	}
    public bool allyAdjacent(int team, bool includeStructures = false)
	{
        foreach(Tile t in neigh)
		{
            if(t && t.getOccupant() && t.getOccupant().teamIndex == team && (!t.getOccupant().isStructure || includeStructures))
			{
                return true;
			}
		}
        return false;
	}

    public List<Tile> tilesInArea(int area)
	{
        List<Tile> selected = new List<Tile>();
        RaycastHit[] hits = Physics.BoxCastAll(transform.position, new Vector3(1, 1, 1) * (area), Vector3.up, Quaternion.identity, 0, LayerMask.GetMask("Tiles"));
        //Debug.Log(hits.Length);
        foreach(RaycastHit hit in hits)
		{
            selected.Add(hit.collider.GetComponent<Tile>());
		}
        return selected;
    }
	#endregion
}
