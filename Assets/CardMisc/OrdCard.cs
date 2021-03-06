using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OrdCard : Card
{

	//GameObject ordPre;

	

	[Server]
	public override void setCardmaker(Cardmaker c)
	{
		string name = c.name;
		string path = "Ordnance/";
		sourceCardmakerPath = path + name;
		base.setCardmaker(c);
	}


	// Start is called before the first frame update

	public override void playCard(Tile target)
	{

		//sourceCardmaker = (GameObject)Resources.Load(sourceCardmakerPath, typeof(GameObject));
		GameObject ability = Instantiate(sourceCardmaker);
		ability.GetComponent<Cardmaker>().provideName(sourceCardmaker.name);
		Ability ab = ability.GetComponent<Ability>();
		ab.initialize();
		
		ab.cast(target, team, null);
		//Destroy(ability);
		NetworkServer.Spawn(ability);
		
		gm.delayedDestroy(ability);
		gm.viewPipe.QueueViewEvent(new ViewPipeline.ViewEvent(ViewPipeline.ViewType.playEffect, ab.GetComponent<NetworkIdentity>().netId, target.netId, Time.time), true);
		//Destroy(gameObject);
	}



}
