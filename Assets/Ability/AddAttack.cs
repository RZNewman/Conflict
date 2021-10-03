using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddAttack : Ability
{
    public int attacks;
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = string.Format("give {0} additional attack" + (attacks>1?"s":""), attacks);

        return desc + " " + targetingDesc(sayTarget, plural);
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            Unit u = target.getOccupant();
            if (u)
            {
                u.addAttacks(attacks);
            }
            return true;
        }

        return false;
    }
}
