using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CostUI : MonoBehaviour
{
    public Image power;
    public Image supply;
    public Image material;
    public Text cost;

    public struct costTypes
	{
        public bool pw;
        public bool sp;
        public bool mt;
        public costTypes(bool p, bool s, bool m)
		{
            pw = p;
            sp = s;
            mt = m;
		}
    }
    public void setCost(string ct, costTypes t)
	{
        cost.text = ct;
        power.gameObject.SetActive(t.pw);
        supply.gameObject.SetActive(t.sp);
        material.gameObject.SetActive(t.mt);

    }
}
