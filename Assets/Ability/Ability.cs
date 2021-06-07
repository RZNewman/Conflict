using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Targeting;

public abstract class Ability : MonoBehaviour
{
	protected GameManager gm;

	//protected StatHandler st;
	// Start is called before the first frame update
	public void initialize()
	{
		gm = FindObjectOfType<GameManager>();
	}

	// Update is called once per frame
	void Update()
	{

	}
	public enum descMode
	{
		normal,
		suffix
	}
	protected string targetingDesc(bool sayTarget, bool plural, string specifier = "to", descMode mode = descMode.normal)
	{
		Targeting tar = GetComponent<Targeting>();
		string desc = "<prefix><noun><suffix>";
		string noun = "tile";
		string prefix = "";
		string suffix = "";
		if (tar)
		{
			foreach (Rule r in tar.rules)
			{
				switch (r.type)
				{
					case TargetRule.isOccupied:
						if (!r.inverse)
						{
							noun = "unit";
						}
						break;
					case TargetRule.isDamaged:
						string damaged = "damaged ";
						if (r.inverse)
						{
							damaged = "non-" + damaged;
						}
						prefix = damaged + prefix;
						break;
					case TargetRule.isAlly:
						if (!r.inverse)
						{
							prefix = "allied " + prefix;
						}
						else
						{
							prefix = "enemy " + prefix;
						}
						break;
					case TargetRule.unitType:
						string type = ((Unit.unitType)(Mathf.FloorToInt(r.value))).ToString();
						if (r.inverse)
						{
							type = "non-" + type;
						}
						prefix = prefix + type + " ";
						break;
					case TargetRule.inArea:
						string area = "in area "+Mathf.FloorToInt(r.value);
						if (r.inverse)
						{
							area = "not " + area;
						}
						suffix += " "+area;
						break;
				}
			}
		}
		if (plural)
		{
			noun += "s";
		}
		if(mode == descMode.normal)
		{
			desc = desc.Replace("<noun>", noun);
			desc = desc.Replace("<prefix>", prefix);
			desc = desc.Replace("<suffix>", suffix);
			string toStatement = specifier + " ";
			if (sayTarget)
			{
				toStatement += "target ";
			}
			desc = toStatement + desc;
		}
		else if(mode == descMode.suffix)
		{
			desc = desc.Replace("<noun>", "");
			desc = desc.Replace("<prefix>", "");
			desc = desc.Replace("<suffix>", suffix);
		}
		
		

		return desc.Trim();
	}
	public abstract string toDesc(bool sayTarget  = true, bool plural  =false);
	public abstract bool cast(Tile target, int team, Tile source);


}
