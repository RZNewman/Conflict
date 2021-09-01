using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    public Text power;
    public Text supply;
    public Text supplyMax;
    public Text material;
    //cards in hand
    public Text cardsInDeck;
    public GameObject powerIncrement;
    public Text supplyIncome;
    public GameObject supplyMaxIncrement;
    public Text materialFragmentIncome;
    public Text cardShardIncome;
    public FragmentCounter matFrags;
    public FragmentCounter cardShards;




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
        void tryRefresh(Text field, int value)
		{
            if (field) field.text = value.ToString();
		}

        tryRefresh(supplyIncome, st.getStat(StatBlock.StatType.supplyIncome));
        tryRefresh(supply, gm.clientPlayer.getCurrentResources());
        tryRefresh(supplyMax, st.getStat(StatBlock.StatType.supplyMax));
        tryRefresh(power, st.getStat(StatBlock.StatType.resourceSpend));
        tryRefresh(cardsInDeck, gm.clientPlayer.getCurrentCards());

        tryRefresh(material, gm.clientPlayer.getCurrentMaterials());
        tryRefresh(materialFragmentIncome, st.getStat(StatBlock.StatType.structureFragmentIncome));
        tryRefresh(cardShardIncome, st.getStat(StatBlock.StatType.cardShardIncome));

        if (matFrags) matFrags.displayFragements(gm.clientPlayer.getCurrentFragments());
        if (cardShards) cardShards.displayFragements(gm.clientPlayer.getCurrentShards());

    }
    public void setLimitIncrease(bool show)
	{
        powerIncrement.SetActive(show);
	}
    public void setMaxIncrease(bool show)
    {
        supplyMaxIncrement.SetActive(show);
    }
}
