using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : Ability
{
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = "blink";

        return desc + " " + targetingDesc(sayTarget, plural);
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            Unit u = source.getOccupant();
            if (u && !target.getOccupant())
            {
                gm.serverMove(u, target);
            }
            return true;
        }

        return false;
    }
}
