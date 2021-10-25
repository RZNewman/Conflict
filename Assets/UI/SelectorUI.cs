using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorUI : MonoBehaviour
{
    public enum SelectType
    {
        active,
        move,
        threat,
        attack,
        ability
    }
	public Sprite ring;
	public Sprite attack;
	public Sprite hover;

	public void select(SelectType type, bool isHover)
	{
		SpriteRenderer s = GetComponent<SpriteRenderer>();
		//selection.SetActive(true);
		switch (type)
		{
			case SelectType.active:
				s.color = GameColors.activeSelect;
				break;
			case SelectType.move:
				s.color = GameColors.moveSelect;
				break;
			case SelectType.threat:
				s.color = GameColors.threatSelect;
				break;
			case SelectType.attack:
				s.color = GameColors.attackSelect;
				break;
			case SelectType.ability:
				s.color = GameColors.abilitySelect;
				break;
		}
		if (isHover)
		{
			s.sprite = hover;
		}
		else if(type == SelectType.attack)
		{
			s.sprite = attack;
		}
		else
		{
			s.sprite = ring;
		}
	}
	// Start is called before the first frame update
	void Start()
    {
        
    }


}
