using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Operations 
{
    //private static Random rng = new Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            
            int k = Random.Range(0, n - 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string Capatialize(string inp)
	{
        for(int i =0; i<inp.Length; i++)
		{
            char c = inp[i];
			if (char.IsLower(c))
			{
                return inp.Substring(0, i) + char.ToUpper(c) + inp.Substring(i + 1);

			}
		}

        return inp;
	}


    public static Texture2D textureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
            return sprite.texture;
    }
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    public static bool Contains<T>(this IEnumerable<T> listT, T  ele)
	{
        foreach(T t in listT)
		{
            if(t.Equals(t))
			{
                return true;
			}
		}
        return false;
	}
    public static int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
