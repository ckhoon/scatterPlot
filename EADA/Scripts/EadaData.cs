using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EadaData : MonoBehaviour {

	public string fullDetails
	{
		get; set;
	}

	public string colName
	{
		get; set;
	}

	public object value
	{
		get; set;
	}

	public bool filtered
	{
		get; set;
	}

	public List<string> tags = new List<string>();
}