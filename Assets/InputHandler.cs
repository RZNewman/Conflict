using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{

    public Tile target;
    public bool click;

    public Vector3 pan;
    public float panStrength= 4f;

    public bool endTurn;

    Vector3 last;
    Vector3 look;

    //float screenThresholdMin = 0.1f;
    //float screenThresholdMax = 0.05f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void updateInputs()
    {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        last = look;
        look = Input.mousePosition;

        
        pan = Vector3.zero;
        
        if (screenRect.Contains(look))
        {
            RaycastHit hit;
            Ray cursorRay = Camera.main.ScreenPointToRay(look);
            if (Physics.Raycast(cursorRay,out hit,50, LayerMask.GetMask("Tiles"))){
                //Debug.Log(hit.collider.name);
                target = hit.collider.GetComponent<Tile>();

			}
			else
			{
                target = null;
			}

			if (Input.GetMouseButton(1))
			{
                pan = last - look;
                pan /= Screen.width;
                pan *= panStrength;
			}
			

        }
        click = Input.GetMouseButtonDown(0) && ! EventSystem.current.IsPointerOverGameObject();
        endTurn = Input.GetKeyDown(KeyCode.Space);

        

        
    }
}
