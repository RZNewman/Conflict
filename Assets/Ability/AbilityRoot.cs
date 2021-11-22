using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ViewPipeline;

public class AbilityRoot : Cardmaker, TeamOwnership, PseudoDestroy
{


	#region unitAbility
	public Unit caster;
    public int maxUses=0;

    [SyncVar]
    int currentUses;

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
			if (maxUses > 0)
			{
                currentUses--;
			}
            caster.cast();
            return true;
        }
        return false;
        
        

    }
    public bool hasCharges()
	{
        return maxUses == 0 || currentUses > 0;
	}
    public int getCharges()
	{
        return currentUses;
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

    void Start()
	{
		if (isServer)
		{
            if (maxUses > 0)
            {
                currentUses = maxUses;
            }
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
    public string toDesc(bool isPrefab)
    {

        string prefix;
        string desc = GetComponent<Ability>().toDesc(trigger == AbilityRoot.TriggerType.none);
        desc = Operations.Capatialize(desc);
        switch (trigger)
        {
            case AbilityRoot.TriggerType.onBuffTick:
                prefix = "On Tick";
                break;
            case AbilityRoot.TriggerType.onDeath:
                prefix = "On Death";
                break;
            default:
                prefix = resourceCost.ToString();
				if (maxUses > 0)
				{
                    int charg;
                    switch (isPrefab)
					{
                        case true:
                            charg = maxUses;
                            break;
                        case false:
                            charg = currentUses;
                            break;
					}
                    prefix += ": charges, " + charg;
				}
                break;
        }

        return prefix + ": " + desc + "\n";

    }
}
