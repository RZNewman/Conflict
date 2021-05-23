using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static StatBlock;
using System.Linq;

public class EquipCard : Card
{

	//GameObject equipPre;



	[Server]
	public override void setCardmaker(Cardmaker c)
	{
		string name = c.name;
		string path = "Equipment/";
		sourceCardmakerPath = path + name;
		base.setCardmaker(c);
	}


	public override void playCard(Tile target)
	{

		//sourceCardmaker = (GameObject)Resources.Load(sourceCardmakerPath, typeof(GameObject));
		GameObject buff = Instantiate(sourceCardmaker);
		Buff b = buff.GetComponent<Buff>();
		buff.GetComponent<Cardmaker>().provideName(sourceCardmaker.name);
		Unit u = target.getOccupant().GetComponent<Unit>();
		b.initailize(u);

		buff.transform.parent = u.transform;
		buff.transform.localPosition = Vector3.zero;
		u.addBuff(b);
		//u.initialize(team, unitPre.name);
		//target.assignUnit(u);
		NetworkServer.Spawn(buff);
		b.RpcAssignUnit(target.getOccupant().netId);
		gm.viewPipe.RpcAddViewEvent(new ViewPipeline.ViewEvent(ViewPipeline.ViewType.playEffect, b.netId, target.netId, Time.time));
		//Destroy(gameObject);
	}

	protected override void inspect()
	{
		gm.clientPlayer.cardInspect(cardBody, CardInspector.inspectType.card, sourceCardmaker.GetComponent<StatHandler>().prefabStats());
	}
	// Start is called before the first frame update

}
