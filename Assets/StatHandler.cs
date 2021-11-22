using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static StatBlock;
using System.Linq;

public class StatHandler : NetworkBehaviour, PseudoDestroy
{

    public Stat[] statsList;
    readonly Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();
    readonly SyncDictionary<StatType, float> stats =  new SyncDictionary<StatType, float>();


	#region stat Streams
	List<StatHandler> Upstream = new List<StatHandler>();
    List<StatHandler> Downstream = new List<StatHandler>();


    public void addUpstream(StatHandler up)
	{
        Upstream.Add(up);
        up._addDownstream(this);
        modifyStatsStream(up.stats);
        //refreshDownstream();

    }
    void _addDownstream(StatHandler down)
	{
        Downstream.Add(down);
	}
    void _removeUpstream(StatHandler up)
	{
        Upstream.Remove(up);
        refreshStream();

    }
    public void removeDownstream(StatHandler down)
    {
        Downstream.Remove(down);
        down._removeUpstream(this);
    }
    [Server]
    void terminateStreams()
	{

        List<StatHandler> clean = new List<StatHandler>();
        foreach (StatHandler h in Downstream)
        {
            clean.Add(h);

        }
        foreach (StatHandler down in clean)
        {
            removeDownstream(down);
        }

        clean = new List<StatHandler>();

        foreach (StatHandler h in Upstream)
        {
            clean.Add(h);

        }
        foreach (StatHandler up in clean)
        {
            up.removeDownstream(this);
        }

	}
    void refreshDownstream()
	{
        foreach(StatHandler s in Downstream)
		{
            s.refreshStream();
		}
	}
    void refreshStream()
	{
        //      for(int i =0; i<stats.Count; i++)
        //{
        //          StatType type = stats;

        //          stats.Remove(type);
        //}
        //stats.Reset();

        List<StatType> cleanTypes = new List<StatType>();
        foreach(StatType t in stats.Keys)
		{

            cleanTypes.Add(t);
			
		}
        foreach (StatType t in cleanTypes)
        {
            stats.Remove(t);
        }

        foreach (StatType type in baseStats.Keys)
		{

            stats[type] = baseStats[type];
            
            

        }
        foreach(StatHandler up in Upstream)
		{
            modifyStatsStream(up.stats);
		}
        refreshDownstream();

    }
    #endregion

    public string toDesc(bool isPrefab, bool skipUnitValues)
	{
        Dictionary<StatType, float> desc_stats;
        string text = "";
        int[] valueText = new int[2];
        string valueLines = "";

        if (isPrefab)
		{
            desc_stats = prefabStats();
		}
		else
		{
            desc_stats = export();

        }

        foreach (StatType t in desc_stats.Keys)
        {
            string line = "";
            switch (t)
            {
                case StatType.health:
                    valueText[1] = (int)desc_stats[t];
                    break;
                case StatType.attack:
                    valueText[0] = (int)desc_stats[t];
                    break;
                case StatType.range:
                    line = "{0} Range";
                    break;
                case StatType.moveSpeed:
                    line = "{0} Starting Movement";
                    break;
                case StatType.supplyMax:
                    line = "{0}"+GameConstants.iconSupplyMax+" Max Supply";
                    break;
                case StatType.supplyIncome:
                    line = "{0}" + GameConstants.iconSupply + " Supply Income";
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
                    line = "{0}" + GameConstants.iconCards + " Card Shard Income";
                    break;
                case StatType.structureFragmentIncome:
                    line = "{0}" + GameConstants.iconMaterial + " Material Fragment Income";
                    break;
                case StatType.addOn:
                    line = "Add-On";
                    break;
                case StatType.resourceSpend:
                    line = "{0}" + GameConstants.iconPower + " Power Limit";
                    break;
                default:
                    line = "";
                    break;
            }
            if (line != "")
            {
                if (t == StatType.range || t == StatType.moveSpeed)
                {
                    valueLines += string.Format(line + "\n", desc_stats[t]);
                }
                else
                {
                    text += string.Format(line + "\n", desc_stats[t]);
                }

            }

        }
        if (!skipUnitValues)
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
        if (text != "")
        {
            text = text.Remove(text.Length - 1);
        }
        return text;

    }

