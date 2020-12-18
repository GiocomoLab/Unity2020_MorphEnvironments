using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MorphBlocks_NeuroMods: MonoBehaviour
{

    private GameObject reward;
    private GameObject player;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
    private Vector3 initialPosition;



    private int numTraversalsLocal = -1;
    


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();


        reward = GameObject.Find("Reward");
        sp.morph=0;
    }

    // Update is called once per frame
    void Update()
    {
        if (numTraversalsLocal != sp.numTraversals)
        {
            numTraversalsLocal = sp.numTraversals;
            if ((numTraversalsLocal%10==0) & (sp.numTraversals > 5))
            {
                Debug.Log("Switch");
                sp.morph = Mathf.Abs(sp.morph - 1.0f);
                //reward.transform.position = new Vector3(0f, 0f, 250.0f + 150.0f * UnityEngine.Random.value); ;
            }
            


        }
    }
}
