using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class createPanoCam_novelobject: MonoBehaviour {
	
	void Update () {
		panoCamScript scr = transform.GetComponent<panoCamScript> ();
		scr.Init();
		
	}
}