using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static StatBlock;

public abstract class CardUI : MonoBehaviour
{
    public Image cardArt;
    public Text cardTitle;
    public CostUI cardCost;
    public Image cardBG;
    public TextMeshProUGUI cardBody;
    public GameObject selection;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public abstract void populateSelf(Cardmaker maker, bool isPrefab);
    protected void populateArt(Sprite art)
	{
        cardArt.sprite = art;
    }
    protected void populateArt(GameObject model/*, int retry =0*/)
    {
        //if (retry > 3)
        //{
        //          return;
        //}

        List<GameObject> dontRender = new List<GameObject>();
        foreach(Transform c in model.transform)
		{
            if (c.tag == "BuffScaler")
			{
                dontRender.Add(c.gameObject);
                c.gameObject.SetActive(false);
			}

        }
        Texture2D art = RuntimePreviewGenerator.GenerateModelPreview(model.transform,120,80);
        foreach(GameObject o in dontRender)
		{
            o.SetActive(true);
		}

        //Texture2D art = AssetPreview.GetAssetPreview(prefab);
		try
		{
            Rect rec = new Rect(0, 0, art.width, art.height);
            cardArt.sprite = Sprite.Create(art, rec, new Vector2(0.5f, 0.5f), 100);
        }
		catch
		{
            Debug.Log("Sprite load error");
            //StartCoroutine(ArtCoRou(prefab,retry+1));
        }
    }
    //   IEnumerator ArtCoRou(GameObject prefab,int retry)
    //{
    //       yield return new WaitForSeconds(1);
    //       populateArt(prefab, retry);
    //   }

    protected void populateTitle(string title)
	{
        cardTitle.text = title;
	}
    protected virtual void populateCost(Cardmaker mkr)
    {
        cardCost.setCost(mkr.resourceCost.ToString(), new CostUI.costTypes(true,true,false));
        //cardCost.transform.parent.GetComponent<Image>().color = GameColors.resources;
    }
    public void select(bool isSelected, Color c)
	{
        selection.SetActive(isSelected);
        c.a = 0.6f;
        selection.GetComponent<Image>().color = c;
	}

}
