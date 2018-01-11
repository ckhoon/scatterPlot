using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class DataPlotterEADA : MonoBehaviour
{

	// Name of the input file, no extension
	public string inputfile;

	//public Canvas DataCanvas;
	public Text DataDetails;

	// List for holding data from CSV reader
	private List<Dictionary<string, object>> pointList;
	private List<string> columnListWithNum;
	//private List<Dictionary<string, float>> maxList = new List<Dictionary<string, float>>();
	//private List<Dictionary<string, float>> minList = new List<Dictionary<string, float>>();
	private Dictionary<string, float> minList = new Dictionary<string, float>();
	private Dictionary<string, float> maxList = new Dictionary<string, float>();

	private const int FIRST_COL_TO_DISPLAY = 12;
	private const float FADE = 0.01f;
	private const float OPAQUE = 1f;

	private const int PERCENT = 20;
	private const float PERCENT_REC = 100 / PERCENT;

	private const string LAST_KEY = "GRND_TOTAL_EXPENSE";

	// USA lat, lng max and min
	private const float xMax = -65.53f;
	private const float zMax = 50f;
	private const float xMin = -124.76f;
	private const float zMin = 23.8f;

	// Indices for columns to be assigned
	public int colLng = 7;
	public int colLat = 8;

	public string latName;
	public string lngName;

	public float plotScale = 50;
	public float cubeSize = 0.05f;
	public float cubeSizeMultipler = 10;

	// The prefab for the data points that will be instantiated
	public GameObject PointPrefab;

	// Object which will contain instantiated prefabs in hiearchy
	public GameObject PointHolder;
	public GameObject PurposePointHolder;

	public Avpl.InputKey key_select;
	public Avpl.InputKey button_select;
	public Avpl.InputKey key_topView;

	private GameObject hitObject;
	private GameObject topView; 

	// Use this for initialization
	void Start()
	{
		// Set pointlist to results of function Reader with argument inputfile
		pointList = CSVReader.Read(inputfile);

		//Log to console
		//Debug.Log(pointList);

		// Declare list of strings, fill with keys (column names)
		List<string> columnList = new List<string>(pointList[1].Keys);
		columnListWithNum = findListWithNum();

		// Print number of keys (using .count)
		Debug.Log("There are " + columnList.Count + " columns in the CSV");
		Debug.Log("There are " + columnListWithNum.Count + " number columns in the CSV");
		Debug.Log("There are " + pointList.Count + " rows in the CSV");

		latName = columnList[colLat];
		lngName = columnList[colLng];

		foreach ( string key in columnListWithNum )
		{
			maxList.Add(key, FindMaxValue(key));
			minList.Add(key, FindMinValue(key));
		}
		//Debug.Log("There are " + maxList.Count + " max items"  + " - " + minList.Count + " min items");

		//Loop through Columns
		for ( var j = 0; j < columnListWithNum.Count/ PERCENT_REC; j++ )
		{
			//Debug.Log(columnListWithNum[j]);


			//float tone = (float)( j + 1 ) / (float)( columnListWithNum.Count / PERCENT_REC );
			int nextColor = ( j % 3 ) + 1;
			if ( nextColor > 2 ) 
				nextColor = 0;

			//Color pointColor = new Color(1 - tone, 1 - tone, 1 - tone, 1.0f);
			//Color pointColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
			Color pointColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
			pointColor[j%3] = 1.0f;
			//pointColor[nextColor] = 1 - tone;
			//Debug.Log(pointColor);

			//Loop through Pointlist
			for ( var i = 0; i < pointList.Count; i++ )
			{
				float x =
					( System.Convert.ToSingle(pointList[i][lngName]) - xMin )
					/ ( xMax - xMin );

				float z =
					( System.Convert.ToSingle(pointList[i][latName]) - zMin )
					/ ( zMax - zMin );

				GameObject dataPoint = Instantiate(
						PointPrefab,
						new Vector3(x * plotScale, cubeSize * (j+1), z * plotScale),
						Quaternion.identity);

				dataPoint.GetComponent<DataStructure>().ColName = columnListWithNum[j];

				dataPoint.transform.parent = PointHolder.transform;
				float y;

				//Debug.Log(pointList[i][columnListWithNum[j]] + " - " + minList[0][columnListWithNum[j]] + " - " + maxList[0][columnListWithNum[j]] + " - " + columnListWithNum[j]);
				//Debug.Log(pointList[i][columnListWithNum[j]]);

				if ( System.Convert.ToSingle(pointList[i][columnListWithNum[j]]) == 0 )
					y = 0;
				else
					y =
					( System.Convert.ToSingle(pointList[i][columnListWithNum[j]]) - minList[columnListWithNum[j]] )
					/ ( maxList[columnListWithNum[j]] - minList[columnListWithNum[j]] );


				float size = y * cubeSize * cubeSizeMultipler;
				dataPoint.transform.localScale = new Vector3(size, cubeSize, size);


				dataPoint.GetComponent<Renderer>().material.color = pointColor;
				//Debug.Log(minList[columnListWithNum[j]] + " - " + maxList[columnListWithNum[j]] + " - " + columnListWithNum[j]);
			}
		}

		ShowPurpose();
		topView = GameObject.Find("QuadTopView");
	}

	void Update()
	{
		if ( key_select.IsToggled()  || button_select.IsToggled() )
		{
			RaycastHit hit;
			if ( Physics.Raycast(Avpl.AvplStatic.GetRay(), out hit) )
			{
				hitObject = hit.collider.gameObject;
				//Debug.Log(hitObject.transform.parent);
				//Debug.Log(hitObject.transform.parent.GetComponent<DataPlotterEADA>().pointList.Count);
				//Debug.Log(hitObject.GetComponent<DataStructure>().ColName);
				if ( hitObject.GetComponent<DataStructure>() )
				{
					//DataCanvas.transform.rotation = GameObject.Find("CaveCenter").transform.rotation;
					//DataCanvas.transform.position = hitObject.transform.position;
					//Transform detail = DataCanvas.transform.FindChild("DataDetail");
					DataDetails.text = hitObject.GetComponent<DataStructure>().ColName;
					//DataCanvas.enabled = true;
					turnAlpha(hitObject.GetComponent<DataStructure>().ColName);
				}
				else
				{
					//DataCanvas.enabled = false;
					DataDetails.text = "";
					turnAlpha();
				}
			}
			else
			{
				//DataCanvas.enabled = false;
				DataDetails.text = "";
				turnAlpha();
			}
		}
		if ( key_topView.IsToggled() )
		{

			topView.SetActive(!topView.activeSelf);
		}



		//m_Wand = GameObject.Find("VRManager");
		//Debug.Log(m_Wand.GetComponent<VRManagerScript>().ShowWand);
		//m_Wand.GetComponent<VRManagerScript>().ShowWand = true;
	}

	private float FindMaxValue(string columnName)
	{
		//set initial value to first value
		float maxValue = Convert.ToSingle(pointList[0][columnName]);

		//Loop through Dictionary, overwrite existing maxValue if new value is larger
		for ( var i = 0; i < pointList.Count; i++ )
		{
			try{

				if ( maxValue < Convert.ToSingle(pointList[i][columnName]) )
					maxValue = Convert.ToSingle(pointList[i][columnName]);
			}
			catch( FormatException e)
			{
				Debug.Log("Error at count " + i + " from value" + pointList[i][columnName]);
				Debug.LogError(e);
			}
		}

		//Spit out the max value
		//Debug.Log("max at " + maxValue);
		return maxValue;
	}

	private float FindMinValue(string columnName)
	{

		float minValue = Convert.ToSingle(pointList[0][columnName]);

		//Loop through Dictionary, overwrite existing minValue if new value is smaller
		for ( var i = 0; i < pointList.Count; i++ )
		{
			try
			{
				if ( Convert.ToSingle(pointList[i][columnName]) < minValue )
					minValue = Convert.ToSingle(pointList[i][columnName]);
			}
			catch(FormatException e){
				Debug.Log("Error at count " + i + " from value" + pointList[i][columnName]);
				Debug.LogError(e);
			}
		}
		//Debug.Log("min at " + minValue);
		return minValue;
	}

	private void turnAlpha(string colName = "")
	{
		float alpha;

		if ( colName == "" )
			alpha = OPAQUE;
		else
			alpha = FADE;

		//Debug.Log(PointHolder.transform.childCount);
		for ( var i = 0; i < PointHolder.transform.childCount; i++ )
		{
			if ( PointHolder.transform.GetChild(i).GetComponent<DataStructure>().ColName != colName )
			{
				//Debug.Log(PointHolder.transform.GetChild(i).GetComponent<DataStructure>().ColName + " - - " + i);
				Color currentColor = PointHolder.transform.GetChild(i).GetComponent<Renderer>().material.color;
				currentColor.a = alpha;
				PointHolder.transform.GetChild(i).GetComponent<Renderer>().material.color = currentColor;
			}
			else
			{
				Color currentColor = PointHolder.transform.GetChild(i).GetComponent<Renderer>().material.color;
				currentColor.a = OPAQUE;
				PointHolder.transform.GetChild(i).GetComponent<Renderer>().material.color = currentColor;
			}
		}

	}

	private void ShowPurpose()
	{
		for ( var i = 0; i < pointList.Count; i++ )
		{
			float x =
				( System.Convert.ToSingle(pointList[i][lngName]) - xMin )
				/ ( xMax - xMin );

			float z =
				( System.Convert.ToSingle(pointList[i][latName]) - zMin )
				/ ( zMax - zMin );

			float y;

			if ( System.Convert.ToSingle(pointList[i][LAST_KEY]) == 0 )
				y = 0;
			else
				y =
				( System.Convert.ToSingle(pointList[i][LAST_KEY]) - minList[LAST_KEY] )
				/ ( maxList[LAST_KEY] - minList[LAST_KEY] );

			float size = y * cubeSize * ( columnListWithNum.Count / PERCENT_REC );

			GameObject dataPoint = Instantiate(
					PointPrefab,
					new Vector3(x * plotScale, cubeSize * ( columnListWithNum.Count / PERCENT_REC + 1 ) + size/2, z * plotScale),
					Quaternion.identity);

			dataPoint.GetComponent<DataStructure>().ColName = LAST_KEY;

			dataPoint.transform.parent = PurposePointHolder.transform;

			dataPoint.transform.localScale = new Vector3(cubeSize/2, size, cubeSize/2);

			dataPoint.GetComponent<Renderer>().material.color = new Color(0.25f, 0.9f, 0.9f, 1.0f);
		}
	}


	private List<string> findListWithNum()
	{
		List<string> columnWithNum = new List<string>();
		List<string> columnList = new List<string>(pointList[0].Keys);

		for ( var i = FIRST_COL_TO_DISPLAY; i < pointList[0].Keys.Count; i++ )
		{
			if ( IsNumeric(pointList[0][columnList[i]]))
			{
				columnWithNum.Add(columnList[i]);
			}
		}

		return columnWithNum;
	}

	public static bool IsNumeric(object expression)
	{
		if ( expression == null )
			return false;

		float number;
		return float.TryParse(Convert.ToString(expression), out number);
	}

}