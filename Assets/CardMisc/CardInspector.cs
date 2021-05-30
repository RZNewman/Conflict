using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static StatBlock;

public class CardInspector : MonoBehaviour
{
    UnitCardUI visualsUnit;
    OrdCardUI visualsAbility;
    EquipCardUI visualsBuff;
    GameObject handSample;

    List<inspectBlock> blocks = new List<inspectBlock>();

    public GameObject keywordHolder;
    public GameObject keywordPre;
    List<GameObject> keywordInstances = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        visualsUnit = GetComponentInChildren<UnitCardUI>();
        visualsUnit.gameObject.SetActive(false);
        visualsAbility = GetComponentInChildren<OrdCardUI>();
        visualsAbility.gameObject.SetActive(false);
        visualsBuff = GetComponentInChildren<EquipCardUI>();
        visualsBuff.gameObject.SetActive(false);
    }
    public enum inspectType
	{
        card,
        cardmaker
	}
    struct inspectBlock
	{
        public GameObject obj;
        public inspectType type;
        public Dictionary<StatType, float> stats;
        public int priority;
        public inspectBlock(GameObject o, inspectType t, int p, Dictionary<StatType, float> s)
        {
            obj = o;
            type = t;
            stats = s;
            priority = p;
        }

    }
    // Update is called once per frame
    public void inspect(GameObject obj, inspectType type, int priority, Dictionary<StatType, float> stats = null)
	{
        //Debug.Log("START ins -"+obj);
        inspectBlock b = new inspectBlock(obj, type, priority, stats);
        int i = 0;

		while (i < blocks.Count)
		{
            if(b.priority < blocks[i].priority)
			{
                break;
			}
            i++;
		}

        if(i == 0)
		{
			if (blocks.Count > 0)
			{
                killInspection();
			}
            createInspection(b);
		}
        blocks.Insert(i, b);
    }
    public void uninspect(GameObject obj)
	{
        //Debug.Log("END ins -" + obj);
        int i = -1;
        for(int j =0; j<blocks.Count; j++)
		{
            if(blocks[j].obj == obj)
			{
                i = j;
                break;
			}
		}
        if(i != -1)
		{
            if(i == 0)
			{
                killInspection();
			}
            blocks.RemoveAt(i);
            if (i == 0 && blocks.Count>0)
            {
                createInspection(blocks[0]);
            }
        }



    }
    void createInspection(inspectBlock b)
	{
        if (b.type == inspectType.card)
        {
            handSample = Instantiate(b.obj, transform);
            handSample.transform.localPosition = Vector3.zero;
            if (b.stats != null)
            {
                generateKeywordDesc(b.stats);
            }
        }
        else if (b.type == inspectType.cardmaker)
        {
            Unit u = b.obj.GetComponent<Unit>();
            Ordnance o = b.obj.GetComponent<Ordnance>();
            Equipment e = b.obj.GetComponent<Equipment>();

            if (u)
            {
                generateKeywordDesc(u.stat.export());
                visualsUnit.populateSelf(u, false);
                visualsUnit.gameObject.SetActive(true);
            }
            else if (o)
            {
                visualsAbility.populateSelf(o, false);
                visualsAbility.gameObject.SetActive(true);
            }
            else if (e)
            {
                generateKeywordDesc(e.GetComponent<StatHandler>().export());
                visualsBuff.populateSelf(e, false);
                visualsBuff.gameObject.SetActive(true);
            }


        }
    }
    void killInspection()
	{
        if (handSample)
        {
            Destroy(handSample);
        }
        else
        {
            visualsUnit.gameObject.SetActive(false);
            visualsAbility.gameObject.SetActive(false);
            visualsBuff.gameObject.SetActive(false);
        }
        //List<GameObject> toRemove = new List<GameObject>();
        foreach (GameObject o in keywordInstances)
        {
            //toRemove.Add(o);
            Destroy(o);
        }
        //foreach (GameObject o in keywordInstances)
    }
    void generateKeywordDesc(Dictionary<StatType, float> stats)
	{
        foreach(StatType type in stats.Keys)
		{
            string[] values = keywordDesc(type, (int)stats[type]);
            if(values[0].Length > 0)
			{
                GameObject o = Instantiate(keywordPre, keywordHolder.transform);
                keywordInstances.Add(o);
                KeywordUI kui = o.GetComponent<KeywordUI>();
                kui.title.text = values[0];
                kui.desc.text = values[1];
            }
            
        }
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
            case StatType.bloodlust:
                desc[0] = "Bloodlust";
                desc[1] = "Attack after killing";
                break;
            default:
                desc[0] = "";
                desc[1] = "";
                break;
        }
        return desc;
	}
}
