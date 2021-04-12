using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    public Text income;
    public Text current;
    public Text max;
    public Text limit;
    public GameObject limitInc;
    public GameObject maxInc;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        if (!gm)
        {
            gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        }

    }

    // Update is called once per frame
    public void refresh()
    {
        //Debug.Log("Refresh called");
        
        StatHandler st = gm.clientPlayer.GetComponent<StatHandler>();
		if (income)
		{
            income.text = st.getStat(StatBlock.StatType.resourceIncome).ToString();
        }
        if (current)
        {
            current.text = gm.clientPlayer.getCurrentResources().ToString();
        }
        if (max)
        {
            max.text = st.getStat(StatBlock.StatType.resourceMax).ToString();
        }
		if (limit)
		{
            limit.text = st.getStat(StatBlock.StatType.resourceSpend).ToString();
        }
        
        

    }
    public void setLimitIncrease(bool show)
	{
        limitInc.SetActive(show);
	}
    public void setMaxIncrease(bool show)
    {
        maxInc.SetActive(show);
    }
}
