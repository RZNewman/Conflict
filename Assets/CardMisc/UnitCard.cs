using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static StatBlock;
using UnityEditor;
using System.Linq;

public class UnitCard : Card
{
    
    UnitCardUI cardVis;
    GameObject unitPre;

    [SyncVar]
    string unitPrePath;

	protected override void populateTemplate()
	{
        getTemplate("UnitCardPre");
        cardBody = Instantiate(cardTemplatePre,transform);
        cardVis = cardBody.GetComponent<UnitCardUI>();
        Unit prefabUnitScript = unitPre.GetComponent<Unit>();
		//
		if (prefabUnitScript.cardArt != null)
		{
            cardVis.populateArt(prefabUnitScript.cardArt);
        }
		else
		{
            cardVis.populateArt(unitPre);
        }
        
        cardVis.populateTitle(unitPre.name);
        cardVis.populateType(prefabUnitScript);
        StatHandler st = unitPre.GetComponent<StatHandler>();
        Dictionary<StatType, float> sts = st.prefabStats();
        resourceCost = prefabUnitScript.resourceCost;
        cardVis.populateCost(resourceCost.ToString());
        cardVis.populateValues(sts);
        cardVis.populateBody(sts,true, prefabUnitScript.abilitiesPre.Select(x =>x.GetComponent<Ability>()).ToArray());

        cardVis.modifyForStructure(prefabUnitScript.isStructure);

	}
	protected override void inspect()
	{
        gm.clientPlayer.cardInspect(cardBody, CardInspector.inspectType.card, unitPre.GetComponent<StatHandler>().prefabStats());
    }
	[Server]
    public void setUnitPre(Unit u)
	{
        string name = u.name;
        string path = "Units/";
		if (u.isStructure)
		{
            path = "Structures/";
		}
        unitPrePath = path + name;
	}

    [Server]
    public override void playCard(Tile target)
	{
        //Debug.Log("Played on Tile:" + target);
        //GameObject pawn =Instantiate(unitPre, target.transform.position, Quaternion.LookRotation(GameManager.dirs[team]));
        unitPre = (GameObject)Resources.Load(unitPrePath, typeof(GameObject));
        GameObject pawn = Instantiate(unitPre);
        Unit u = pawn.GetComponent<Unit>();
        u.initialize(team,unitPre.name);
        target.assignUnit(u);             
        NetworkServer.Spawn(pawn);
        target.RpcAssignUnit(u.netId, false);
        Destroy(gameObject);
	}

	// Start is called before the first frame update
	protected override void Start()
    {
        

        if (isClient)
		{
            unitPre = (GameObject)Resources.Load(unitPrePath, typeof(GameObject));
            //if (ClientScene.prefabs.ContainsKey())
            //{

            //}

            //TODO Hide register until action

            //if (!ClientScene.prefabs.ContainsValue(unitPre))
            //{
            //             ClientScene.RegisterPrefab(unitPre);
            //         }
            //         foreach(GameObject aPre in unitPre.GetComponent<Unit>().abilitiesPre)
            //          {
            //             if (!ClientScene.prefabs.ContainsValue(aPre))
            //             {
            //                 ClientScene.RegisterPrefab(aPre);
            //             }
            //         }
            unitPre.GetComponent<Unit>().register();
            
		}
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }


}
