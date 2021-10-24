using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static StatBlock;

public class EquipCardUI : CardUI
{
    public Text type;



	public void populateType(Buff e)
	{
        string t = "";
		switch (e.type)
		{
            case Unit.unitType.structure:
                t = "Structure";
                break;
            case Unit.unitType.light:
                t = "Light";
                break;
            case Unit.unitType.heavy:
                t = "Heavy";
                break;
            case Unit.unitType.flying:
                t = "Flying";
                break;

        }
        type.text = t;
	}

    public void setBackground()
	{
        cardBG.color = GameColors.equipment;
	}
    public override void populateSelf(Cardmaker maker, bool isPrefab)
    {
		Buff e = maker.GetComponent<Buff>();
		//
		if (e.cardArt != null)
		{
			populateArt(e.cardArt);
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
        populateBody(e, isPrefab);


        setBackground();
		populateType(e);
		

		populateCost(maker);
		

	}

    public void populateBody(Buff bu, bool isPrefab)
    {
        string desc = bu.toDesc(isPrefab);
        //desc = Operations.Capatialize(desc);
        desc.Replace("  ", " ");
        cardBody.text = desc;
    }
}
