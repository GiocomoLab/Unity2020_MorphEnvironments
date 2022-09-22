using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowRewardTowerCue : MonoBehaviour
{
	private SP_TwoReward sp;
	private PC_TwoReward pc;
	private GameObject player;
	//private SavePositionData saveScript;
	// private PlayerController playerScript;
	// private SessionParams paramsScript;
	private Renderer rend;
	private bool lowmorph = false;
	//private bool unityEnded = false;

	// Use this for initialization
	void Start ()
	{
		GameObject player = GameObject.Find ("Player");
		sp = player.GetComponent<SP_TwoReward>();
		pc = player.GetComponent<PC_TwoReward>();
		// playerScript = player.GetComponent<PlayerController> ();
		// paramsScript = player.GetComponent<SessionParams> ();
		// saveScript = player.GetComponent<SavePositionData> ();

		rend = GetComponent<Renderer>();
		rend.enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
	{

		if (sp.morph > 0)
        {
			lowmorph = false;
        } else
        {
		    lowmorph = true;
        }
		//unityEnded = saveScript.unityEnded;
		// keep everything off during dark, and on during VR
		rend.enabled = lowmorph;

	}
}

