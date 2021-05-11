using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static StatBlock;

public class EquipCardUI : CardUI
{
    public Text type;



	public void populateType(Equipment e)
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
		Equipment e = maker.GetComponent<Equipment>();
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
            StatHandler st = maker.GetComponent<StatHandler>();
            Dictionary<StatType, float> sts = st.prefabStats();
            populateTitle(maker.name);

            populateBody(sts, false, maker.GetComponent<Buff>().abilitiesPre.Select(x => x.GetComponent<Ability>()).ToArray());
        }
        else
        {
            populateTitle(maker.originalName);
            populateBody(e.GetComponent<StatHandler>().export(), false, e.GetComponent<Buff>().abilities.Select(x => x.GetComponent<Ability>()).ToArray());
        }


        setBackground();
		populateType(e);
		

		populateCost(maker.resourceCost.ToString());
		

	}
}
