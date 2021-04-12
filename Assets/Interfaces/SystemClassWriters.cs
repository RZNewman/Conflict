using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public static class SystemClassWriters 
{
    public static void WriteTargetRule(this NetworkWriter writer, Targeting.TargetRule targetR)
    {
        writer.WriteByte((byte)targetR);
    }
    public static void WriteStatType(this NetworkWriter writer, StatBlock.StatType stat)
    {
        writer.WriteByte((byte)stat);
    }

      
    


}

