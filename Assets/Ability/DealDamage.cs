using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DealDamage : Ability
{
    public int damage;
    public bool piercing;
	public override string toDesc(bool sayTarget = true, bool plural = false)
	{
        string desc = string.Format("deal {0} {1}damage", damage, piercing ? "piercing " : "");
        
        return desc +" " +targetingDesc(sayTarget,plural);
    }
	public override bool cast(Tile target, int team, Tile source)
	{
		if (GetComponent<Targeting>().evaluate(target, team, source))
		{
            Unit u = target.getOccupant();
            if (u)
            {
                u.takeDamage(damage, piercing, Unit.damageSource.ability);
            }
            return true;
        }
        return false;
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
