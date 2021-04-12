using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Targeting;

public class Equipment : MonoBehaviour, Cardmaker
{
    public int resourceCost;
    public Sprite cardArt;
    public Unit.unitType type;
    // Start is called before the first frame update

    [Client]
    public void register() //prefab
    {
        if (!ClientScene.prefabs.ContainsValue(gameObject))
        {
            ClientScene.RegisterPrefab(gameObject);
            foreach (GameObject aPre in GetComponent<Buff>().abilitiesPre)
            {
                aPre.GetComponent<Ordnance>().register();
            }
        }
    }
    public GameObject findCardPrefab()
    {
        return (GameObject)Resources.Load("DynamicEquipCard", typeof(GameObject));
    }

    public void modifyCardAfterCreation(GameObject o)
    {
        EquipCard card = o.GetComponent<EquipCard>();
        card.setEquipPre(this);
        Targeting tar = o.GetComponent<Targeting>();
        Rule[] mod = new Rule[tar.rules.Length];
        for (int i = 0; i < tar.rules.Length; i++)
        {
            mod[i] = tar.rules[i];
            if (mod[i].type == TargetRule.unitType)
            {
                mod[i].value = (int)type;
            }
        }

        tar.rules = mod;
    }
}
