using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refresh : Ability
{
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = "Give another turn";

        return desc + " " + targetingDesc(sayTarget, plural);
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            Unit u = target.getOccupant();
            if (u)
            {
                u.refresh();
            }
            return true;
        }

        return false;
    }
}
