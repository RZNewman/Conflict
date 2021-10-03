using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slow : Ability
{
    public int slow;
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = string.Format("slow {0}", slow);

        return desc + " " + targetingDesc(sayTarget, plural, "on");
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            Unit u = target.getOccupant();
            if (u)
            {
                u.getSlowed(slow);
            }
            return true;
        }

        return false;
    }
}