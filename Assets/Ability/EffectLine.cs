using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Targeting;
using static Tile;

public class EffectLine : Ability
{
	public enum castType
	{
		after
	}
	public castType type;
	public int length;

	public override bool cast(Tile target, int team, Tile source)
	{

		if (GetComponent<Targeting>().evaluate(target, team, source))
		{

			bool onevalid = false;
			List<Tile> ts = new List<Tile>();

			switch (type)
			{
				case castType.after:
					neighDir dir = source.directionToTile(target);
					Tile look = target;
					for (int i = 0; i < length; i++)
					{
						ts.Add(look);
						look = look.getNeighbor(dir);
					}

					break;
			}
			foreach (Tile t in ts)
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
				desc += ab.toDesc(false, true);
				first = false;
			}
			else
			{
				desc += " and " + ab.toDesc(false, true);
			}


		}
		string spec = "to";
		switch (type)
		{
			case castType.after:
				desc += " in bypass range " + length;
				spec = "behind";
				break;
		}
		desc = desc + " " + targetingDesc(sayTarget, false, spec);
		return desc;

	}


}