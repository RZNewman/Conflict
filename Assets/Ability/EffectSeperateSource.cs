using System.Collections.Generic;
using UnityEngine;

public class EffectSeperateSource : Ability
{
	public override bool cast(Tile target, int team, Tile source)
	{
		if (GetComponent<Targeting>().evaluate(target, team))
		{
		

			foreach (Transform child in transform)
			{
				Ability ab = child.GetComponent<Ability>();

				ab.cast(target, team, target);

			}
			return true;
		}
		return false;
		
	}

	public override string toDesc(bool sayTarget = true, bool plural = false)
	{
		string desc = "";
		bool first = true;
		foreach (Transform child in transform)
		{
			Ability ab = child.GetComponent<Ability>();

			if (first)
			{
				desc += ab.toDesc(false);
				first = false;
			}
			else
			{
				desc += " and " + ab.toDesc(false);
			}


		}
		return desc+ " " + targetingDesc(sayTarget, plural, "of"); 
	}


}
