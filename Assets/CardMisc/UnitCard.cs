using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static StatBlock;
using UnityEditor;
using System.Linq;

public class UnitCard : Card
{
    

    //GameObject unitPre;

    


	protected override void inspect()
	{
        gm.clientPlayer.cardInspect(cardBody, CardInspector.inspectType.card, sourceCardmaker.GetComponent<StatHandler>().prefabStats());
    }
	[Server]
    public override void setCardmaker(Cardmaker c)
    {
        string name = c.name;
        string path = "Units/";
		if ((c as Unit).isStructure)
		{
            path = "Structures/";
		}
        sourceCardmakerPath = path + name;
        base.setCardmaker(c);
	}

    [Server]
    public override void playCard(Tile target)
	{
        //Debug.Log("Played on Tile:" + target);
        //GameObject pawn =Instantiate(unitPre, target.transform.position, Quaternion.LookRotation(GameManager.dirs[team]));
        //sourceCardmaker = (GameObject)Resources.Load(sourceCardmakerPath, typeof(GameObject));
        GameObject pawn = Instantiate(sourceCardmaker);
        Unit u = pawn.GetComponent<Unit>();
        u.initialize(team);
        u.provideName(sourceCardmaker.name);                     
        NetworkServer.Spawn(pawn);
        target.assignUnit(u);
        gm.viewPipe.RpcAddViewEvent(new ViewPipeline.ViewEvent(ViewPipeline.ViewType.unitPlay, u.netId, target.netId, Time.time));
        //target.RpcAssignUnit(u.netId, false);
        //Destroy(gameObject);
	}

	// Start is called before the first frame update


    protected override void Update()
    {
        base.Update();
    }


}
