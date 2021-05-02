using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using static StatBlock;
using System.Linq;

public class EquipCard : Card
{
	EquipCardUI cardVis;
	GameObject equipPre;

	[SyncVar]
	string equipPrePath;

	[Server]
	public void setEquipPre(Equipment e)
	{
		string name = e.name;
		string path = "Equipment/";
		equipPrePath = path + name;
	}
	protected override void Start()
	{


		if (isClient)
		{
			equipPre = (GameObject)Resources.Load(equipPrePath, typeof(GameObject));
			//if (ClientScene.prefabs.ContainsKey())
			//{

			//}

			//TODO Hide register until action
			//if (!ClientScene.prefabs.ContainsValue(equipPre))
			//{
			//	ClientScene.RegisterPrefab(equipPre);
			//}
			equipPre.GetComponent<Equipment>().register();
		}
		base.Start();
	}

	public override void playCard(Tile target)
	{

		equipPre = (GameObject)Resources.Load(equipPrePath, typeof(GameObject));
		GameObject buff = Instantiate(equipPre);
		Buff b = buff.GetComponent<Buff>();
		
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
		Destroy(gameObject);
	}

	protected override void populateTemplate()
	{
		getTemplate("EquipCardPre");
		cardBody = Instantiate(cardTemplatePre, transform);
		cardVis = cardBody.GetComponent<EquipCardUI>();
		Equipment prefabEquipScript = equipPre.GetComponent<Equipment>();
		//
		if (prefabEquipScript.cardArt != null)
		{
			cardVis.populateArt(prefabEquipScript.cardArt);
		}
		else
		{
			cardVis.populateArt(equipPre);
		}
		cardVis.setBackground();
		cardVis.populateTitle(equipPre.name);
		cardVis.populateType(prefabEquipScript);
		StatHandler st = equipPre.GetComponent<StatHandler>();
		Dictionary<StatType, float> sts = st.prefabStats();
		resourceCost = prefabEquipScript.resourceCost;
		cardVis.populateCost(resourceCost.ToString());
		cardVis.populateBody(sts, false, equipPre.GetComponent<Buff>().abilitiesPre.Select(x => x.GetComponent<Ability>()).ToArray());

	}

	// Start is called before the first frame update

}
