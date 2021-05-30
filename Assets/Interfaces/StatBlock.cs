using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public static class StatBlock 
{
    [Serializable]
    public enum StatType : byte
    {
        health=0,
        attack,
        movement,
        range,
        
        //resourceCost,
        resourceIncome=10,
        resourceMax,
        cardDraw,
        resourceSpend,

        armor =20,
        piercing,
        regen,
        agile,
        overwhelm,
        charge,
        bypass,
        frontline,
        ghost,
        slow,
        addOn,
        bloodlust,
        //collateral,
        //cleave
        






    }    

    [Serializable]
    public struct Stat
    {
        public StatType type;
        public float value;
        public Stat(StatType t, float v)
        {
            type = t;
            value = v;
        }
        public Stat(Stat s)
        {
            type = s.type;
            value = s.value;
        }
    }
    

    



}

