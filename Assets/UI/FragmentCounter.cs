using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FragmentCounter : MonoBehaviour
{
	public Sprite[] fragments;

	public void displayFragements(int f)
	{
		if (f > fragments.Length) f = fragments.Length - 1;
		GetComponent<Image>().sprite = fragments[f];
	}


}
