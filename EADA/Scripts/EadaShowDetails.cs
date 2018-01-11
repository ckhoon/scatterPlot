using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class EadaShowDetails : MonoBehaviour
{
	public GameObject canvasMenuHolder;
	public GameObject canvasMenuHighlight;
	public GameObject txtDetailCanvas;
	public GameObject plotter;
//	public GameObject plotterParallel;
	public GameObject btnSmaller;
	public GameObject btnGreater;
	public Text txtDetails;
	public GameObject scaler;
	public Avpl.InputKey key_plus;
	public Avpl.InputKey key_minus;
	public Avpl.InputKey button_select;
	public Avpl.InputKey button_menuCycle;
	public Avpl.InputKey button_menuCycleLeft;
	public Avpl.InputKey button_menuSelect;
	public Avpl.InputKey button_menuSort;
	public Avpl.InputKey button_menuReset;
	public Avpl.InputKey button_menuMode;
	private const float MENU_SELECT_R = 0.0f;
	private const float MENU_UNSELECT_R = 1.0f;

	private int menuSelected = 1;
	private int scaleFactor = 1;
	private UIInputTarget[] allTargets;

	// Update is called once per frame
	void Update()
	{
		if ( button_select.IsToggled() )
		{
			allTargets = GetComponentsInChildren<UIInputTarget>();
			List<UIInputTarget> RaycastHits = new List<UIInputTarget>();
			//This loop performs the UI "raycast" using the ray given from GetSelectionRay();
			foreach ( UIInputTarget target in allTargets )
			{
				//Skip objects that are inactive
				if ( !target.gameObject.activeInHierarchy )
					continue;

				Vector3 hitPos;
				if ( RayIntersectsRectTransform(target.RectTransform, Avpl.AvplStatic.GetRay(), out hitPos) )
				{
					RaycastHits.Add(target);
				}
			}

			RaycastHit hit;
			if ( Physics.Raycast(Avpl.AvplStatic.GetRay(), out hit) )
			{
				GameObject hitObject = hit.collider.gameObject;

				if ( RaycastHits.Count != 0 )
				{
					RaycastHits = RaycastHits.OrderByDescending(x => x.Graphic.depth).ToList();
					if (
						( Avpl.AvplStatic.wandRay.transform.position.z - RaycastHits[0].transform.position.z ) >
						 ( Avpl.AvplStatic.wandRay.transform.position.z - hitObject.transform.position.z )
					)
					{
						plotter.GetComponent<EadaPlotter>().FilterSort(RaycastHits[0].colName, RaycastHits[0].value, RaycastHits[0].greater);
						RaycastHits[0].OnClick();
						return;
					}
				}

				if ( hitObject.GetComponent<EadaData>()
					&& hitObject.GetComponent<Renderer>().enabled )
				{
					txtDetails.text = (string)hitObject.GetComponent<EadaData>().fullDetails;
					txtDetailCanvas.transform.rotation = Quaternion.identity;       //might not need this.. added in just in case.
					txtDetailCanvas.transform.position = Avpl.AvplStatic.wandRay.transform.position;
					Vector3 vDir = Avpl.AvplStatic.wandRay.transform.position - hitObject.transform.position;
					vDir.Normalize();
					vDir *= -0.5f;
					txtDetailCanvas.transform.Translate(vDir);
					txtDetailCanvas.transform.rotation = Avpl.AvplStatic.wandRay.transform.rotation;
					txtDetailCanvas.transform.Translate(Avpl.AvplStatic.wandRay.transform.up * 0.3f);

//					if ( plotter.activeSelf )
						plotter.GetComponent<EadaPlotter>().TurnAlpha((string)hitObject.GetComponent<EadaData>().colName);

					UIInputTarget[] btns = txtDetailCanvas.GetComponentsInChildren<UIInputTarget>();
					foreach ( UIInputTarget btn in btns )
					{
						btn.colName = (string)hitObject.GetComponent<EadaData>().colName;
						btn.value = hitObject.GetComponent<EadaData>().value;
					}
				}
				else
				{
//					if ( plotter.activeSelf )
						plotter.GetComponent<EadaPlotter>().TurnAlpha("");
					Debug.Log("hit - " + hit.collider.bounds);
					Debug.Log("i am hit instead - " + hitObject);
				}
			}
			else
			{
				if ( RaycastHits.Count != 0 )
				{
					RaycastHits = RaycastHits.OrderByDescending(x => x.Graphic.depth).ToList();
					plotter.GetComponent<EadaPlotter>().FilterSort(RaycastHits[0].colName, RaycastHits[0].value, RaycastHits[0].greater);
					RaycastHits[0].OnClick();
				}
				else
				{
//					if ( plotter.activeSelf )
						plotter.GetComponent<EadaPlotter>().TurnAlpha("");
				}
			}
		}
		else if ( button_menuCycle.IsToggled() )
		{
			menuSelected++;
			if ( menuSelected > canvasMenuHolder.transform.childCount )
			{
				menuSelected = 1;
			}
			Vector3 pos = canvasMenuHolder.transform.GetChild(menuSelected - 1).position;
			pos.z = canvasMenuHighlight.transform.position.z;
			canvasMenuHighlight.transform.position = pos;
		}
		else if ( button_menuCycleLeft.IsToggled() )
		{
			menuSelected--;
			if ( menuSelected < 1 )
			{
				menuSelected = canvasMenuHolder.transform.childCount;
			}
			Vector3 pos = canvasMenuHolder.transform.GetChild(menuSelected - 1).position;
			pos.z = canvasMenuHighlight.transform.position.z;
			canvasMenuHighlight.transform.position = pos;
		}
		else if ( button_menuSelect.IsToggled() )
		{
			Color color = canvasMenuHolder.transform.GetChild(menuSelected - 1).GetComponentInChildren<Image>().color;
			if ( color.r == MENU_SELECT_R )
				color.r = MENU_UNSELECT_R;
			else
				color.r = MENU_SELECT_R;
			canvasMenuHolder.transform.GetChild(menuSelected - 1).GetComponentInChildren<Image>().color = color;
			
			//if( plotter.activeSelf )
				plotter.GetComponent<EadaPlotter>().Filter(getTagString());
			//if( plotterParallel.activeSelf )
			//	plotterParallel.GetComponent<EadaParallel>().Filter(getTagString());
		}
		else if ( button_menuSort.IsToggled() )
		{
			RaycastHit hit;
			if ( Physics.Raycast(Avpl.AvplStatic.GetRay(), out hit) )
			{
				GameObject hitObject = hit.collider.gameObject;
				if ( hitObject.GetComponent<EadaData>()
					&& hitObject.GetComponent<Renderer>().enabled )
				{
					if(plotter.GetComponent<EadaPlotter>().SortBy((string)hitObject.GetComponent<EadaData>().colName))
						plotter.GetComponent<EadaPlotter>().Filter(getTagString());
					//plotterParallel.GetComponent<EadaParallel>().Filter(getTagString());
				}
			}
		}
		else if ( button_menuReset.IsToggled() )
		{
//			if ( plotter.activeSelf )
//			{
				plotter.GetComponent<EadaPlotter>().ReInitPlot();
				plotter.GetComponent<EadaPlotter>().ClearPlot();
				plotter.GetComponent<EadaPlotter>().Plot();
				plotter.GetComponent<EadaPlotter>().Filter(getTagString());
//			}
		}
		else if ( button_menuMode.IsToggled() )
		{
			plotter.GetComponent<EadaPlotter>().ChangeMode();
			//plotter.GetComponent<EadaPlotter>().ClearPlot();
			//plotter.GetComponent<EadaPlotter>().Plot();
			//plotter.GetComponent<EadaPlotter>().Filter(getTagString());
		}
		else if ( key_plus.IsToggled() )
		{
			scaleFactor++;
			scaler.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
		}
		else if ( key_minus.IsToggled() )
		{
			if ( scaleFactor > 1 )
				scaleFactor--;
			scaler.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
		}
	}

	private string getTagString()
	{
		string tag = "";

		for ( int i = 0; i < canvasMenuHolder.transform.childCount; i++ )
		{
			if ( canvasMenuHolder.transform.GetChild(i).GetComponentInChildren<Image>().color.r == MENU_SELECT_R )
			{
				tag += canvasMenuHolder.transform.GetChild(i).GetComponent<EadaMenuData>().tag + " ";
			}
		}
		//Debug.Log(tag);
		return tag;
	}

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
