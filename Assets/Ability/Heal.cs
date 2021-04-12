using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Heal : Ability
{
    public int health;
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = string.Format("heal {0} health", health);

        return desc + " " + targetingDesc(sayTarget, plural);
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            Unit u = target.getOccupant();
            if (u)
            {
                u.heal(health);
            }
            return true;
        }

        return false;
    }
}
