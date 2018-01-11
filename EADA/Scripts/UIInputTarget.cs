using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIInputTarget : MonoBehaviour
{
	private Graphic _grfc;
	public Graphic Graphic
	{
		get
		{
			if ( _grfc == null )
				_grfc = GetComponent<Graphic>();
			return _grfc;
		}
	}

	private RectTransform _rect;
	public RectTransform RectTransform
	{
		get
		{
			if ( _rect == null )
				_rect = (RectTransform)transform;
			return _rect;
		}
	}

	public string colName
	{
		get; set;
	}

	public object value
	{
		get; set;
	}

	public bool greater;

	
	public void OnClick()
	{
		Debug.Log("You clicked UIInputTarget: " + name);
	}
}