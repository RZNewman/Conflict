using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StatBlock;

public class TileUI : MonoBehaviour
{
    public Text attack;
    public Text defense;
	public Text movement;
	public Text range;
	public Image piercing;
	public Image armor;
	public Image ghost;
	public Image bypass;
	public Image canAttack;
	public Image canMove;
	public Image canCast;

	public GameObject selectorPre;
	public GameObject info;

	//List<GameObject> selectors =  new List<GameObject>();

	Unit occ;
	// Start is called before the first frame update
	private void Start()
	{
		attack.color = GameColors.attack;
		defense.color = GameColors.defense;
		movement.color = GameColors.move;
		range.color = GameColors.range;

		piercing.color = GameColors.UIEnhanced;
		armor.color = GameColors.UIEnhanced;
		ghost.color = GameColors.UIEnhanced;
		bypass.color = GameColors.UIEnhanced;

		
		//gameObject.SetActive(false);
	}
	public void activate(Unit u)
	{
		//Debug.Log("Activate");
		occ = u;
		refresh();
		info.SetActive(true);
		
	}
	public void deactivate()
	{
		//Debug.Log("Deactivate");
		info.SetActive(false);
	}
	public void refresh()
	{
		//Debug.Log("Refresh");
		attack.text = Mathf.Max(occ.stat.getStat(StatType.attack),0).ToString();
		defense.text = (occ.getHeath()+ occ.stat.getStat(StatType.armor)).ToString();
		movement.text = occ.getMove().ToString();
		range.text = occ.stat.getStat(StatType.range).ToString();

		piercing.gameObject.SetActive(occ.stat.getBool(StatType.piercing));
		armor.gameObject.SetActive(occ.stat.getStat(StatType.armor)>0);
		bypass.gameObject.SetActive(occ.stat.getBool(StatType.bypass));
		ghost.gameObject.SetActive(occ.stat.getBool(StatType.ghost));
		canAttack.gameObject.SetActive(occ.canAttackVisual);
		canCast.gameObject.SetActive(occ.canCastVisual);
		canMove.gameObject.SetActive(occ.canMoveVisual);
	}

	
	public GameObject select(SelectorUI.SelectType type, bool isHover)
	{
		GameObject s = Instantiate(selectorPre, transform);
		//selection.SetActive(true);
		s.GetComponent<SelectorUI>().select(type, isHover);
		//selectors.Add(s);
		return s;
	}
	//public void deselect()
	//{
	//	foreach(GameObject s in selectors)
	//	{
	//		Destroy(s);
	//	}
	//}
}
