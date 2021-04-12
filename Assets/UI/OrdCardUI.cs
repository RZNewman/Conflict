using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrdCardUI : CardUI
{
    public void populateBody(Ability ab)
	{
        string desc = ab.toDesc();
        desc = Operations.Capatialize(desc);
        cardBody.text = desc;
    }
    public void setBackground()
    {
        cardBG.color = GameColors.ordnance;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
