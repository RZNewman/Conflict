using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OrdCard : Card
{
	OrdCardUI cardVis;
	GameObject ordPre;

	[SyncVar]
	string ordPrePath;

	[Server]
	public void setEquipPre(Ordnance o)
	{
		string name = o.nameString;
		string path = "Ordnance/";
		ordPrePath = path + name;
	}


	// Start is called before the first frame update
	protected override void Start()
	{


		if (isClient)
		{
			ordPre = (GameObject)Resources.Load(ordPrePath, typeof(GameObject));
			//if (ClientScene.prefabs.ContainsKey())
			//{

			//}

			//TODO Hide register until action
			//if (!ClientScene.prefabs.ContainsValue(ordPre))
			//{
			//	ClientScene.RegisterPrefab(ordPre);
			//}
			ordPre.GetComponent<Ordnance>().register();
		}
		base.Start();
	}
	public override void playCard(Tile target)
	{

		ordPre = (GameObject)Resources.Load(ordPrePath, typeof(GameObject));
		GameObject ability = Instantiate(ordPre);
		Ability ab = ability.GetComponent<Ability>();
		ab.initialize();
		ab.cast(target, team, null);
		Destroy(ability);
		Destroy(gameObject);
	}

	protected override void populateTemplate()
	{
		getTemplate("OrdCardPre");
		cardBody = Instantiate(cardTemplatePre, transform);
		cardVis = cardBody.GetComponent<OrdCardUI>();
		Ability prefabAbilityScript = ordPre.GetComponent<Ability>();
		Ordnance prefabOrdScript = ordPre.GetComponent<Ordnance>();
		//
		if (prefabOrdScript.cardArt != null)
		{
			cardVis.populateArt(prefabOrdScript.cardArt);
		}
		else
		{
			cardVis.populateArt(ordPre);
		}

		cardVis.populateTitle(ordPre.name);
		cardVis.setBackground();
		resourceCost = prefabOrdScript.resourceCost;
		cardVis.populateCost(resourceCost.ToString());

		cardVis.populateBody(prefabAbilityScript);
	}

}
