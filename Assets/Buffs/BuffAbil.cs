using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    protected override void CallbackTick()
    {
		if (carrier)
		{
            foreach (GameObject o in abilities)
            {
                AbilityRoot ab = o.GetComponent<AbilityRoot>();
                if (ab.trigger == AbilityRoot.TriggerType.onBuffTick)
                {
                    ab.eventAbil(carrier.loc, teamInd, carrier.loc);
    
                }
            }
        }
        
    }
    public override string toDesc(bool isPrefab)
    {
        Ability[] abs;
        int durr;
        if (isPrefab)
        {
            abs = abilitiesPre.Select(x => x.GetComponent<Ability>()).ToArray();
            durr = maxDuration;
        }
        else
        {
            abs = abilities.Select(x => x.GetComponent<Ability>()).ToArray();
            durr = currentDuration;
        }
        string desc = CardUI.cardText(new Dictionary<StatBlock.StatType, float>(), Status.getDefault(), false, abs);
        if (maxDuration > 0)
        {
            desc += " for " + durr + " round" + (durr > 1 ? "s" : "");
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
