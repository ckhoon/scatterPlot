using UnityEngine;

interface PlotterInterface
{
	bool TurnAlpha(GameObject gb=null);
	bool TurnAlpha(float y);
	string GetDimension();
}