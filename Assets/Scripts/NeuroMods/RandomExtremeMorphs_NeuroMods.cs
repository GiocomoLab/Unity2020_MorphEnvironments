using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RandomExtremeMorphs_NeuroMods : MonoBehaviour
{

    private GameObject reward0;
    private GameObject reward1;
    private GameObject player;
    private SP_NeuroMods sp;
    private PC_NeuroMods pc;
    private Vector3 initialPosition;



    private int numTraversalsLocal = -1;

    public float[] trialOrder;


    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_NeuroMods>();
        pc = player.GetComponent<PC_NeuroMods>();


        reward0 = GameObject.Find("Reward_A");
        reward1 = GameObject.Find("Reward_B");

        trialOrder = new float[sp.numTrialsTotal];
        for (int i = 0; i < sp.numTrialsTotal / 2; i++)
        {
            trialOrder[2 * i] = 0f;
            trialOrder[2 * i + 1] = 1f;
        }
        trialOrder = FisherYates(trialOrder);
    }

    // Update is called once per frame
    void Update()
    {
        if ((numTraversalsLocal != sp.numTraversals) & (player.transform.position.z<0))
        {
            numTraversalsLocal = sp.numTraversals;
            sp.morph = trialOrder[numTraversalsLocal];

          

            if (sp.morph == 0f)
            {
                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward0.SetActive(false);

                }
                else
                {
                    reward0.SetActive(true);
                }

                reward1.SetActive(false);
            }
            else
            {

                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward1.SetActive(false);

                }
                else
                {
                    reward1.SetActive(true);
                }

                reward0.SetActive(false);
            }



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
