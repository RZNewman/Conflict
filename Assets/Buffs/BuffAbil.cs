using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffAbil : Buff
{
    public List<GameObject> abilitiesPre;
    [HideInInspector]
    public List<GameObject> abilities = new List<GameObject>();

	public override void initailize(Unit u = null)
	{
		base.initailize(u);
        if (u)
        {
            foreach (GameObject o in abilitiesPre)
            {
                abilities.Add(u.createAbility(o));
            }
        }
    }
	protected override void CallbackPD()
	{
        foreach (GameObject o in abilities)
        {
            gm.delayedDestroy(o);
        }
    }
    public override string toDesc()
    {
        string desc = CardUI.cardText(GetComponent<StatHandler>().prefabStats(), false).Replace('\n', ',');
        desc = "'" + desc + "'";
        if (maxDuration > 0)
        {
            desc += " for " + maxDuration + " round" + (maxDuration > 1 ? "s" : "");
        }
        return desc;

    }
	public override void register()
	{
		base.register();
        foreach (GameObject aPre in abilitiesPre)
        {
            aPre.GetComponent<AbilityRoot>().register();
        }
    }
}
