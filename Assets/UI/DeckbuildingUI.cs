using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckbuildingUI : MonoBehaviour
{
    public Deck deckCards;
    public GameObject mainCardList;
    public GameObject maidCardScroll;
    public GameObject mainDeckList;
    public GameObject maidDeckScroll;

    public Text mainDeckCount;
    public Text mainDeckMax;

    public GameObject buildCardPre;
    public GameObject cardCountPre;

    public static int currentDeckSize = 0;

    Dictionary<int, CardCountUI> currectDeckList = new Dictionary<int, CardCountUI>();

    // Start is called before the first frame update
    void Start()
    {
        mainDeckMax.text = GameConstants.mainDeckSize.ToString();
        createPlayableCards();
        LoadDeck();
    }
    void createPlayableCards()
	{
        //foreach(GameObject c in deckCards.main)
        for(int i = 0; i<deckCards.main.Count; i++)
		{
            GameObject c = deckCards.main[i];
            Cardmaker mkr = c.GetComponent<Cardmaker>();
            GameObject card = Instantiate(buildCardPre, mainCardList.transform);
            card.GetComponent<BuildCard>().initalize(mkr,i, this);
		}
        maidCardScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
	}
    public void addCard(int index, Cardmaker mkr, int count = 1)
	{
		if (currectDeckList.ContainsKey(index))
		{
			if (currectDeckList[index])
			{
                currectDeckList[index].incrementCount(count);
                return;
            }
		}

        GameObject cc = Instantiate(cardCountPre, mainDeckList.transform);
        CardCountUI ccUI = cc.GetComponent<CardCountUI>();
        ccUI.setCardmaker(mkr, index);
        ccUI.incrementCount(count);
        currectDeckList[index] = ccUI;


    }
    // Update is called once per frame
    void Update()
    {
        mainDeckCount.text = currentDeckSize.ToString();
    }


    public void SaveDeck()
	{
        Dictionary<int, int> finalDeck = new Dictionary<int, int>();
        foreach(int index in currectDeckList.Keys)
		{
			if (currectDeckList[index])
			{
                finalDeck.Add(index, currectDeckList[index].count);
			}
		}
        DeckRW.saveDeck("test", finalDeck);
	}
    public void LoadDeck()
	{
        Dictionary<int, int> loadedDeck = DeckRW.loadDeck("test");
        foreach(int index in loadedDeck.Keys)
		{
            addCard(index, deckCards.main[index].GetComponent<Cardmaker>(),loadedDeck[index]);
		}
    }
}
