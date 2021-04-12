using System.Collections;
using System.Collections.Generic;
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
    // Start is called before the first frame update
    void Start()
    {
        attack.color = GameColors.attack;
        defense.color = GameColors.defense;
        movement.color = GameColors.move;
        range.color = GameColors.range;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void populateValues(Dictionary<StatType, float> sts)
	{
        
        attack.text = sts[StatType.attack].ToString();
        defense.text = sts[StatType.health].ToString();
        movement.text = sts[StatType.movement].ToString();
        range.text = sts[StatType.range].ToString();
    }
    public void populateValues(Unit u)
	{
        attack.text = u.stat.getStat(StatType.attack).ToString();
        range.text = u.stat.getStat(StatType.range).ToString();
        defense.text = u.getHeath().ToString();
        movement.text = u.getMove().ToString();
        
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


}
