using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPolling : MonoBehaviour {

	public Avpl.InputKey key;
	GameObject hitObject;

	private int numHit = 0;
	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update() {

		if ( key.IsToggled() )
			return;
		RaycastHit hit;
		if ( Physics.Raycast(Avpl.AvplStatic.GetRay(), out hit))
		{
			hitObject = hit.collider.gameObject;
			Debug.Log("i am hit - " + numHit++ + hitObject);
		}
	}
}
