using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
}
