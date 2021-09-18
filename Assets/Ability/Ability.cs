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

	protected string targetingDesc(bool sayTarget, bool plural, string specifier = "to", descMode mode = descMode.normal)
	{
		Targeting tar = GetComponent<Targeting>();
		if (tar)
		{
			return tar.targetingDesc(sayTarget, plural, specifier, mode);
		}
		return "";
	}
	public abstract string toDesc(bool sayTarget  = true, bool plural  =false);
	public abstract bool cast(Tile target, int team, Tile source);


}
