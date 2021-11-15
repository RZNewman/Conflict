using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class GameGrid : NetworkBehaviour
{
    Tile[] tiles = new Tile[0];
    Quaternion uiRot;
    bool rotSet = false;

    List<Foundation> foundations = new List<Foundation>();
    //SyncDictionary<uint, uint> tileOcc = new SyncDictionary<uint, uint>();
    //SyncDictionary<uint, uint> unitLoc = new SyncDictionary<uint, uint>();

    
    // Start is called before the first frame update
    public void initialize()
	{
        buildGrid();
        RpcGridAlign();
        getUnits();
    }
    void buildGrid()
	{
        findTiles();
        linkTiles();
        buildFoundations();
        alignUIDir();
    }

    [ClientRpc]
    void RpcGridAlign()
    {
		if (isServer)
		{
            return;
		}
        buildGrid();


    }

    public Tile[] allTiles
	{
		get
		{
            return tiles;
		}
	}
    void findTiles()
    {
        tiles = GetComponentsInChildren<Tile>(true);
    }
    void linkTiles()
    {
        foreach (Tile t in tiles)
        {
            t.link();
        }
    }
    public void setUIDir(Quaternion rot)
	{
        uiRot = rot;
        rotSet = true;
        alignUIDir();
    }
    void alignUIDir()
	{
        if(rotSet && tiles.Length > 0)
		{
            foreach (Tile t in tiles)
            {
                t.setUIDir(uiRot);
            }
        }
        

    }

	void getUnits()
	{
		foreach (Tile t in tiles)
		{
			t.findUnit();
		}
	}
    public List<Foundation> GetFoundations()
	{
        return foundations;
	}

    void buildFoundations()
	{
        Dictionary<Tile, List<Tile>> groupings = new Dictionary<Tile, List<Tile>>();
        Dictionary<Tile, Tile> ownership = new Dictionary<Tile, Tile>();
        List<Tile> search = new List<Tile>();
        foreach (Tile t in tiles)
        {
            if (t.isFoundation) {
                groupings[t] = new List<Tile>() { t };
                ownership[t] = t;
                search.Add(t);
			}
        }
        foreach(Tile t in search)
		{
            Tile n = t.firstFoundation();
			if (n)
			{

                Tile master = ownership[n];
                Tile fallen = ownership[t];

                if(master != fallen)
				{
                    foreach (Tile f in groupings[fallen])
                    {
                        ownership[f] = master;
                    }
                    groupings[master].AddRange(groupings[fallen]);
                    groupings.Remove(fallen);
                }
			}
		}
        foreach(List<Tile> foundation in groupings.Values)
		{
            foundations.Add(new Foundation(foundation));
		}

    }

	//bool initial = false;

}
