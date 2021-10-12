using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Status 
{
	[Serializable]
	public struct Effects
	{
		public bool movement;
		public bool attacking;
		public bool casting;
		public bool damage;

		public string toString()
		{
			bool stunned = !movement && !attacking && !casting;

			string desc = "";
			if(stunned && !damage)
			{
				desc += ", Banished";
			}
			else
			{
				if (!damage)
				{
					desc += ", Impervious";
				}
				if (stunned)
				{
					desc += ", Stunned";
				}
				else
				{
					if (!movement)
					{
						desc += ", Rooted";
					}
					if (!attacking)
					{
						desc += ", Disarmed";
					}
					if (!casting)
					{
						desc += ", Silenced";
					}
				}

			}
			if (desc.Length > 1)
			{
				desc = desc.Substring(2);
			}
			
			return desc;
		}

		
	}
	public static Effects getEffects(List<Effects> effects)
	{
		Effects e = new Effects();
		e.attacking = true;
		e.movement = true;
		e.casting = true;
		e.damage = true;

		foreach (Effects e2 in effects)
		{
			e.attacking = e.attacking && e2.attacking;
			e.movement = e.movement && e2.movement;
			e.casting = e.casting && e2.casting;
			e.damage = e.damage && e2.damage;
		}
		return e;
	}

}
