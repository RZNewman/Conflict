using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static StatBlock;

public abstract class CardUI : MonoBehaviour
{
    public Image cardArt;
    public Text cardTitle;
    public CostUI cardCost;
    public Image cardBG;
    public TextMeshProUGUI cardBody;
    public GameObject selection;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public abstract void populateSelf(Cardmaker maker, bool isPrefab);
    protected void populateArt(Sprite art)
	{
        cardArt.sprite = art;
    }
    protected void populateArt(GameObject model/*, int retry =0*/)
    {
        //if (retry > 3)
        //{
        //          return;
        //}

        List<GameObject> dontRender = new List<GameObject>();
        foreach(Transform c in model.transform)
		{
            if (c.tag == "BuffScaler")
			{
                dontRender.Add(c.gameObject);
                c.gameObject.SetActive(false);
			}

        }
        Texture2D art = RuntimePreviewGenerator.GenerateModelPreview(model.transform,120,80);
        foreach(GameObject o in dontRender)
		{
            o.SetActive(true);
		}

        //Texture2D art = AssetPreview.GetAssetPreview(prefab);
		try
		{
            Rect rec = new Rect(0, 0, art.width, art.height);
            cardArt.sprite = Sprite.Create(art, rec, new Vector2(0.5f, 0.5f), 100);
        }
		catch
		{
            Debug.Log("Sprite load error");
            //StartCoroutine(ArtCoRou(prefab,retry+1));
        }
    }
    //   IEnumerator ArtCoRou(GameObject prefab,int retry)
    //{
    //       yield return new WaitForSeconds(1);
    //       populateArt(prefab, retry);
    //   }

    protected void populateTitle(string title)
	{
        cardTitle.text = title;
	}
    protected virtual void populateCost(Cardmaker mkr)
    {
        cardCost.setCost(mkr.resourceCost.ToString(), new CostUI.costTypes(true,true,false));
        //cardCost.transform.parent.GetComponent<Image>().color = GameColors.resources;
    }
    public void select(bool isSelected, Color c)
	{
        selection.SetActive(isSelected);
        c.a = 0.6f;
        selection.GetComponent<Image>().color = c;
	}

    protected void populateBody(Dictionary<StatType, float> stats, bool skipUnitValues = true, Ability[] abils = null, Aura[] auras = null)
	{
        cardBody.text = cardText(stats,Status.getDefault(),skipUnitValues,abils,auras);
    }
    protected void populateBody(Dictionary<StatType, float> stats, Status.Effects status, bool skipUnitValues = true, Ability[] abils = null, Aura[] auras = null)
    {
        cardBody.text = cardText(stats,status, skipUnitValues, abils, auras);
    }

    public static string cardText(Dictionary<StatType, float> stats, Status.Effects status, bool skipUnitValues = true, Ability[] abils = null, Aura[] auras = null)
	{
        string text = "";
        int[] valueText = new int[2];
        string valueLines = "";

        string effects = status.toString();
		if (effects.Length > 1)
		{
            text += "<i>" + effects + "</i>\n";

        }

        foreach (StatType t in stats.Keys)
		{
            string line = "";
			switch (t)
			{
                case StatType.health:
                    valueText[1] = (int)stats[t];
                    break;
                case StatType.attack:
                    valueText[0] = (int)stats[t];
                    break;
                case StatType.range:
                    line = "{0} Range";
                    break;
                case StatType.moveSpeed:
                    line = "{0} Starting Movement";
                    break;
                case StatType.supplyMax:
                    line = "{0}<sprite index= 5> Max Supply";
                    break;
                case StatType.supplyIncome:
                    line = "{0}<sprite index= 1> Supply Income";
                    break;
                case StatType.armor:
                    line = "Armor {0}";
                    break;
                case StatType.piercing:
                    line = "Piercing";
                    break;
                case StatType.regen:
                    line = "Regen {0}";
                    break;
                case StatType.agile:
                    line = "Agile";
                    break;
                //case StatType.overwhelm://benched
                //    line = "Overwhelm {0}";
                //    break;
                case StatType.charge:
                    line = "Charge";                 
                    break;
                case StatType.bypass:
                    line = "Bypass";
                    break;
                case StatType.frontline:
                    line = "Frontline";
                    break;
                case StatType.ghost:
                    line = "Ghost";
                    break;
                case StatType.slow:
                    line = "Slow {0}";
                    break;
				case StatType.bloodlust:
					line = "Bloodlust";
					break;
				//case StatType.collateral://benched
				//    line = "Collateral {0}";
				//    break;
				case StatType.cleave:
					line = "Cleave {0}";
					break;
				case StatType.cardShardIncome:
                    line = "{0}<sprite index= 3> Card Shard Income";
                    break;
                case StatType.structureFragmentIncome:
                    line = "{0}<sprite index= 2> Material Fragment Income";
                    break;
                case StatType.addOn:
                    line = "Add-On";
                    break;
                case StatType.resourceSpend:
                    line = "{0}<sprite index= 0> Power Limit";
                    break;
                default:
                    line = "";
                    break;
			}
			if (line != "")
			{
                if(t == StatType.range || t == StatType.moveSpeed)
				{
                    valueLines += string.Format(line + "\n", stats[t]);
                }
				else
				{
                    text += string.Format(line + "\n", stats[t]);
                }
                
            }
            
		}
		if (!skipUnitValues )
		{
			if (valueText[0] != 0 || valueText[1] != 0)
            {
                string atkcolor = ColorUtility.ToHtmlStringRGB(GameColors.attack);
                string defcolor = ColorUtility.ToHtmlStringRGB(GameColors.defense);
                string valueLine = "<color=#" + atkcolor + ">" + valueText[0] + "</color>/<color=#" + defcolor + ">" + valueText[1] + "</color>\n";
                valueLines = valueLine + valueLines;
            }
            
            text = valueLines + text;

        }
		if (abils != null)
		{
            foreach(Ability ab in abils)
			{
                
                
                AbilityRoot root = ab.GetComponent<AbilityRoot>();
                string prefix;
                string desc = ab.toDesc(root.trigger == AbilityRoot.TriggerType.none);
                desc = Operations.Capatialize(desc);
                switch (root.trigger)
				{
                    case AbilityRoot.TriggerType.onBuffTick:
                        prefix = "On Tick";
                        break;
                    case AbilityRoot.TriggerType.onDeath:
                        prefix = "On Death";
                        break;
                    default:
                        prefix = root.resourceCost.ToString();
                        break;
				}

                text += prefix + ": " + desc + "\n";
			}
		}
        if (auras != null)
		{
            foreach (Aura au in auras)
            {
                //TODO tell if aura is prefab
                string desc = au.toDesc(true);
                desc = Operations.Capatialize(desc);

                text += desc + "\n";
            }
        }
        if(text != "")
		{
            text = text.Remove(text.Length - 1);
		}
        return text;
	}
}
