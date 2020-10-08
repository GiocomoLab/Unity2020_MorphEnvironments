using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TurnController_random : MonoBehaviour
{

    private SP_YMazeTrain sp;
    private PC_YMazeTrain pc;
    private GameObject player;
    private int switchFlag = 0;
    private float rand_val;
    private int numTraversalsLocal = -1;
    

    

    public float[] trialOrder;

    private void Awake()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_YMazeTrain>();
        pc = player.GetComponent<PC_YMazeTrain>();

        

    }

    void Start()
    {
        trialOrder = new float[sp.numTrialsTotal+1];
        for (int i=0; i<sp.numTrialsTotal+1; i++)
        {
            if ((i % 2) == 0)
            {
                trialOrder[i] = -1f;
            }
            else
            {
                trialOrder[i] = 1f;
            }
            
            
        }
        trialOrder = FisherYates(trialOrder);
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.position.z < 0 & numTraversalsLocal != sp.numTraversals)
        {
            numTraversalsLocal++;
            sp.LR = trialOrder[numTraversalsLocal];
            Debug.Log(sp.LR);
        }
    }


    float[] FisherYates(float[] origArray)
    {
        // then shuffle values (Fisher-Yates shuffle)
        int[] order = new int[origArray.Length];

        for (int i = 0; i < origArray.Length; i++)
        {
            order[i] = i;
        }


        for (int i = order.Length - 1; i >= 0; i--)
        {
            int r = (int)UnityEngine.Mathf.Round(UnityEngine.Random.Range(0, i));
            int tmp = order[i];
            order[i] = order[r];
            order[r] = tmp;
        }

        float[] permArray = new float[origArray.Length];
        for (int i = 0; i < origArray.Length; i++)
        {
            permArray[i] = origArray[order[i]];
        }

        return permArray;
    }


}
