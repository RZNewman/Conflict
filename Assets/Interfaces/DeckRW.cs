using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DeckRW 
{
	static readonly string deckKey = "LocalDecks";
	static readonly string deckKeyDefault = "LocalDeckDefault";
	static readonly string[] protectedKeys = new string[] { deckKey, deckKeyDefault };
	static readonly string deckDefault = "New Deck";
	public static string[] getDecks()
	{
		if (PlayerPrefs.HasKey(deckKey))
		{
			string decks = PlayerPrefs.GetString(deckKey);
			if (decks.Contains(","))
			{
				string[] decksList = decks.Split(',');
				return decksList.Skip(1).ToArray();
			}
			
		}
		return new string[0];
	}
	public static bool saveDeck(string name, Dictionary<int, int> countsMain, Dictionary<int, int> countsStrc) //true if new 
	{
		bool isNew =false;
		string tempName = name.Replace(',', 'x');
		if (protectedKeys.Contains(tempName) || tempName.Trim() == "")
		{
			tempName = deckDefault;
		} 
		string[] decks = getDecks();
		if (!decks.Contains(tempName))
		{
			PlayerPrefs.SetString(deckKey, PlayerPrefs.GetString(deckKey) + "," + tempName);
			isNew = true;
		}
		string deckStr = writeDeck(countsMain, countsStrc);
		//Debug.Log(deckStr);
		PlayerPrefs.SetString(tempName, deckStr);
		PlayerPrefs.Save();
		return isNew;
	}

	public static Dictionary<int, int>[] loadDeck(string name)
	{
		//string tempName = name.Replace(',', 'x');
		if (name != "" && PlayerPrefs.HasKey(name))
		{
			string deck = PlayerPrefs.GetString(name);

			return readDeck(deck);
			//return deck.Split('|').Select(x => x.Split('-')).ToDictionary(x => int.Parse(x[0]), x => int.Parse(x[1]));
		}

		Dictionary<int, int>[] def = new Dictionary<int, int>[2];
		def[0] = new Dictionary<int, int>();
		def[1] = new Dictionary<int, int>();
		return def;
	}

	public static string getDefaultDeck()
	{		
		
		if (PlayerPrefs.HasKey(deckKeyDefault))
		{
			return  PlayerPrefs.GetString(deckKeyDefault);
		}

		return "";
		
	}
	public static void setDefaultDeck(string deck)
	{

		PlayerPrefs.SetString(deckKeyDefault, deck);
		PlayerPrefs.Save();

	}

	public static Dictionary<int, int>[] readDeck(string deckFormattedStr)
	{
		return deckFormattedStr.Split('&').Select(x => x.Split('|').Select(x => x.Split('-')).ToDictionary(x => x[0].Length > 0 ? int.Parse(x[0]) : -1, x => x.Length > 1 && x[1].Length > 0 ? int.Parse(x[1]) : 0)).ToArray();
	}
	public static string writeDeck(Dictionary<int, int> main, Dictionary<int, int> strc)
	{
		if(main.Count() ==0 || strc.Count() == 0)
		{
			return "&";
		}
		string mainDeckStr = string.Join("|", main.Select(x => x.Key.ToString() + "-" + x.Value.ToString()));
		string strcDeckStr = string.Join("|", strc.Select(x => x.Key.ToString() + "-" + x.Value.ToString()));
		return mainDeckStr + "&" + strcDeckStr;
	}
}
