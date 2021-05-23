using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DeckRW 
{
	static readonly string deckKey = "LocalDecks";
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
	public static void saveDeck(string name, Dictionary<int, int> countsMain)
	{
		string tempName = name.Replace(',', 'x');
		string[] decks = getDecks();
		if (!decks.Contains(tempName))
		{
			PlayerPrefs.SetString(deckKey, PlayerPrefs.GetString(deckKey) + "," + tempName);
		}
		string deckStr = string.Join("|", countsMain.Select(x => x.Key.ToString() + "-" + x.Value.ToString()));
		PlayerPrefs.SetString(tempName, deckStr);
		PlayerPrefs.Save();
	}

	public static Dictionary<int, int> loadDeck(string name)
	{
		//string tempName = name.Replace(',', 'x');
		if (PlayerPrefs.HasKey(name))
		{
			string deck = PlayerPrefs.GetString(name);
			return deck.Split('|').Select(x => x.Split('-')).ToDictionary(x => int.Parse(x[0]), x => int.Parse(x[1]));
		}

		return new Dictionary<int, int>();
	}
}
