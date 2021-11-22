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
        inRangeBypass,
        self,
        currentHPGT

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
    bool evaluateRules(Rule[] eRules,Tile t, int team, Tile source = null)
	{
        //Debug.Log(t + " - " + team);
        foreach (Rule r in eRules)
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
                    
                    result = source.rangeToTile(t, false, out didBypass)<= Mathf.FloorToInt(r.value) && !didBypass;
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
                case TargetRule.self:
                    if (!source)
                    {
                        result = false;
                        break;
                    }
                    result = source == t;
                    break;
                case TargetRule.isDamaged:
                    if (!t.getOccupant())
                    {
                        result = false;
                        break;
                    }
                    result = t.getOccupant().isDamaged;
                    break;
                case TargetRule.currentHPGT:
                    if (!t.getOccupant())
                    {
                        result = false;
                        break;
                    }
                    result = t.getOccupant().getHeath() > r.value;
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
					case TargetRule.foundation:
						ruleHit = true;
                        subset = FindObjectOfType<GameGrid>()
                            .GetFoundations()
                            .Where((fou) => fou.getTeam() == team)
                            .Where((fou) => (!fou.hasBuilding() ^ r.value > 1))
                            .Select((fou) => fou.getTiles())
                            .Aggregate ( new List<Tile>(),(a, b) => { a.AddRange(b); return a; })
                            .Where((fou) => fou.isFoundation)
                            .Where((fou) => fou.getTerrainWalkCost(Unit.unitType.structure) != -1)
                            .ToList();

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

    public List<Tile> getAuraArea(Tile source)
	{
        List<Tile> targets = new List<Tile>();
        foreach (Rule r in rules)
        {
			if (r.inverse)
			{
                continue;
			}
            switch (r.type)
            {
                case TargetRule.inArea:
                    targets = source.tilesInArea(Mathf.FloorToInt(r.value));
                    break;
                case TargetRule.inRange:

                    targets = source.tilesInRange(Mathf.FloorToInt(r.value), false);
                    break;
                case TargetRule.inRangeBypass:

                    targets = source.tilesInRange(Mathf.FloorToInt(r.value), true);
                    break;
            }
        }
        if(targets.Count == 0)
		{
            targets = new List<Tile>(FindObjectOfType<GameGrid>().allTiles);
        }
        return targets;
    }

    static bool FoundationCheck(Tile t, int team, bool addOn)
	{
        return t.isFoundation && t.getTerrainWalkCost(Unit.unitType.structure) != -1 && t.found.getTeam() == team && (!t.found.hasBuilding() ^ addOn);
    }

    public enum descMode
    {
        normal,
        suffix
    }
    public string targetingDesc(bool sayTarget, bool plural, string specifier = "to", descMode mode = descMode.normal)
    {
        string desc = "<prefix><noun><suffix>";
        string noun = "tile";
        string prefix = "";
        string suffix = "";
		if (rules.Length == 0)
		{
			return "";
		}

		foreach (Rule r in rules)
        {
            switch (r.type)
            {
                case TargetRule.isOccupied:
                    if (!r.inverse)
                    {
                        noun = "unit";
                    }
                    break;
                case TargetRule.isDamaged:
                    string damaged = "damaged ";
                    if (r.inverse)
                    {
                        damaged = "non-" + damaged;
                    }
                    prefix = damaged + prefix;
                    break;
                case TargetRule.isAlly:
                    if (!r.inverse)
                    {
                        prefix = "allied " + prefix;
                    }
                    else
                    {
                        prefix = "enemy " + prefix;
                    }
                    break;
                case TargetRule.unitType:
                    string type = ((Unit.unitType)(Mathf.FloorToInt(r.value))).ToString();
                    if (r.inverse)
                    {
                        type = "non-" + type;
                    }
                    prefix = prefix + type + " ";
                    break;
                case TargetRule.self:
                    string self = "self";
                    if (r.inverse)
                    {
                        self = "non-" + self;
                        prefix = prefix + self + " ";
                    }
					else
					{
                        noun = self;
					}
                    
                    break;
                case TargetRule.currentHPGT:
                    string currentHP = "with heath ";
                    int num = Mathf.FloorToInt( r.value);
                    if (r.inverse)
                    {
                        currentHP += "less than " + (num + 1) + " ";
                    }
					else
					{
                        currentHP += "greater than " + num + " ";
                    }
                    suffix = " "+ currentHP + suffix;
                    break;
                case TargetRule.inArea:
                    string area = "in area " + Mathf.FloorToInt(r.value);
                    if (r.inverse)
                    {
                        area = "not " + area;
                    }
                    suffix += " " + area;
                    break;
                case TargetRule.inRange:
                    string range = "in range " + Mathf.FloorToInt(r.value);
                    if (r.inverse)
                    {
                        area = "not " + range;
                    }
                    suffix += " " + range;
                    break;
                case TargetRule.inRangeBypass:
                    string rangeB = "in bypass range " + Mathf.FloorToInt(r.value);
                    if (r.inverse)
                    {
                        area = "not " + rangeB;
                    }
                    suffix += " " + rangeB;
                    break;
            }
        }
        
        if (plural)
        {
            noun += "s";
        }
        if (mode == descMode.normal)
        {
            desc = desc.Replace("<noun>", noun);
            desc = desc.Replace("<prefix>", prefix);
            desc = desc.Replace("<suffix>", suffix);
            string toStatement = specifier + " ";
            if (sayTarget)
            {
                toStatement += "target ";
            }
            desc = toStatement + desc;
        }
        else if (mode == descMode.suffix)
        {
            desc = desc.Replace("<noun>", "");
            desc = desc.Replace("<prefix>", "");
            desc = desc.Replace("<suffix>", suffix);
        }



        return desc.Trim();
    }
}



