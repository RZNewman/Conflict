using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveBuff : Ability
{
    public GameObject buffGiven;
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc;

        desc = string.Format("apply: {0}",
            buffGiven.GetComponent<Buff>().toDesc()
            );

        return desc + " " + targetingDesc(sayTarget, plural);
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            Unit u = target.getOccupant();
            if (u)
            {
                GameObject buff = Instantiate(buffGiven);
                buff.transform.parent = u.transform;
                buff.transform.localPosition = Vector3.zero;
                Buff b = buff.GetComponent<Buff>();
                b.setTeam(team);
                u.addBuff(b);
                NetworkServer.Spawn(buff);
                b.RpcAssignParent(u.netId);
            }
            return true;
        }

        return false;
    }
	protected override void onRegister()
	{
        if (!NetworkClient.prefabs.ContainsValue(buffGiven))
        {
            NetworkClient.RegisterPrefab(buffGiven);

        }
        buffGiven.GetComponent<Buff>().register();
    }
}