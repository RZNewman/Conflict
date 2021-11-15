using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SelectorUI;

public class TileSelector : MonoBehaviour
{
    List<GameObject> tilesSelectedTarget = new List<GameObject>();
    List<GameObject> tilesSelectedHover = new List<GameObject>();

    public void select(Tile t, bool isHover)
	{
        deselect(isHover);

		if (isHover)
		{
            tilesSelectedHover = getSelectTiles(t, isHover);

        }
		else
		{
            tilesSelectedTarget = getSelectTiles(t, isHover);

        }
	}
    public void select(Tile t, bool isHover, Targeting targ, int teamInd)
    {
        deselect(isHover);

        if (isHover)
        {
            tilesSelectedHover = getSelectTiles(t, isHover, targ, teamInd);

        }
        else
        {
            tilesSelectedTarget = getSelectTiles(t, isHover, targ, teamInd);

        }
    }

    public void deselect(bool isHover)
    {
        if (isHover)
        {
            foreach (GameObject s in tilesSelectedHover)
            {
                Destroy(s);
            }
        }
        else
        {
            foreach (GameObject s in tilesSelectedTarget)
            {
                Destroy(s);
            }
        }
    }

    List<GameObject> getSelectTiles(Tile root, bool isHover)
    {
        List<GameObject> selected = new List<GameObject>();
        List<Tile> searched = new List<Tile>();
        if (!isHover)
        {
            selected.Add(root.select(SelectType.active, isHover));

        }
        searched.Add(root);

        Unit occupant = root.getOccupant();

        if (occupant)
        {
            List<Tile> moveSelect = root.tilesInMove(occupant.getMoveActionable(), occupant.type, occupant.teamIndex, occupant.stat.getBool(StatBlock.StatType.ghost));
            foreach (Tile t in moveSelect)
            {
                selected.Add(t.select(SelectType.move, isHover));
            }
            searched.AddRange(moveSelect);

            if (occupant.canAttack)
            {
                List<Tile> attackSelect = root.tilesInAttack(occupant.stat.getStat(StatBlock.StatType.range), occupant.stat.getBool(StatBlock.StatType.bypass), occupant.teamIndex);
                foreach (Tile t in attackSelect)
                {
                    selected.Add(t.select(SelectType.attack, isHover));
                }
                searched.AddRange(attackSelect);

                void threatFind(Tile t)
                {
                    List<Tile> threat = t.tilesInRange(occupant.stat.getStat(StatBlock.StatType.range), occupant.stat.getBool(StatBlock.StatType.bypass));

                    foreach (Tile t2 in threat)
                    {
                        if (!searched.Contains(t2))
                        {

                            selected.Add(t2.select(SelectType.threat, isHover));
                            searched.Add(t2);
                        }
                    }
                }

                threatFind(root);
                foreach (Tile t in moveSelect)
                {
                    threatFind(t);
                }

            }

        }
        return selected;
    }

    List<GameObject> getSelectTiles(Tile root,  bool isHover, Targeting targ,  int teamInd) 
	{
        List<GameObject> selection = new List<GameObject>();
        foreach (Tile t in targ.evaluateTargets(teamInd, root))
        {
            selection.Add(t.select(SelectType.ability, isHover));
        }
        return selection;
	}
}
