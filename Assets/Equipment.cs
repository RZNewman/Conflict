using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Targeting;

public class Equipment : Cardmaker
{

    public Unit.unitType type;
    // Start is called before the first frame update

    [Client]
    public override void register() //prefab
    {
        if (!NetworkClient.prefabs.ContainsValue(gameObject))
        {
            NetworkClient.RegisterPrefab(gameObject);
            foreach (GameObject aPre in GetComponent<Buff>().abilitiesPre)
            {
                aPre.GetComponent<Ordnance>().register();
            }
        }
    }
    public override GameObject findCardPrefab()
    {
        return (GameObject)Resources.Load("DynamicEquipCard", typeof(GameObject));
    }
    public override GameObject findCardTemplate()
    {
        return (GameObject)Resources.Load("EquipCardPre", typeof(GameObject));

    }
    public override Color getColor()
    {

        return GameColors.equipment;

    }
    protected override int getOrderType()
    {
        return 2;
    }

    public override void modifyCardAfterCreation(GameObject o)
    {
        EquipCard card = o.GetComponent<EquipCard>();
        card.setCardmaker(this);
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
