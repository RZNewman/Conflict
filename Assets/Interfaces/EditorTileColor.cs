using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class EditorTileColor : MonoBehaviour
{


	// Start is called before the first frame update
	private void Update()
	{
		//GetComponent<Tile>().deriveType();
		GetComponent<Tile>().checkTypeVis(true);
	}
	private void Start()
	{
		//GetComponent<Tile>().fillPresets();
		//GetComponent<MeshRenderer>().sharedMaterial = GetComponent<Tile>().baseMat;
	}
}
