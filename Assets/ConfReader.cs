using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConfReader
{
    static readonly string confFile = "config";
    static Dictionary<string, string> confValues = new Dictionary<string, string>();


    static void getValues()
	{
		if (confValues.Count > 0)
		{
			return;
		}
		TextAsset data = Resources.Load(confFile) as TextAsset;
		string[] lines = data.text.Split('\n');
		foreach(string line in lines)
		{
			if (line.Contains("="))
			{
				string[] values = line.Split('=');
				confValues[values[0]] = values[1];
			}
		}
	}

	public static string Value(string name)
	{
		getValues();
		if (confValues.ContainsKey(name))
		{
			return confValues[name];
		}
		return "";
	}
}
