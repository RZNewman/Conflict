using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHP : Ability
{
    public int setHP;
    public override string toDesc(bool sayTarget = true, bool plural = false)
    {
        string desc = string.Format("set health to {0}", setHP);

        return desc + " " + targetingDesc(sayTarget, plural, "for");
    }
    public override bool cast(Tile target, int team, Tile source)
    {
        if (GetComponent<Targeting>().evaluate(target, team, source))
        {
            Unit u = target.getOccupant();
            if (u)
            {
                int maxHP = u.stat.getStat(StatBlock.StatType.health);
                u.setHealth(maxHP);

                GameObject buff = Instantiate(GameStaticPrefabs.AbilityBuffPre);     
                buff.transform.parent = u.transform;
                buff.transform.localPosition = Vector3.zero;
                BuffStat b = buff.GetComponent<BuffStat>();
                b.setPrefabStat(StatBlock.StatType.health, setHP - maxHP);
                b.setTeam(team);
                u.addBuff(b);
                NetworkServer.Spawn(buff);
                b.RpcAssignParent(u.netId);

            }
            return true;
        }

        return false;
    }

}