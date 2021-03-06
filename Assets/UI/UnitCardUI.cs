using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static StatBlock;

public class UnitCardUI : CardUI
{
    public Text attack;
    public Text defense;
    public Text movement;
    public Text range;
    public Text type;
    public GameObject descriptors;
    // Start is called before the first frame update
    void Start()
    {
        attack.color = GameColors.attack;
        defense.color = GameColors.defense;
        movement.color = GameColors.move;
        range.color = GameColors.range;
    }

    public void populateValues(Dictionary<StatType, float> sts)
	{
        
        attack.text = sts[StatType.attack].ToString();
        defense.text = sts[StatType.health].ToString();
        movement.text = sts[StatType.moveSpeed].ToString();
        range.text = sts[StatType.range].ToString();
    }
    public void populateValues(Unit u)
	{
        
        attack.text = u.stat.getStat(StatType.attack).ToString();
        range.text = u.stat.getStat(StatType.range).ToString();
        defense.text = u.getHeath().ToString() + "/" + u.stat.getStat(StatType.health).ToString();
        movement.text = u.getMove().ToString()+"/"+ Mathf.Max(0, u.stat.getStat(StatType.moveSpeed)).ToString();
        
    }
    public void showDescriptors()
	{
        descriptors.SetActive(true);
    }

    public void populateType(Unit u)
	{
        string t = "";
		switch (u.type)
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

    public void modifyForStructure(bool isStructure)
	{
		if (isStructure)
		{
            cardBG.color = GameColors.structure;
        }
		else
		{
            cardBG.color = Color.white;
        }
        
	}
    protected override void populateCost(Cardmaker mkr)
	{
		if (mkr.GetComponent<Unit>().isStructure)
		{
            cardCost.setCost(mkr.resourceCost.ToString(), new CostUI.costTypes(true, false, true));
        }
		else
		{
            base.populateCost(mkr);
		}
	}

	public override void populateSelf(Cardmaker maker, bool isPrefab)
	{
        Unit u = maker.GetComponent<Unit>();
        //
        if (u.cardArt != null)
        {
            populateArt(u.cardArt);
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
            populateValues(sts);

        }
		else
		{
            populateTitle( maker.originalName);
            populateValues(u);

        }
        populateBody(u, isPrefab);
        
        populateType(u);      
        populateCost(maker);
        modifyForStructure(u.isStructure);
    }
    protected void populateBody(Unit u, bool isPrefab)
    {
        string text = "";
        Ability[] abils;
        Aura[] auras;
        Status.Effects status;


        
		if (isPrefab)
		{
            abils = u.abilitiesPre.Select(x => x.GetComponent<Ability>()).ToArray();
            auras = u.aurasPre.Select(x => x.GetComponent<Aura>()).ToArray();
            status = Status.getDefault();
        }
		else
		{
            abils = u.abilities.Select(x => x.GetComponent<Ability>()).ToArray();
            auras = u.aurasEmitted.Select(x => x.GetComponent<Aura>()).ToArray();
            status = u.getStatus();
        }


        string effects = status.toString();
        if (effects.Length > 1)
        {
            text += "<i>" + effects + "</i>\n";

        }
        text += u.stat.toDesc(isPrefab, true);
        foreach (Ability ab in abils)
        {


            AbilityRoot root = ab.GetComponent<AbilityRoot>();
            text += root.toDesc(isPrefab);
        }
        foreach (Aura au in auras)
        {
            string desc = au.toDesc(isPrefab);
            //desc = Operations.Capatialize(desc);

            text += desc + "\n";
        }
        
        cardBody.text = text;
    }
}
