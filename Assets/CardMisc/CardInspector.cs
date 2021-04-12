using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static StatBlock;

public class CardInspector : MonoBehaviour
{
    UnitCardUI visualsUnit;
    OrdCardUI visualsAbility;
    GameObject handSample;
    // Start is called before the first frame update
    void Start()
    {
        visualsUnit = GetComponentInChildren<UnitCardUI>();
        visualsUnit.gameObject.SetActive(false);
        visualsAbility = GetComponentInChildren<OrdCardUI>();
        visualsAbility.gameObject.SetActive(false);
    }
    public enum inspectType
	{
        card,
        unit,
        ability
	}
    // Update is called once per frame
    public void inspect(GameObject obj, inspectType type)
	{
        if (type == inspectType.card)
        {
            handSample = Instantiate(obj, transform);
            handSample.transform.localPosition = Vector3.zero;
        }
        else if(type == inspectType.unit)
        {


            Unit u = obj.GetComponent<Unit>();
            if (u.cardArt != null)
            {
                visualsUnit.populateArt(u.cardArt);
            }
            else
            {
                visualsUnit.populateArt(obj);
            }

            visualsUnit.populateTitle(u.unitName);
            visualsUnit.populateType(u);


            visualsUnit.populateCost(u.resourceCost.ToString());
            visualsUnit.populateValues(u);
            visualsUnit.populateBody(u.stat.export(),true,u.abilities.Select(x => x.GetComponent<Ability>()).ToArray());
            visualsUnit.modifyForStructure(u.isStructure);



            visualsUnit.gameObject.SetActive(true);
        }
        else if(type == inspectType.ability)
		{
            Ordnance o = obj.GetComponent<Ordnance>();
            if (o.cardArt != null)
            {
                visualsAbility.populateArt(o.cardArt);
            }
            else
            {
                visualsAbility.populateArt(obj);
            }

            visualsAbility.populateTitle(o.nameString);
            visualsAbility.setBackground();

            visualsAbility.populateCost(o.resourceCost.ToString());

            visualsAbility.populateBody(o.GetComponent<Ability>());
            visualsAbility.gameObject.SetActive(true);
        }
    }
    public void uninspect()
	{
        if (handSample)
        {
            Destroy(handSample);
        }
		else
		{
            visualsUnit.gameObject.SetActive(false);
            visualsAbility.gameObject.SetActive(false);
        }
        
	}
    void generateKeywordDesc()
	{

	}
    string[] keywordDesc(StatType type, int value)
	{
        string[] desc = new string[2];
		switch (type)
		{
            case StatType.addOn:
                desc[0] = "Add-on";
                desc[1] = "Place on your foundations";
                break;
            case StatType.agile:
                desc[0] = "Agile";
                desc[1] = "Attack between moves";
                break;
            case StatType.armor:
                desc[0] = "Armor";
                desc[1] = "Reduce incoming damage";
                break;
            case StatType.bypass:
                desc[0] = "Bypass";
                desc[1] = "Attack through blockers";
                break;
            case StatType.charge:
                desc[0] = "Charge";
                desc[1] = "Act immediately";
                break;
            case StatType.frontline:
                desc[0] = "Frontline";
                desc[1] = "Deploy next to an ally";
                break;
            case StatType.ghost:
                desc[0] = "Ghost";
                desc[1] = "Move through enemies";
                break;
            case StatType.overwhelm:
                desc[0] = "Overwhelm";
                desc[1] = "Reduce retailiation";
                break;
            case StatType.piercing:
                desc[0] = "Piercing";
                desc[1] = "Ignore armor";
                break;
            case StatType.regen:
                desc[0] = "Regen";
                desc[1] = "Regenerate health";
                break;
            case StatType.slow:
                desc[0] = "Slow";
                desc[1] = "Reduce target's movement";
                break;
            case StatType.resourceSpend:
                desc[0] = "Resource Limit";
                desc[1] = "Play bigger cards";
                break;
            default:
                desc[0] = "";
                desc[1] = "";
                break;
        }
        return desc;
	}
}
