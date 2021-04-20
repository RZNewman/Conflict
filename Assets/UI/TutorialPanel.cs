using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPanel : MonoBehaviour
{
	public List<GameObject> cameraScenes = new List<GameObject>();
	public void activate(bool active)
	{
		foreach(GameObject o in cameraScenes)
		{
			//Debug.Log(o);
			Camera c = o.GetComponentInChildren<Camera>();
			c.enabled = active;
		}
	}
}
