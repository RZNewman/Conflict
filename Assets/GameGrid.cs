using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class GameGrid : NetworkBehaviour
{
    Tile[] tiles = new Tile[0];
    Quaternion uiRot;
    bool rotSet = false;
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
        alignUIDir();
    }

    [ClientRpc]
    void RpcGridAlign()
    {

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

	//bool initial = false;
	//// Update is called once per frame
	//void Update()
	//{
	//    if (!initial && isServer)
	//    {
	//        getUnits();
	//        initial = true;
	//    }
	//}
}
