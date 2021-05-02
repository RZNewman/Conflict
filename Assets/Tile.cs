using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Tile : NetworkBehaviour, TeamOwnership
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
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 0),
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
            RpcAssignUnit(u.netId);
            assignUnit(u);
            
        }
    }
    public void assignUnit(Unit u)
    {
        occEnter(u);
        u.teamColor();
        u.visibility(true);
        alignOcc();
        //TODO WTF is all this shit
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
    float tileHeight;
    TileUI unitUI;
    public bool isWalk = true;
    public bool isSight = true;
    public int teamDeploy = -1;
    public bool isFoundation = false;
    
    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
        tileHeight = col.bounds.extents.y;
        unitUI = transform.GetChild(0).GetComponent<TileUI>();
        //Debug.Log(unitUI);
        fillPresets();
        colorSelf();
    }


    Dictionary<Color, Material> presets
    {
		get
		{
			if (isFoundation)
			{
                return presetsFound;
			}
			else
			{
                return presetsBase;
			}
		}

    }

    static Dictionary<Color, Material> presetsBase;
    static Dictionary<Color, Material> presetsFound;


    public Material baseMat;
    public Material foundationMat;

    public void fillPresets()
	{
        if(presetsBase == null)
		{
            presetsBase = new Dictionary<Color, Material>();

        }
        if (presetsFound == null)
        {
            presetsFound = new Dictionary<Color, Material>();

        }
        //baseColor = GetComponent<MeshRenderer>().sharedMaterial;
        tryAddPreset(Color.white);
        tryAddPreset(GameColors.smokescreen);
        tryAddPreset(GameColors.barricade);
        tryAddPreset(GameColors.deployfield);
        tryAddPreset(GameColors.structure);
        //colorPresets();

        

    }
    void tryAddPreset(Color c)
	{
		if (!presetsBase.ContainsKey(c))
		{
            //Debug.Log(presets);
            presetsBase.Add(c, new Material(baseMat));
            presetsBase[c].color = c;
            //Debug.Log("added");
        }
        else if (!presetsBase[c])
		{
            presetsBase[c] = new Material(baseMat);
            presetsBase[c].color = c;
        }
        if (!presetsFound.ContainsKey(c))
        {
            //Debug.Log(presets);
            presetsFound.Add(c, new Material(foundationMat));
            presetsFound[c].color = c;
            //Debug.Log("added");
        }
        else if (!presetsFound[c])
        {
            presetsFound[c] = new Material(foundationMat);
            presetsFound[c].color = c;
        }

    }


    public void colorSelf(bool callEditor =false)
	{

        fillPresets();

        if (!isSight)
        {
            //GetComponent<MeshRenderer>().material.color = GameColors.smokescreen;
            GetComponent<MeshRenderer>().material = presets[GameColors.smokescreen];
        }
        else if (!isWalk)
		{
            //GetComponent<MeshRenderer>().material.color = GameColors.barricade;
            GetComponent<MeshRenderer>().material = presets[GameColors.barricade];
        }
        else if (teamDeploy != -1)
        {
            //GetComponent<MeshRenderer>().material.color = GameColors.deployfield;
            GetComponent<MeshRenderer>().material = presets[GameColors.deployfield];
        }

		else
		{
            //GetComponent<MeshRenderer>().material.color = Color.white;
            GetComponent<MeshRenderer>().material = presets[Color.white];

        }


        float localY = 0.2f;
		if (!isWalk)
		{
            localY = 0.6f;
        }
        transform.localScale = new Vector3(1, localY, 1);

		if (!callEditor)
		{
            unitUI.gameObject.transform.localScale = new Vector3(1, 1 / localY, 1);

        }
    }


    // Update is called once per frame
    void Update()
    {
    }
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

	public int getTeam()
	{
        return teamDeploy;
	}
    #region tile Searching
    public List<Tile> select()
    {
        List<Tile> selected = new List<Tile>();
        selected.Add(this);
        unitUI.select(TileUI.SelectType.active);

		if (occupant)
		{
            List<Tile> moveSelect = tilesInMove(occupant.getMove(), !(occupant.type == Unit.unitType.flying), occupant.teamIndex,occupant.isStructure,occupant.stat.getBool(StatBlock.StatType.ghost));
            foreach (Tile t in moveSelect)
            {
                t.unitUI.select(TileUI.SelectType.move);
            }
            selected.AddRange(moveSelect);

			if (occupant.canAttack)
			{
                List<Tile> attackSelect = tilesInRange(occupant.stat.getStat(StatBlock.StatType.range), occupant.stat.getBool(StatBlock.StatType.bypass), occupant.teamIndex);
                foreach (Tile t in attackSelect)
                {
                    t.unitUI.select(TileUI.SelectType.attack);
                }
                selected.AddRange(attackSelect);

            }

		}
        return selected;
    }
    public void selectAbility()
	{
        unitUI.select(TileUI.SelectType.ability);
	}
    public void deselect()
	{
        unitUI.deselect();
	}
    List<Tile> tilesInMove(int dist, bool walking, int team, bool isStructure, bool ghost)
	{
        List<Tile> selected = new List<Tile>();
        Queue<Tile> search = new Queue<Tile>();
        Dictionary<Tile, int> found = new Dictionary<Tile, int>();
        search.Enqueue(this);
        found.Add(this, 0);

        while (search.Count > 0)
        {
            Tile t = search.Dequeue();
            if ((!walking || t.isWalk) && !t.getOccupant() && found[t] <= dist && (!isStructure || t.isFoundation))
			{
                selected.Add(t);
			}
            if ((!walking || t.isWalk) && (!t.getOccupant() || t.getOccupant().teamIndex == team || ghost) && found[t] < dist && (!isStructure || t.isFoundation))
            {
                foreach (Tile n in t.neigh)
                {
                    if (n && !found.ContainsKey(n))
                    {
                        found.Add(n, found[t] + 1);
                        search.Enqueue(n);
                    }
                }
            }

        }
        //selected.Remove(this);
        
        return selected;
    }
    public int distToTile(Tile target, bool walking, int team, bool ghost)
	{
        //TODO A*
        Queue<Tile> search = new Queue<Tile>();
        Dictionary<Tile, int> found = new Dictionary<Tile, int>();
        search.Enqueue(this);
        found.Add(this, 0);
		while (search.Count > 0)
		{
            Tile t = search.Dequeue();
            if(t == target)
			{
                return found[t];
			}
			if ((!walking || t.isWalk) && (!t.getOccupant() || t.getOccupant().teamIndex == team || ghost) )
			{
                foreach (Tile n in t.neigh)
                {
                    if (n && !found.ContainsKey(n))
                    {
                        found.Add(n, found[t] + 1);
                        search.Enqueue(n);
                    }
                }
            }

		}

        return -1;
	}
    List<Tile> tilesInRange(int range, bool bypass, int team)
	{
        List<Tile> selected = new List<Tile>();
        for(int dirInd = 0; dirInd < neigh.Length; dirInd++)
		{
            Tile currentTile = neigh[dirInd];
            int currentRange = 1;
            while (currentTile != null && currentRange <= range)
            {
                //Debug.Log(currentTile);
                if (currentTile.getOccupant() && currentTile.getOccupant().teamIndex != team)
                {
                    selected.Add(currentTile);
                    break;
                }
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
