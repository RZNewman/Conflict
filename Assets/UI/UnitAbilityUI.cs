using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitAbilityUI : MonoBehaviour
{
    List<GameObject> AbilsSqs = new List<GameObject>();
    public GameObject ordSqPre;
    
    public void addAbility(AbilityRoot abilityInstance)
	{
        GameObject abilToken = Instantiate(ordSqPre, transform);
        abilToken.GetComponent<OrdSqUI>().intialize(abilityInstance.GetComponent<AbilityRoot>());
        AbilsSqs.Add(abilToken);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void clearAll()
	{
        foreach(GameObject sq  in AbilsSqs)
		{
            Destroy(sq.gameObject);
		}
	}
    // Start is called before the first frame update
    void Start()
    {



        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
