using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTileAura : Ability
{
    public GameObject auraGiven;
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc;

        desc = string.Format("{0}",
            auraGiven.GetComponent<Aura>().toDesc(true)
            );

        return desc + " " + targetingDesc(sayTarget, plural,"on");
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {

            GameObject aura = Instantiate(auraGiven);
            aura.transform.parent = target.transform;
            aura.transform.localPosition = Vector3.zero;
            Aura a = aura.GetComponent<Aura>();
            a.setTeam(team);
            a.initailize();
            a.updateLocation(target);
            NetworkServer.Spawn(aura);
            a.RpcAssignParent(target.netId);
            
            return true;
        }

        return false;
    }
    protected override void onRegister()
    {
        if (!NetworkClient.prefabs.ContainsValue(auraGiven))
        {
            NetworkClient.RegisterPrefab(auraGiven);

        }
        auraGiven.GetComponent<Buff>().register();
    }
}
