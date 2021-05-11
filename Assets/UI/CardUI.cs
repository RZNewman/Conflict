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
    public Text cardCost;
    public Image cardBG;
    public TextMeshProUGUI cardBody;
    public GameObject selection;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public abstract void populateSelf(Cardmaker maker, bool isPrefab);
    protected void populateArt(Sprite art)
	{
        cardArt.sprite = art;
    }
    protected void populateArt(GameObject prefab/*, int retry =0*/)
    {
		//if (retry > 3)
		//{
  //          return;
		//}
        Texture2D art = RuntimePreviewGenerator.GenerateModelPreview(prefab.transform,120,80);
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
    protected void populateCost(string cost)
    {
        cardCost.text = cost;
        //cardCost.transform.parent.GetComponent<Image>().color = GameColors.resources;
    }
    public void select(bool isSelected)
	{
        selection.SetActive(isSelected);
	}

    protected void populateBody(Dictionary<StatType, float> stats, bool skipUnitValues = true, Ability[] abils = null)
	{
        string text = "";
        int[] valueText = new int[2];
        string valueLines = "";
        
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
                    line = "+{0} Range";
                    break;
                case StatType.movement:
                    line = "+{0} Movement";
                    break;
                case StatType.resourceMax:
                    line = "+{0} Max Resources";
                    break;
                case StatType.resourceIncome:
                    line = "+{0}<sprite index= 1> Resource Income";
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
                case StatType.overwhelm:
                    line = "Overwhelm {0}";
                    break;
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
                //case StatType.hydra://benched
                //    line = "Hydra";
                //    break;
                //case StatType.collateral://benched
                //    line = "Collateral {0}";
                //    break;
                //case StatType.cleave://benched
                //    line = "Cleave {0}";
                //    break;
                case StatType.cardDraw:
                    line = "+{0} Card Draw";
                    break;
                case StatType.addOn:
                    line = "Add-On";
                    break;
                case StatType.resourceSpend:
                    line = "+{0}<sprite index= 0> Resource Limit";
                    break;
                default:
                    line = "";
                    break;
			}
			if (line != "")
			{
                if(t == StatType.range || t == StatType.movement)
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
                string valueLine = "+<color=#" + atkcolor + ">" + valueText[0] + "</color>/<color=#" + defcolor + ">" + valueText[1] + "</color>\n";
                valueLines = valueLine + valueLines;
            }
            
            text = valueLines + text;

        }
		if (abils != null)
		{
            foreach(Ability ab in abils)
			{
                string desc = ab.toDesc();
                desc = Operations.Capatialize(desc);

                text += ab.GetComponent<Ordnance>().resourceCost + ": " + desc + "\n";
			}
		}
        cardBody.text = text;
	}
}
