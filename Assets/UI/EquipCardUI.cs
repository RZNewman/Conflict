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
        Dictionary<StatType, float> sts;
        if (isPrefab)
        {
            StatHandler st = maker.GetComponent<StatHandler>();
            
            if (st)
            {
                sts = st.prefabStats();
            }
			else
			{
                sts = new Dictionary<StatType, float>();
			}

            populateTitle(maker.name);

            populateBody(sts, false, maker.GetComponent<BuffAbil>()?.abilitiesPre.Select(x => x.GetComponent<Ability>()).ToArray());
        }
        else
        {
            StatHandler st = maker.GetComponent<StatHandler>();
            if (st)
            {
                sts = st.export();
            }
            else
            {
                sts = new Dictionary<StatType, float>();
            }
            populateTitle(maker.originalName);
            populateBody(sts, false, e.GetComponent<BuffAbil>()?.abilities.Select(x => x.GetComponent<Ability>()).ToArray());
        }


        setBackground();
		populateType(e);
		

		populateCost(maker);
		

	}
}
