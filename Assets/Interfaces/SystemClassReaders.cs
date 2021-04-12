using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SystemClassReaders
{
    public static Targeting.TargetRule ReadTargetRule(this NetworkReader reader)
    {
        return (Targeting.TargetRule)reader.ReadByte();
    }
    public static StatBlock.StatType ReadStatType(this NetworkReader reader)
    {
        return (StatBlock.StatType)reader.ReadByte();
    }

    
}

