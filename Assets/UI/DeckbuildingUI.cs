using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static StatBlock;

public class DeckbuildingUI : MonoBehaviour
{
    public Deck deckCards;

    public GameObject deckRoot;
    public GameObject decksMadeList;
    public GameObject decksMadeScroll;
    public Text newDeckName;

    public GameObject mainRoot;
    public GameObject mainCardList;
    public GameObject mainCardScroll;
    public GameObject mainDeckList;
    public GameObject mainDeckScroll;
    public Text mainDeckCount;
    public Text mainDeckMax;

    public GameObject strcRoot;
    public GameObject strcCardList;
    public GameObject strcCardScroll;
    public GameObject strcDeckList;
    public GameObject strcDeckScroll;
    public Text strcDeckCount;
    public Text strcDeckMax;

    public enum deckType
	{
        deck,
        main,
        structure
	}
    deckType mode = deckType.deck;

    public GameObject buildCardPre;
    public GameObject cardCountPre;
    public GameObject deckPre;
    public GameObject deckExportPre;

    public int currentMainDeckSize = 0;
    public int currentStrcDeckSize = 0;

    Dictionary<int, CardCountUI> currentMainDeckList = new Dictionary<int, CardCountUI>();
    Dictionary<int, CardCountUI> currentStrcDeckList = new Dictionary<int, CardCountUI>();

    string currentEditingDeck="";
    DeckUI currentDeckUI;
    Dictionary<string, DeckUI> deckObjs = new Dictionary<string, DeckUI>();

    CardInspector inspect;

    // Start is called before the first frame update
    void Start()
    {
        inspect = FindObjectOfType<CardInspector>();
        mainDeckMax.text = GameConstants.mainDeckSize.ToString();
        strcDeckMax.text = GameConstants.structureDeckSize.ToString();
        createPlayableCards();
        selectDeck(DeckRW.getDefaultDeck());
        //LoadDeck();
    }

    public void cardInspect(GameObject body, Dictionary<StatType, float> stats = null)
    {
        inspect.inspect(body, CardInspector.inspectType.cardmakerPre, 2, stats);
    }
    public void cardUnInspect(GameObject body)
    {
        inspect.uninspect(body);
    }

    public void swapMode(deckType newMode)
	{

        modeToggle(false);
        mode = newMode;
        modeToggle(true);

	}  
    public void swapMode(int newMode)
    {

        swapMode((deckType)newMode);

    }
    void modeToggle(bool on)
	{
        ScrollRect scroll = null;
        switch (mode)
        {
            case deckType.main:
                mainRoot.gameObject.SetActive(on);
                scroll = mainCardScroll.GetComponent<ScrollRect>();
                break;
            case deckType.structure:
                strcRoot.gameObject.SetActive(on);
                scroll = mainCardScroll.GetComponent<ScrollRect>();
                break;
            case deckType.deck:
                deckRoot.gameObject.SetActive(on);
                scroll = mainCardScroll.GetComponent<ScrollRect>();
                break;
        }
		if (on)
		{
            scroll.verticalNormalizedPosition = 1;
		}
    }

    void createPlayableCards()
	{
        string[] decksMade = DeckRW.getDecks();
        for (int i = 0; i < decksMade.Length; i++)
        {
            GameObject d = Instantiate(deckPre, decksMadeList.transform);
            DeckUI dui = d.GetComponent<DeckUI>();
            dui.assignName(decksMade[i], this);
            deckObjs.Add(decksMade[i], dui);
        }
        
        //foreach(GameObject c in deckCards.main)
        for (int i = 0; i<deckCards.main.Count; i++)
		{
            GameObject c = deckCards.main[i];
            Cardmaker mkr = c.GetComponent<Cardmaker>();
            GameObject card = Instantiate(buildCardPre, mainCardList.transform);
            card.GetComponent<BuildCard>().initalize(mkr,i, this, deckType.main);
		}

        GameObject[] cardBuilders = mainCardList.GetComponentsInChildren<BuildCard>().Select(x => x.gameObject).ToArray();
        cardBuilders = cardBuilders.OrderBy(c=> c.GetComponent<BuildCard>().getOrder()).ToArray();
        for (int i = 0; i < cardBuilders.Length; i++)
        {
            cardBuilders[i].transform.SetSiblingIndex(i);
        }


        for (int i = 0; i < deckCards.structures.Count; i++)
        {
            GameObject c = deckCards.structures[i];
            Cardmaker mkr = c.GetComponent<Cardmaker>();
            GameObject card = Instantiate(buildCardPre, strcCardList.transform);
            card.GetComponent<BuildCard>().initalize(mkr, i, this, deckType.structure);
        }

        cardBuilders = strcCardList.GetComponentsInChildren<BuildCard>().Select(x => x.gameObject).ToArray();
        cardBuilders = cardBuilders.OrderBy(c => c.GetComponent<BuildCard>().getOrder()).ToArray();
        for (int i = 0; i < cardBuilders.Length; i++)
        {
            cardBuilders[i].transform.SetSiblingIndex(i);
        }

        mainCardScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
        strcCardScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
        decksMadeScroll.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;


    }
    public void addCard(int index, Cardmaker mkr, deckType type, int count = 1)
	{
        Dictionary<int, CardCountUI> deckList;
        GameObject deckTarget;
        if(type == deckType.main)
		{
            deckList = currentMainDeckList;
            deckTarget = mainDeckList;
        }
        else //if (type == deckType.structure)
		{
            deckList = currentStrcDeckList;
            deckTarget = strcDeckList;
        }

        if (deckList.ContainsKey(index))
		{
			if (deckList[index])
			{
                //Debug.Log("existing " + count);
                deckList[index].incrementCount(count);
                return;
            }
		}
        //Debug.Log("new " + count);
        //GameObject cc = Instantiate(cardCountPre, deckTarget.transform);
        GameObject cc = Instantiate(cardCountPre);
        
        CardCountUI ccUI = cc.GetComponent<CardCountUI>();
        ccUI.setCardmaker(mkr, index, type, this);
        ccUI.incrementCount(count);
        deckList[index] = ccUI;
        setCounterInOrder(cc, deckTarget);
        //Debug.Log(deckList.Count);
        //Debug.Log(deckList[index].count);

    }
    void setCounterInOrder(GameObject counter, GameObject list)
	{
        
        int index;
        for(index=0; index < list.transform.childCount; index++)
		{

			if (string.Compare(list.transform.GetChild(index).GetComponent<CardCountUI>().getOrder(),
                    counter.GetComponent<CardCountUI>().getOrder()

                    ) > 0
                
                )
			{
                counter.transform.SetParent(list.transform);
                counter.transform.SetSiblingIndex(index);
                return;
            }
		}
        counter.transform.SetParent(list.transform);

    }

