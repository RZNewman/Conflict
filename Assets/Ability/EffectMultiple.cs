using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Targeting;
public class EffectMultiple : Ability
{
	public override bool cast(Tile target, int team, Tile source)
	{
		bool onevalid = false;
		List<Tile> ts = GetComponent<Targeting>().evaluateTargets(team, source);

		foreach(Tile t in ts)
		{
			onevalid = true;
			foreach (Transform child in transform)
			{
				Ability ab = child.GetComponent<Ability>();

				ab.cast(t, team, source);
				
			}
		}
		return onevalid;
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
				desc += ab.toDesc(false, true);
				first = false;
			}
			else
			{
				desc+= " and "+ ab.toDesc(false, true);
			}
			

		}
		return desc + " " + targetingDesc(sayTarget, false,"to");

	}


}
