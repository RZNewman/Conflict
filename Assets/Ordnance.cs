using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ordnance : MonoBehaviour, Cardmaker, TeamOwnership
{
    public int resourceCost;
    public Sprite cardArt;
    public string nameString;
	#region unitAbility
	public Unit caster;

    public int getTeam()
    {
        return caster.teamIndex;
    }

    public bool castAbil(Tile target, int team, Tile source)
	{
        if(GetComponent<Ability>().cast(target, team, source))
		{
            caster.cast();
            return true;
        }
        return false;
        
        

    }
    [Client]
    public void register() //prefab
    {
        if (!ClientScene.prefabs.ContainsValue(gameObject))
        {
            ClientScene.RegisterPrefab(gameObject);
            
        }
    }
    private void OnDestroy()
	{
		if (caster)
		{
            caster.removeAbility(this);
		}
	}
	#endregion
	public GameObject findCardPrefab()
    {
        return (GameObject)Resources.Load("DynamicOrdCard", typeof(GameObject));
    }

	

	public void modifyCardAfterCreation(GameObject o)
    {
        nameString = name;
        OrdCard card = o.GetComponent<OrdCard>();
        card.setEquipPre(this);

        Targeting tar = o.GetComponent<Targeting>();


        tar.rules = GetComponent<Targeting>().rules;
    }
}
