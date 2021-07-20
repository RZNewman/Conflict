using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatBlock;
using Mirror;
using System.Linq;

public class Targeting : MonoBehaviour
{
    [Serializable]
    public enum TargetRule : byte
    {
        isOccupied = 0,
        isDeployable,
        foundation,
        isAlly,
        unitType,
        roomForEquipment,
        inArea,
        isDamaged,
        inRange,
        inRangeBypass

        //inMove
        //inRange
        //inAttack
	}
    [Serializable]
    public struct Rule{
        public TargetRule type;
        public bool inverse;
        public float value;
        public Rule(TargetRule r,bool i, float v)
        {
            type = r;
            inverse = i;
            value = v;
        }
    }
    public Rule[] rules;

    
    
    public enum DeplolableRules
	{
        baseline=0,
        frontline
	}
    public bool evaluate(Tile t, int team, Tile source = null)
	{
        return evaluateRules(rules, t, team, source);
	}
    public bool evaluateRules(Rule[] eRules,Tile t, int team, Tile source = null)
	{
        //Debug.Log(t + " - " + team);
        foreach(Rule r in eRules)
		{
            bool result =false;
            bool didBypass;
            //bool goal = r.value > 0;
            //Debug.Log(r.type + " - " + r.value);
            switch (r.type)
			{
                case TargetRule.isOccupied:
                    result = t.getOccupant();
                    break;
                case TargetRule.isDeployable:
					//goal false -> frontline
                    
					if (r.value == (int)DeplolableRules.baseline)
					{
                        result = t.teamDeploy == team;
						
					}
					else if (r.value == (int)DeplolableRules.frontline)
					{
                        result = t.allyAdjacent(team) || t.teamDeploy == team;

                    }
                    break;
                case TargetRule.foundation:
                    if (!t.isFoundation)
                    {
                        result = false;
                        break;
                    }
                    if (!t.isWalk)
                    {
                        result = false;
                        break;
                    }

                    result = FoundationCheck(t, team, r.value > 1);

                    break;
                case TargetRule.isAlly:
					if (!t.getOccupant())
					{
                        result = false;
                        break;
                    }


                    result = t.getOccupant().teamIndex == team;
                    break;
                case TargetRule.unitType:
                    if (!t.getOccupant())
                    {
                        result = false;
                        break;
                    }
                    result = t.getOccupant().type == (Unit.unitType)r.value;

                    break;
                case TargetRule.roomForEquipment:
                    if (!t.getOccupant())
                    {
                        result = false;
                        break;
                    }
                    result = t.getOccupant().equipmentCount() == 0;

                    break;
                case TargetRule.inArea:
                    if (!source)
                    {
                        result = false;
                        break;
                    }
                    result = source.tilesInArea(Mathf.FloorToInt(r.value)).Contains(t);
                    break;
                case TargetRule.inRange:
                    if (!source)
                    {
                        result = false;
                        break;
                    }
                    
                    result = source.rangeToTile(t, false, out didBypass)<= Mathf.FloorToInt(r.value);
                    //result = source.tilesInRange(Mathf.FloorToInt(r.value),false, team).Contains(t);
                    break;
                case TargetRule.inRangeBypass:
                    if (!source)
                    {
                        result = false;
                        break;
                    }
                    result = source.rangeToTile(t, true, out didBypass) <= Mathf.FloorToInt(r.value);
                    //result = source.tilesInRange(Mathf.FloorToInt(r.value), true, team).Contains(t);
                    break;
                case TargetRule.isDamaged:
                    if (!t.getOccupant())
                    {
                        result = false;
                        break;
                    }
                    result = t.getOccupant().isDamaged;
                    break;
                default:

                    Debug.LogError("unknown rule");
                    break;
			}
            if((!result && ! r.inverse) || (result && r.inverse))
			{
                return false;
			}

        }

        return true;
	}
    public List<Tile> evaluateTargets(int team, Tile source = null)
	{
        List<Tile> targets = new List<Tile>();
        bool subsetFound = false;
        List<Rule> tempRules = new List<Rule>(rules);
        List<Tile> toRemove;
        foreach (Rule r in rules)
        {
            if (!r.inverse)
            {
                List<Tile> subset = new List<Tile>();
                bool ruleHit = false;
                switch (r.type)
                {
                    case TargetRule.inArea:
                        
                        if (!source)
                        {
                            break;
                        }
                        ruleHit = true;
                        subset = source.tilesInArea(Mathf.FloorToInt(r.value));                       
                        break;
                    case TargetRule.inRange:
                        
                        if (!source)
                        {
                            break;
                        }
                        ruleHit = true;
                        subset = source.tilesInRange(Mathf.FloorToInt(r.value),false);
                        break;
                    case TargetRule.inRangeBypass:

                        if (!source)
                        {
                            break;
                        }
                        ruleHit = true;
                        subset = source.tilesInRange(Mathf.FloorToInt(r.value), true);
                        break;
                    case TargetRule.isOccupied:
                        ruleHit = true;
                        subset = FindObjectsOfType<Unit>().Select(u => u.loc).ToList();
                        break;
                }
				if (ruleHit)
				{
                    tempRules.Remove(r);
                    if (!subsetFound)
                    {
                        targets = subset;
                        subsetFound = true;
                    }
                    else
                    {
                        toRemove = new List<Tile>();
                        foreach (Tile t in targets)
                        {
                            if (!subset.Contains(t))
                            {
                                toRemove.Add(t);

                            }
                        }
                        foreach(Tile t in toRemove)
						{
                            targets.Remove(t);
						}
                    }
                }
            }
        }
		if (!subsetFound)
		{
            targets = new List<Tile>(FindObjectOfType<GameGrid>().allTiles);
		}
        toRemove = new List<Tile>();
        foreach (Tile t in targets)
        {
            if (!evaluateRules(tempRules.ToArray(),t,team,source))
            {
                toRemove.Add(t);

            }
        }
        foreach (Tile t in toRemove)
        {
            targets.Remove(t);
        }



        return targets;
	}


    static bool FoundationCheck(Tile t, int team, bool addOn)
	{
        bool teamPresent = false;
        bool structureExists = false;
        List<Tile> alreadyChecked = new List<Tile>();
        bool FoundationCheckRecurse(Tile t)
        {
            if (!t.isFoundation)
            {
                return true;
            }
			if (alreadyChecked.Contains(t))
			{
                return true;
			}
			if (t.getOccupant())
			{
                int occTeam = t.getOccupant().teamIndex;
                if(team == occTeam)
				{
                    teamPresent = true;
					if (t.getOccupant().isStructure) 
                    {
                        structureExists = true;
						//Debug.Log("Structure Exists - " + t);
						if (!addOn)
						{
                            return false;
                        }
                        
                    }
				}
				else
				{
                    //Debug.Log("Enemy Exists - " + t);
                    return false;
				}
			}
            alreadyChecked.Add(t);
            bool result = true;

            foreach(Tile neigh in t.getNeightbors())
			{
                result = result && FoundationCheckRecurse(neigh);

            }
            return result;
        }
        //Debug.Log("team PResent - " + teamPresent); 
        return FoundationCheckRecurse(t) && teamPresent && (!addOn || structureExists);
    }

    
}



