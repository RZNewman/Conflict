using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw : Ability
{
    public int cards;
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = string.Format("draw {0} cards", cards);

        return desc /*+ " " + targetingDesc(sayTarget, plural, "")*/;
    }
    public void delegateDraw(PlayerGhost p)
	{
        p.drawCards(cards);
	}
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            gm.delegateToTeam(delegateDraw, team);
            return true;
        }

        return false;
    }
}

