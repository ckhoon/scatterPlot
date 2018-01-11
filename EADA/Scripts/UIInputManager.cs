using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class UIInputManager : MonoBehaviour
{
	private UIInputTarget[] allTargets;

	private void Awake()
	{
		allTargets = GetComponentsInChildren<UIInputTarget>();
	}

	private void Update()
	{
		CheckForClickEvent();
	}

	private void CheckForClickEvent()
	{
		bool buttonPress = GetClickButtonDown();
		if ( !buttonPress )
			return;

		Ray ray = GetSelectionRay();

		List<UIInputTarget> RaycastHits = new List<UIInputTarget>();
		//This loop performs the UI "raycast" using the ray given from GetSelectionRay();
		foreach ( UIInputTarget target in allTargets )
		{
			//Skip objects that are inactive
			if ( !target.gameObject.activeInHierarchy )
				continue;

			Vector3 hitPos;
			if ( RayIntersectsRectTransform(target.RectTransform, ray, out hitPos) )
			{
				RaycastHits.Add(target);
			}
		}

		if ( RaycastHits.Count == 0 )
		{
			return;
		}

		//Finally, sort the list of raycast hits by Graphic.depth and send the event to the first one
		if ( RaycastHits.Count > 1 )
		{
			RaycastHits = RaycastHits.OrderByDescending(x => x.Graphic.depth).ToList();
		}
		RaycastHits[0].OnClick();
	}

	private Ray GetSelectionRay()
	{
		//Set this to the transform of your vive controller or something childed to it
		//Make sure this is rotated properly so "forward"
		//comes out of the controller in the direction you want the user to point at the UI
		Transform t = null;

		return new Ray(t.position, t.forward);
	}

	private bool GetClickButtonDown()
	{
		//Fill this in to return true if the button you want a user to press to "click" the UI
		//ex. return Input.GetKeyDown(KeyCode.F);
		//Make sure to use GetPressDown since we don't want to click every frame the button is held, just the one it's pressed

		throw new System.NotImplementedException();
	}

	/// <summary>
	/// Detects whether a ray intersects a RectTransform and if it does also
	/// returns the world position of the intersection.
	/// </summary>
	/// <param name="rectTransform"></param>
	/// <param name="ray"></param>
	/// <param name="worldPos"></param>
	/// <returns></returns>
	public static bool RayIntersectsRectTransform(RectTransform rectTransform, Ray ray, out Vector3 worldPos)
	{
		Vector3[] corners = new Vector3[4];
		rectTransform.GetWorldCorners(corners);
		Plane plane = new Plane(corners[0], corners[1], corners[2]);

		float enter;
		if ( !plane.Raycast(ray, out enter) )
		{
			worldPos = Vector3.zero;
			return false;
		}

		Vector3 intersection = ray.GetPoint(enter);

		Vector3 BottomEdge = corners[3] - corners[0];
		Vector3 LeftEdge = corners[1] - corners[0];
		float BottomDot = Vector3.Dot(intersection - corners[0], BottomEdge);
		float LeftDot = Vector3.Dot(intersection - corners[0], LeftEdge);
		if ( BottomDot < BottomEdge.sqrMagnitude && // Can use sqrMag because BottomEdge is not normalized
			LeftDot < LeftEdge.sqrMagnitude &&
				BottomDot >= 0 &&
				LeftDot >= 0 )
		{
			worldPos = corners[0] + LeftDot * LeftEdge / LeftEdge.sqrMagnitude + BottomDot * BottomEdge / BottomEdge.sqrMagnitude;
			return true;
		}
		else
		{
			worldPos = Vector3.zero;
			return false;
		}
	}
}