    [Server]
    public void initialize()
	{
        foreach (Stat s in statsList)
        {
            if (stats.ContainsKey(s.type))
            {
                baseStats[s.type] += s.value;
                stats[s.type] += s.value;
            }
            else
            {
                baseStats[s.type] = s.value;
                stats[s.type] = s.value;
            }
        }
    }
    public Dictionary<StatType, float> prefabStats()
	{
        Dictionary<StatType, float> pre = new Dictionary<StatType, float>();
        foreach (Stat s in statsList)
        {
            if (pre.ContainsKey(s.type))
            {
                pre[s.type] += s.value;
            }
            else
            {
                pre[s.type] = s.value;
            }
        }
        return pre;
    }
    #region UI callbacks

    public delegate void Refresh();
    List<Refresh> statCallbacks = new List<Refresh>();
    public void addRefresh(Refresh re)
	{
        statCallbacks.Add(re);
	}

    public void removeRefresh(Refresh re)
    {
        statCallbacks.Remove(re);
    }
    public override void OnStartClient()
	{
        stats.Callback += handleStats;

    }
	void handleStats(SyncDictionary<StatType, float>.Operation op, StatType stat, float value)
	{
        // equipment changed,  perhaps update the gameobject
        //Debug.Log(op + " - " + stat);

        runRefresh();

	}
    public void runRefresh()
	{
        foreach (Refresh re in statCallbacks)
        {
            //Debug.Log(re);
            re();
        }
    }
	#endregion

	public void PDestroy(bool isSev)
	{
		if (isSev)
		{
            terminateStreams();
        }
        
	}
	public int getStat(StatType s, bool baseStatCard = false)
	{
		if (!baseStatCard)
		{
            if (stats.ContainsKey(s))
            {
                //Debug.Log(s +" "+stats[s].ToString());
                return Mathf.FloorToInt(stats[s]);
            }
        }
		else
		{
            if (baseStats.ContainsKey(s))
            {
                //Debug.Log(s +" "+stats[s].ToString());
                return Mathf.FloorToInt(baseStats[s]);
            }
        }
		
        return 0;
	}
    public Dictionary<StatType, float> export()
	{
        Dictionary<StatType, float> dict = new Dictionary<StatType, float>();
        foreach(StatType type in stats.Keys)
		{
            dict.Add(type, stats[type]);
		}


        return dict;

    }
    public bool getBool(StatType s)
	{
        if (stats.ContainsKey(s))
        {
            //Debug.Log(s +" "+stats[s].ToString());
            return stats[s]>0;
        }
        return false;
    }
    //Only for 'Current' attributes
    public void setStat(StatType s, int i)
	{
        stats[s] = i;
        refreshDownstream();


    }
    public void modifyStat(StatType s, int i)
    {

        if (stats.ContainsKey(s))
        {
            
            stats[s] += i;

            baseStats[s] += i;


        }
        else
        {

            stats[s] = i;

            baseStats[s] = i;

        }

        refreshDownstream();
        
        
    }
    public void setPrefabStat(StatType type, int val)
    {
        bool found = false;
        for(int i =0; i< statsList.Length; i++)
		{
            
            if(statsList[i].type == type)
			{
                statsList[i].value = val;
                found = true;
                break;
			}
		}
		if (!found)
		{
            Stat st = new Stat(type, val);

            statsList = statsList.Concat(new Stat[] { st }).ToArray();
		}


    }
    void modifyStatsStream(SyncDictionary<StatType, float> newStats)
    {
        foreach (StatType t in newStats.Keys)
        {
            //Debug.Log(t + " - " + newStats[t]+ " - "+(int)StatType.currentAttacks );

            if (stats.ContainsKey(t))
            {
                stats[t] += newStats[t];

            }
            else
            {
                stats[t] = newStats[t];
            }
            
            
        }
        refreshDownstream();
    }


    public static Dictionary<StatType, float> sumStats(SyncDictionary<StatType, float> a, SyncDictionary<StatType, float> b)
    {
        Dictionary<StatType, float> statsBasic = new Dictionary<StatType, float>();
        foreach (StatType s in a.Keys)
        {
            statsBasic[s] = a[s];
        }
        foreach (StatType s in b.Keys)
        {
            if (statsBasic.ContainsKey(s))
            {
                statsBasic[s] += b[s];
            }
            else
            {
                statsBasic[s] = b[s];
            }
        }
        return statsBasic;
    }
}
