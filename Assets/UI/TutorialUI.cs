using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    int childPanelIndex = 0;
	// Start is called before the first frame update

	private void OnEnable()
	{
		switchToPanel(0);
	}
	public void changePanelDelta(int delta)
	{
        switchToPanel(childPanelIndex + delta);
	}
    void switchToPanel(int panel)
	{
        if(panel >= 0 && panel < transform.childCount)
		{
			switchPanel(false);
            childPanelIndex = panel;
			switchPanel(true);

        }
	}
	void switchPanel(bool isOn)
	{
		GameObject p = transform.GetChild(childPanelIndex).gameObject;
		p.GetComponent<TutorialPanel>().activate(isOn);
		p.SetActive(isOn);
	}
	private void OnDisable()
	{
		switchPanel(false);
	}

}
