using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainResources : Ability
{
    public int resources;
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = string.Format("gain {0} resources", resources);

        return desc /*+ " " + targetingDesc(sayTarget, plural, "")*/;
    }
    public void delegateGain(PlayerGhost p)
    {
        p.gainResources(resources);
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            gm.delegateToTeam(delegateGain, team);
            return true;
        }

        return false;
    }
}

