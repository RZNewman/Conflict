using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrdCardUI : CardUI
{
    public void populateBody(Ability ab)
	{
        string desc = ab.toDesc();
        desc = Operations.Capatialize(desc);
		desc.Replace("  ", " ");
        cardBody.text = desc;
    }

	protected override void populateCost(Cardmaker mkr)
	{
		if (mkr.GetComponent<AbilityRoot>().caster)
		{
			cardCost.setCost(mkr.resourceCost.ToString(), new CostUI.costTypes(false, true, false));
		}
		else
		{
			base.populateCost(mkr);
		}
	}

	public void setBackground()
    {
        cardBG.color = GameColors.ordnance;
    }
    public override void populateSelf(Cardmaker maker, bool isPrefab)
    {
		Ability ab = maker.GetComponent<Ability>();
		AbilityRoot ord = maker.GetComponent<AbilityRoot>();
		//
		if (ord.cardArt != null)
		{
			populateArt(ord.cardArt);
		}
		else
		{
			populateArt(maker.gameObject);
		}

		if (isPrefab)
		{
			populateTitle(maker.name);
		}
		else
		{
			populateTitle(maker.originalName);
		}
		
		setBackground();
		populateCost(maker);
		populateBody(ab);
    }
}
