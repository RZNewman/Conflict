using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillUnit : Ability
{
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = string.Format("kill");

        return desc + " " + targetingDesc(sayTarget, plural, "");
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            Unit u = target.getOccupant();
            if (u)
            {
                u.killSelf();
            }
            return true;
        }

        return false;
    }
}