    // Update is called once per frame
    public void deckSizeUpdate()
    {
        mainDeckCount.text = currentMainDeckSize.ToString();
        strcDeckCount.text = currentStrcDeckSize.ToString();
        //Debug.Log(mainCardScroll.GetComponent<ScrollRect>().verticalNormalizedPosition);
    }
    public void newDeck()
	{
        currentEditingDeck = newDeckName.text;
        newDeckName.GetComponent<InputField>().text = "New Deck";
        swapMode(deckType.main);
	}
    public void goToDeck()
	{
        if (!currentDeckUI) { return; }

        LoadDeck(currentEditingDeck);
        swapMode(deckType.main);

	}
    public void selectDeck(string deck)
	{
        if(currentDeckUI)
		{
            currentDeckUI.select(false);
		}
		if (deckObjs.ContainsKey(deck))
		{
            DeckUI dui = deckObjs[deck];
            dui.select(true);
            currentEditingDeck = deck;
            currentDeckUI = dui;
            exportDeck();
        }
        
    }
    public void exportDeck()
	{
		if (currentEditingDeck == "") { return; }
        GameObject exp;
        exp = GameObject.FindGameObjectWithTag("DeckExport");
		if (!exp)
		{
            exp = Instantiate(deckExportPre);
        }
        exp.GetComponent<DeckExport>().holdDeck(DeckRW.loadDeck(currentEditingDeck));
        DeckRW.setDefaultDeck(currentEditingDeck);
            

	}
    public void leaveDeck()
	{
		if (SaveDeck(currentEditingDeck))
		{
            //create deck
            GameObject d = Instantiate(deckPre, decksMadeList.transform);
            DeckUI dui = d.GetComponent<DeckUI>();
            dui.assignName(currentEditingDeck, this);
            currentDeckUI = dui;
        }

        resetDeckCounters();
        swapMode(deckType.deck);
        selectDeck(currentEditingDeck);
	}

    void resetDeckCounters()
	{
        foreach(int i in currentMainDeckList.Keys)
		{
            //Debug.Log(currentMainDeckList[i]);
            if (currentMainDeckList[i])
            {
                Destroy( currentMainDeckList[i].gameObject);
                
            }
        }
        foreach (int i in currentStrcDeckList.Keys)
        {
            //Debug.Log(currentStrcDeckList[i]);
            if (currentStrcDeckList[i])
            {
                Destroy(currentStrcDeckList[i].gameObject);
                
            }
        }
        currentMainDeckSize = 0;
        currentStrcDeckSize = 0;
        currentMainDeckList = new Dictionary<int, CardCountUI>();
        currentStrcDeckList = new Dictionary<int, CardCountUI>();
    }
    bool SaveDeck(string deckName) // true if new
	{
        Dictionary<int, int> finalMainDeck = new Dictionary<int, int>();
        Dictionary<int, int> finalStrcDeck = new Dictionary<int, int>();
        foreach (int index in currentMainDeckList.Keys)
		{
			if (currentMainDeckList[index])
			{
                //Debug.Log(currentMainDeckList[index].count);
                finalMainDeck.Add(index, currentMainDeckList[index].count);
			}
		}
        foreach (int index in currentStrcDeckList.Keys)
        {
            if (currentStrcDeckList[index])
            {
                finalStrcDeck.Add(index, currentStrcDeckList[index].count);
            }
        }
        return DeckRW.saveDeck(deckName, finalMainDeck, finalStrcDeck);

	}
    void LoadDeck(string deckName)
	{
        Dictionary<int, int>[] loadedDeck = DeckRW.loadDeck(deckName);
        foreach(int index in loadedDeck[0].Keys)
		{
            if(index!= -1)
			{
                if(loadedDeck[0][index] == 0)
				{
                    Debug.LogError("Saved count 0");
				}
                //Debug.Log(loadedDeck[0][index]);
                addCard(index, deckCards.main[index].GetComponent<Cardmaker>(), deckType.main, loadedDeck[0][index]);
            }
            
		}
		if (loadedDeck.Length > 1)
		{
            foreach (int index in loadedDeck[1].Keys)
            {
                if (index != -1)
                {
                    //Debug.Log(index);
                    if (loadedDeck[1][index] == 0)
                    {
                        Debug.LogError("Saved count 0");
                    }
                    addCard(index, deckCards.structures[index].GetComponent<Cardmaker>(), deckType.structure, loadedDeck[1][index]);
                }
            }
        }
    }
}
