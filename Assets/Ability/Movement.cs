using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : Ability
{
    public int move;
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = string.Format("give {0} movement", move);

        return desc + " " + targetingDesc(sayTarget, plural);
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            Unit u = target.getOccupant();
            if (u)
            {
                u.changeMovement(move);
            }
            return true;
        }

        return false;
    }
}
