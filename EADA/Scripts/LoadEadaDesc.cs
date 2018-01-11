using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LoadEadaDesc : MonoBehaviour 
{
	static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))"; // Define delimiters, regular expression craziness
	static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r"; // Define line delimiters, regular experession craziness
	static int COL_NUM = 7;

	public static Dictionary<string, string> Read(string file, List<string> colList) //Declare method
	{
		var descriptions = new Dictionary<string, string>(); //declare dictionary list

		TextAsset data = Resources.Load(file) as TextAsset; //Loads the TextAsset named in the file argument of the function
		var lines = Regex.Split(data.text, LINE_SPLIT_RE); // Split data.text into lines using LINE_SPLIT_RE characters

		if ( lines.Length != colList.Count )
		{
			Debug.LogError("column count in tag file is wrong " + lines.Length + " - " + colList.Count);
			return descriptions;
		}

		// Loops through lines
		for ( var i = 0; i < lines.Length; i++ )
		{
			var values = Regex.Split(lines[i], SPLIT_RE); //Split lines according to SPLIT_RE, store in var (usually string array)
			if ( !values[1].Equals(colList[i]) )
			{
				Debug.LogError("col name does not match tag col name " + values[1] + " - " + colList[i]);
				return null;
			}

			if ( values.Length < COL_NUM )
				continue; // Skip to end of loop (continue) if value < 8 length means no descriptions.

			var entry = new Dictionary<string, string>(); // Creates dictionary object

			string value = "";
			if ( !values[COL_NUM-1].Equals("") )
				value += (string)values[COL_NUM-1];
			descriptions[(string)values[1]] = (string)value;
			//Debug.Log(value);
		}
		return descriptions; //Return list
	}
}