using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static StatBlock;

public class Foundation {
	List<Tile> tiles;


	int teamInd = -1;


	public Foundation(List<Tile> ts)
	{
		tiles = ts;
		foreach (Tile t in tiles)
		{
			t.found=this;
		}
	}
	public int getTeam()
	{
		return teamInd;
	}
	public List<Tile> getTiles()
	{
		return tiles;
	}

	public bool serverChangeCurrentOwner()
	{
		int discoverTeam = -1;
		foreach(Tile t in tiles)
		{
			Unit u = t.getOccupant();
			if (u)
			{
				int team = u.teamIndex;
				if (discoverTeam == -1)
				{
					discoverTeam = team;
				}
				else if( discoverTeam != team)
				{
					return false;
				}
			}
		}
		if(discoverTeam != -1)
		{
			if(teamInd!= discoverTeam)
			{
				teamInd = discoverTeam;
				return true;
			}
			


		}
		return false;
	}
	public void clientChangeCurrentOwner(int team)
	{
		teamInd = team;
		foreach (Tile t in tiles)
		{
			t.setFoundColor(GameObject.FindObjectOfType<GameManager>().clientPlayer.teamIndex == team);
		}
	}
	public bool hasBuilding()
	{
		bool building = false;
		foreach(Tile t in tiles)
		{
			Unit u = t.getOccupant();
			if (u)
			{
				if (u.isStructure && !u.stat.getBool(StatType.addOn))
				{
					building = true;
					break;
				}
			}
		}
		return building;
	}


}
