using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ViewPipeline;

public class AbilityRoot : Cardmaker, TeamOwnership, PseudoDestroy
{


	#region unitAbility
	public Unit caster;

    public enum TriggerType
    {
        none,
        onDeath,
        onBuffTick,
    }
    public TriggerType trigger = TriggerType.none;
    public int getTeam()
    {
        return caster.teamIndex;
    }

    public bool castAbil(Tile target, int team, Tile source)
	{
        if(GetComponent<Ability>().cast(target, team, source))
		{
            FindObjectOfType<GameManager>().viewPipe.QueueViewEvent(new ViewEvent(ViewType.playEffect, netId, target.netId, Time.time), true);
            caster.cast();
            return true;
        }
        return false;
        
        

    }

    public void eventAbil(Tile target, int team, Tile source)
    {
        FindObjectOfType<GameManager>().viewPipe.QueueViewEvent(new ViewEvent(ViewType.playEffectTrigger, netId, target.netId, Time.time));
        GetComponent<Ability>().cast(target, team, source);


    }
    [Client]
    public override void register() //prefab
    {
        if (!NetworkClient.prefabs.ContainsValue(gameObject))
        {
            NetworkClient.RegisterPrefab(gameObject);
            
        }
        GetComponent<Ability>().register();
    }
    public void PDestroy(bool isSev)
    {
		if (caster)
		{
            caster.removeAbility(this);
		}
	}
	#endregion
	public override GameObject findCardPrefab()
    {
        return (GameObject)Resources.Load("DynamicOrdCard", typeof(GameObject));
    }
    public override GameObject findCardTemplate()
    {
        return (GameObject)Resources.Load("OrdCardPre", typeof(GameObject));

    }
    public override Color getColor()
    {

        return GameColors.ordnance;

    }
    protected override int getOrderType()
    {

        return 3;
    }


    public override void modifyCardAfterCreation(GameObject o)
    {
        OrdCard card = o.GetComponent<OrdCard>();
        card.setCardmaker(this);

        Targeting tar = o.GetComponent<Targeting>();


        tar.rules = GetComponent<Targeting>().rules;
    }
}
