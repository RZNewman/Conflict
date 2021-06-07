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
    public Text cardCount;
    public GameObject limitInc;
    public GameObject maxInc;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        assignGM();

    }
    void assignGM()
	{
        if (!gm)
        {
            GameObject o = GameObject.FindGameObjectWithTag("GameController");
            if (o)
            {
                gm = o.GetComponent<GameManager>();
            }

        }
    }

    // Update is called once per frame
    public void refresh()
    {
        //Debug.Log("Refresh called");
        assignGM();
		if (!gm) { return; }
        if (!gm.clientPlayer) { return; }
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
		if (cardCount)
		{
            cardCount.text = gm.clientPlayer.getCurrentCards().ToString();
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
