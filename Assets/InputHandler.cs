using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{

    public Tile target;
    public bool click;
    public bool cancel;

    public Vector3 pan;
    public float panStrength= 4f;

    public float zoom;
    public float zoomStrength = 1f;

    public bool endTurn;

    Vector3 last;
    Vector3 look;

    //float screenThresholdMin = 0.1f;
    //float screenThresholdMax = 0.05f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    


    //float panMod = 0.35f;
    // Update is called once per frame
    public void updateInputs()
    {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        last = look;
        look = Input.mousePosition;

        zoom = Input.mouseScrollDelta.y* zoomStrength;
		if (Input.GetKeyDown(KeyCode.KeypadPlus))
		{
            zoom += 3 * zoomStrength;
		}
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            zoom -= 3 * zoomStrength;
        }


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
            //Pan with hold right click

			if (Input.GetMouseButton(1))
			{
                pan = last - look;
                pan /= Screen.width;
                pan *= panStrength;
			}
            //Pan with screen edge
            /*
			else if (! Operations.IsPointerOverUIObject())
			{
                float leftPer = look.x / Screen.width;
                float rightPer = (Screen.width - look.x) / Screen.width;
                float downPer = look.y / Screen.height;
                float upPer = (Screen.height - look.y) / Screen.height;
                void alterPan(float per, bool isVert, bool neg)
                {
                    int mod = neg ? -1 : 1;
                    if (per < 0.1f)
                    {
                        float str;
                        if (per < 0.03f)
                        {
                            str = 2 * panStrength*panMod;
                        }
                        else
                        {
                            str = panStrength*panMod;
                        }

                        if (isVert)
                        {
                            pan.y += mod * str * Time.deltaTime;
                        }
                        else
                        {
                            pan.x += mod * str * Time.deltaTime;
                        }
                    }
                }
                alterPan(leftPer, false, true);
                alterPan(rightPer, false, false);
                alterPan(upPer, true, false);
                alterPan(downPer, true, true);
            }
            */

        }
        
        click = Input.GetMouseButtonDown(0) && ! EventSystem.current.IsPointerOverGameObject();
        cancel = Input.GetMouseButtonDown(1);
        endTurn = Input.GetKeyDown(KeyCode.Space);

        

        
    }
}
