using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    int childPanelIndex = 0;
	// Start is called before the first frame update

	private void OnEnable()
	{
        changePanelDelta(0);
	}
	public void changePanelDelta(int delta)
	{
        switchToPanel(childPanelIndex + delta);
	}
    void switchToPanel(int panel)
	{
        if(panel >= 0 && panel < transform.childCount)
		{
            GameObject p = transform.GetChild(childPanelIndex).gameObject;
            p.GetComponent<TutorialPanel>().activate(false);
            p.SetActive(false);
            childPanelIndex = panel;
            p = transform.GetChild(childPanelIndex).gameObject;          
            p.SetActive(true);
            p.GetComponent<TutorialPanel>().activate(true);

        }
	}


}
