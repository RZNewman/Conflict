using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverText : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    public string description;
    public GameObject hoverObjPre;

    GameObject currentObj;

    bool hovered = false;
    bool inspecting = false;
    float currentTime = 0;
    void Update()
    {
        //Debug.Log(hovered);
        if (hovered)
        {
            currentTime += Time.deltaTime;

            if (currentTime > GameConstants.hoverInspectTime)
            {
                currentTime = GameConstants.hoverInspectTime;
                if (!inspecting)
                {
                    GameObject canv = GameObject.FindGameObjectWithTag("Canvas");
                    currentObj = Instantiate(hoverObjPre, canv.transform);
                    currentObj.GetComponentInChildren<Text>().text = description;
                    inspecting = true;

                }

            }
        }
        else
        {

            currentTime = 0;

            if (inspecting)
            {
                Destroy(currentObj);
                inspecting = false;
            }

        }

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }
}
