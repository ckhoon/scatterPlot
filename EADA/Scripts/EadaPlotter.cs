using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class EadaPlotter : MonoBehaviour
{
	private const int COL_NUM = 13;
	private const int PERCENT = 100;
	private const float PERCENT_REC = 100 / PERCENT;
	private const string LNG_NAME = "longtitude";
	private const string LAT_NAME = "latitude";
	private const float maxLat = 49.464372f;
	private const float minLat = 24.517714f;
	private const float maxLng = -66.865886f;
	private const float minLng = -124.463635f;
	private const float imageScaling = 593.0f / 948.0f;
	private const float ColColorParti = 0.0167f;
	private const float ColColorStaff = 0.33f;
	private const float ColColorRev = 0.64f;
	private const float HIDDEN_LOCATION = 100;
	private const float FADE = 0.001f;
	private const float OPAQUE = 0.8f;

	public static class TAGS
	{
		public const string
			PARTICIPANT = "participant",
			STAFF = "StaffAndSalaries",
			REV = "RevenuesAndExpenses",
			MEN = "men",
			WOMEN = "women",
			COED = "coed",
			TOTAL = "total";
	}

	public string inputfile;
	public string inputTagFile;
	public GameObject PointPrefab;
	public GameObject PointHolder;

	private List<Dictionary<string, object>> pointList;
	private Dictionary<string, string> tags;
	private Dictionary<string, string> descriptions;
	private List<string> columnList;
	private Dictionary<string, float> minList = new Dictionary<string, float>();
	private Dictionary<string, float> maxList = new Dictionary<string, float>();
	private Dictionary<string, float> ColHeightList = new Dictionary<string, float>();
	private Dictionary<string, float> ColColorH = new Dictionary<string, float>();
	private Dictionary<string, int> ColRotation = new Dictionary<string, int>();

	private int mode = 0;
	private float cubeSize = 0.1f;
	private float cubeSizeMultipler = 3;
	private float plotScale = 10;
	//private const float cubeSize = 0.02f;
	//private const float cubeSizeMultipler = 25;
	private float ColHeightMen;
	private float ColHeightWomen;
	private float ColHeightCoed;
	private float ColHeightTotal;

	// Use this for initialization
	void Start()
	{
		ColHeightMen = cubeSize;
		ColHeightWomen = 3 * cubeSize;
		ColHeightCoed = 5 * cubeSize;
		ColHeightTotal = 7 * cubeSize;
		// Set pointlist to results of function Reader with argument inputfile
		pointList = LoadEadaData.Read(inputfile);
		columnList = new List<string>(pointList[0].Keys);
		tags = LoadEadaTag.Read(inputTagFile, columnList);
		descriptions = LoadEadaDesc.Read(inputTagFile, columnList);
		initColValues();
		Plot();
	}

	public void ChangeMode()
	{
		if ( mode == 0 )
		{
			mode = 1;
			cubeSize = 0.02f;
			cubeSizeMultipler = 40;
			plotScale = 1;
			ColHeightMen = cubeSize;
			ColHeightWomen = 3 * cubeSize;
			ColHeightCoed = 5 * cubeSize;
			ColHeightTotal = 7 * cubeSize;
		}
		else
		{
			mode = 0;
			cubeSize = 0.1f;
			cubeSizeMultipler = 3;
			plotScale = 10;
			ColHeightMen = cubeSize;
			ColHeightWomen = 3 * cubeSize;
			ColHeightCoed = 5 * cubeSize;
			ColHeightTotal = 7 * cubeSize;
		}
		ReInitPlot();
		ClearPlot();
		Plot();
	}

	public void Filter(string strTags)
	{
		for ( int i = 0; i < PointHolder.transform.childCount; i++ )
		{
			if ( !PointHolder.transform.GetChild(i).GetComponent<EadaData>() )
				continue;

			bool filtered = false;
			if ( !strTags.Equals("") )
			{
				foreach ( string strTag in strTags.Trim().Split(' ') )
				{
					if ( PointHolder.transform.GetChild(i).GetComponent<EadaData>().tags.Contains(strTag) )
					{
						//Debug.Log(strTag + " - " + PointHolder.transform.GetChild(i).GetComponent<EadaData>().tags[0]);
						filtered = true;
						break;
					}
				}
			}
			if ( filtered )
			{
				if ( !PointHolder.transform.GetChild(i).GetComponent<EadaData>().filtered )
				{
					PointHolder.transform.GetChild(i).GetComponent<EadaData>().filtered = true;
					PointHolder.transform.GetChild(i).gameObject.SetActive(false);
					//PointHolder.transform.GetChild(i).Translate(0, 0, HIDDEN_LOCATION);
				}
			}
			else
			{
				if ( PointHolder.transform.GetChild(i).GetComponent<EadaData>().filtered )
				{
					PointHolder.transform.GetChild(i).GetComponent<EadaData>().filtered = false;
					PointHolder.transform.GetChild(i).gameObject.SetActive(true);
					//PointHolder.transform.GetChild(i).Translate(0, 0, -HIDDEN_LOCATION);
				}
			}
		}
	}

	public void Filter(string colName, object value, bool greater = true)
	{
		for ( int i = 0; i < PointHolder.transform.childCount; i++ )
		{
			if ( !PointHolder.transform.GetChild(i).GetComponent<EadaData>() )
				continue;

			if ( !PointHolder.transform.GetChild(i).GetComponent<EadaData>().colName.Equals(colName) )
				continue;

			if ( Convert.ToSingle(PointHolder.transform.GetChild(i).GetComponent<EadaData>().value) > Convert.ToSingle(value) )
			{
				if ( greater )
				{
					PointHolder.transform.GetChild(i).GetComponent<EadaData>().filtered = true;
					PointHolder.transform.GetChild(i).gameObject.SetActive(false);
				}
			}
			else
			{
				if ( !greater )
				{
					PointHolder.transform.GetChild(i).GetComponent<EadaData>().filtered = true;
					PointHolder.transform.GetChild(i).gameObject.SetActive(false);
				}
			}
		}
	}

	public void FilterSort(string colName, object value, bool greater = true)
	{
		if ( greater )
			pointList = pointList.Where(o => Convert.ToSingle(o[colName]) >= Convert.ToSingle(value)).ToList();
		else
			pointList = pointList.Where(o => Convert.ToSingle(o[colName]) <= Convert.ToSingle(value)).ToList();
		ClearPlot();
		Plot();
		TurnAlpha(colName);
	}

	public bool SortBy(string colName)
	{
		if ( mode == 0 )
			return false;
		pointList = pointList.OrderBy(o => Convert.ToSingle(o[colName])).ToList();
		ClearPlot();
		Plot();
		return true;
	}

	public void TurnAlpha(string colName)
	{
		for ( int i = 0; i < PointHolder.transform.childCount; i++ )
		{
			if ( !PointHolder.transform.GetChild(i).GetComponent<EadaData>() )
				continue;

			//Color currentColor = PointHolder.transform.GetChild(i).GetComponent<Renderer>().material.color;
			if ( !colName.Equals("") )
			{
				if ( !PointHolder.transform.GetChild(i).GetComponent<EadaData>().colName.Equals(colName) )
				{
					//		currentColor.a = FADE;
					PointHolder.transform.GetChild(i).gameObject.SetActive(false);
				}
				else
				{
					//		currentColor.a = OPAQUE;
					PointHolder.transform.GetChild(i).gameObject.SetActive(true);
				}
			}
			else
			{
				//	currentColor.a = OPAQUE;
				PointHolder.transform.GetChild(i).gameObject.SetActive(true);
			}
			//PointHolder.transform.GetChild(i).GetComponent<Renderer>().material.color = currentColor;
		}
	}

	public void ClearPlot()
	{
		for ( int i = PointHolder.transform.childCount - 1; i >= 0; i-- )
		{
			Destroy(PointHolder.transform.GetChild(i).gameObject);
		}
	}

	public void ReInitPlot()
	{
		pointList = LoadEadaData.Read(inputfile);
	}

	public void Plot()
	{
		if ( mode == 0 )
			plotMap();
		else
			plotParallel();
	}

	public void plotMap()
	{
		//Loop through Pointlist
		for ( var i = 0; i < pointList.Count; i++ )
		{
			float x = getNorm((float)pointList[i][LNG_NAME], maxLng, minLng);
			float z = getNormScale((float)pointList[i][LAT_NAME], maxLat, minLat);

			//Loop through Columns
			for ( int j = COL_NUM - 1; j < columnList.Count / PERCENT_REC; j++ )
			{
				if ( tags[columnList[j]].Equals("") )
					continue;

				float y = Convert.ToSingle(pointList[i][columnList[j]]);
				if ( y == 0 )
					continue;

				y = ( y - minList[columnList[j]] ) / ( maxList[columnList[j]] - minList[columnList[j]] );

				GameObject dataPoint = drawPoint(new Vector3(x, y, z), columnList[j]);

				string strDetails = "Institution: " + (string)pointList[i]["institution_name"]
					+ "\nCity: " + (string)pointList[i]["city_txt"]
					+ "\n" + descriptions[columnList[j]] + ": " + pointList[i][columnList[j]];
				dataPoint.GetComponent<EadaData>().fullDetails = strDetails;
				foreach ( string strTag in tags[columnList[j]].Split(' ') )
				{
					dataPoint.GetComponent<EadaData>().tags.Add(strTag);
				}
				dataPoint.GetComponent<EadaData>().filtered = false;
				dataPoint.GetComponent<EadaData>().colName = columnList[j];
				dataPoint.GetComponent<EadaData>().value = pointList[i][columnList[j]];
				//Debug.Log("total tag = " + dataPoint.GetComponent<EadaData>().tags.Count + " - " + dataPoint.GetComponent<EadaData>().tags[0]);
			}
		}
	}

	public void plotParallel()
	{
		//Loop through Pointlist
		float offsetX = pointList.Count * cubeSize / 2;

		for ( var i = 0; i < pointList.Count; i++ )
		{
			float x = ( i + 1 ) * cubeSize - offsetX;

			//Loop through Columns
			for ( int j = COL_NUM - 1; j < columnList.Count / PERCENT_REC; j++ )
			{
				if ( tags[columnList[j]].Equals("") )
					continue;

				float z = ( j - COL_NUM + 1 ) * cubeSize;
				float y = Convert.ToSingle(pointList[i][columnList[j]]);
				if ( y == 0 )
					continue;

				y = ( y - minList[columnList[j]] ) / ( maxList[columnList[j]] - minList[columnList[j]] );

				GameObject dataPoint = drawPoint(new Vector3(x, y, z), columnList[j]);

				string strDetails = "Institution: " + (string)pointList[i]["institution_name"]
					+ "\nCity: " + (string)pointList[i]["city_txt"]
					+ "\n" + descriptions[columnList[j]] + ": " + pointList[i][columnList[j]];
				dataPoint.GetComponent<EadaData>().fullDetails = strDetails;
				foreach ( string strTag in tags[columnList[j]].Split(' ') )
				{
					dataPoint.GetComponent<EadaData>().tags.Add(strTag);
				}
				dataPoint.GetComponent<EadaData>().filtered = false;
				dataPoint.GetComponent<EadaData>().colName = columnList[j];
				dataPoint.GetComponent<EadaData>().value = pointList[i][columnList[j]];
				//Debug.Log("total tag = " + dataPoint.GetComponent<EadaData>().tags.Count + " - " + dataPoint.GetComponent<EadaData>().tags[0]);
			}
		}
	}

	private void initColValues()
	{
		int iWomen = 0, iMen = 0, iCoed = 0, iTotal = 0;
		for ( int i = COL_NUM - 1; i < columnList.Count; i++ )
		{
			if ( !tags[columnList[i]].Equals("") )
			{
				maxList.Add(columnList[i], FindMaxValue(columnList[i]));
				minList.Add(columnList[i], FindMinValue(columnList[i]));

				string tag = (string)tags[columnList[i]];
				
				if ( Regex.IsMatch(tag, @"\b" + TAGS.WOMEN + @"\b") ) // women must come before men because men is contains in women
				{
					ColHeightList.Add(columnList[i], ColHeightWomen);
					ColRotation.Add(columnList[i], iWomen += ( 360 / 40 ));
				}
				else if ( Regex.IsMatch(tag, @"\b" + TAGS.MEN + @"\b") )
				{
					ColHeightList.Add(columnList[i], ColHeightMen);
					ColRotation.Add(columnList[i], iMen += ( 360 / 40 ));
				}
				else if ( Regex.IsMatch(tag, @"\b" + TAGS.COED + @"\b") )
				{
					ColHeightList.Add(columnList[i], ColHeightCoed);
					ColRotation.Add(columnList[i], iCoed += ( 360 / 45 ));
				}
				else if ( Regex.IsMatch(tag, @"\b" + TAGS.TOTAL + @"\b") )
				{
					ColHeightList.Add(columnList[i], ColHeightTotal);
					ColRotation.Add(columnList[i], iTotal += ( 360 / 25 ));
				}
				else
				{
					Debug.Log("tag is " + tags[columnList[i]] + " name is " + columnList[i] + " IS nothing");
				}

				if ( tag.Contains(TAGS.PARTICIPANT) )
					ColColorH.Add(columnList[i], ColColorParti);
				else if ( tag.Contains(TAGS.STAFF) )
					ColColorH.Add(columnList[i], ColColorStaff);
				else if ( tag.Contains(TAGS.REV) )
					ColColorH.Add(columnList[i], ColColorRev);
				else
				{
					Debug.Log("tag is " + tags[columnList[i]] + " name is " + columnList[i] + " has no color");
					ColColorH.Add(columnList[i], 0.0f);
				}
			}
		}
	}

	private GameObject drawPoint(Vector3 pos, string colName)
	{
		if ( mode == 0 )
			return drawPointMap(pos, colName);
		else
			return drawPointParallel(pos, colName);
	}

	private GameObject drawPointMap(Vector3 pos, string colName)
	{
		GameObject dataPoint = Instantiate(
			PointPrefab,
			new Vector3(pos.x * plotScale, ColHeightList[colName], pos.z * plotScale),
			Quaternion.identity);
		float size = pos.y * cubeSize * cubeSizeMultipler;
		dataPoint.transform.localScale = new Vector3(cubeSize / 3.0f, cubeSize, size);

		Vector3 centerBeforeTranslate = dataPoint.transform.position;
		dataPoint.transform.Translate(0, 0, size / 2);
		dataPoint.transform.RotateAround(centerBeforeTranslate, new Vector3(0, 1, 0), ColRotation[colName]);

		Color pointColor = Color.HSVToRGB(ColColorH[colName], 0.5f, pos.y);
		pointColor.a = OPAQUE;
		dataPoint.GetComponent<Renderer>().material.color = pointColor;

		dataPoint.transform.parent = PointHolder.transform;

		return dataPoint;
	}

	private GameObject drawPointParallel(Vector3 pos, string colName)
	{
		GameObject dataPoint = Instantiate(
			PointPrefab,
			new Vector3(pos.x * plotScale, 0.1f, pos.z * plotScale),
			Quaternion.identity);
		float size = pos.y * cubeSize * cubeSizeMultipler;
		dataPoint.transform.localScale = new Vector3(cubeSize, size, cubeSize);

		Color pointColor = Color.HSVToRGB(ColColorH[colName], 0.5f, pos.y);
		pointColor.a = 0.8f;
		dataPoint.GetComponent<Renderer>().material.color = pointColor;

		dataPoint.transform.parent = PointHolder.transform;

		return dataPoint;
	}

	private float getNorm(float value, float max, float min)
	{
		float x = ( value - min )
					/ ( max - min );
		x -= 0.5f;
		return x;
	}

	private float getNormScale(float value, float max, float min)
	{
		float x = ( value - min )
					/ ( max - min );
		x -= 0.5f;
		x *= imageScaling;
		return x;
	}

	private float FindMaxValue(string columnName)
	{
		float maxValue = Convert.ToSingle(pointList[0][columnName]);
		for ( var i = 0; i < pointList.Count; i++ )
		{
			try
			{
				if ( maxValue < Convert.ToSingle(pointList[i][columnName]) )
					maxValue = Convert.ToSingle(pointList[i][columnName]);
			}
			catch ( FormatException e )
			{
				Debug.Log("Error at count " + i + " from value" + pointList[i][columnName]);
				Debug.LogError(e);
			}
		}
		return maxValue;
	}

	private float FindMinValue(string columnName)
	{
		float minValue = Convert.ToSingle(pointList[0][columnName]);
		for ( var i = 0; i < pointList.Count; i++ )
		{
			try
			{
				if ( Convert.ToSingle(pointList[i][columnName]) < minValue )
					minValue = Convert.ToSingle(pointList[i][columnName]);
			}
			catch ( FormatException e )
			{
				Debug.Log("Error at count " + i + " from value" + pointList[i][columnName]);
				Debug.LogError(e);
			}
		}
		return minValue;
	}
}