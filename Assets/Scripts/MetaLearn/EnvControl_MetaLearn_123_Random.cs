using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvControl_MetaLearn_123_Random : MonoBehaviour
{

    private GameObject reward1;
    private GameObject reward2;
    private GameObject reward3;
    private GameObject player;
    private GameObject morphmaze;
    private GameObject xmaze;
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

        reward3 = GameObject.Find("Reward_A");
        reward1 = GameObject.Find("Reward_B");
        reward2 = GameObject.Find("Reward_C");

        morphmaze = GameObject.Find("MorphMaze");
        xmaze = GameObject.Find("XMaze");

        trialOrder = new float[sp.numTrialsTotal];
        for (int i = 0; i < sp.numTrialsTotal / 3; i++)
        {
            trialOrder[3 * i] = 0f;
            trialOrder[3 * i + 1] = 1f;
            trialOrder[3 * i + 2] = 0.5f;
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

            if (sp.morph==0f)
            {
                morphmaze.SetActive(true);
                xmaze.SetActive(false);

                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward1.SetActive(false);

                }
                else
                {
                    reward1.SetActive(true);
                }
                
                reward2.SetActive(false);
                reward3.SetActive(false);
            }

            else if (sp.morph==1.0f)
            {
                morphmaze.SetActive(true);
                xmaze.SetActive(false);

                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward2.SetActive(false);

                }
                else
                {
                    reward2.SetActive(true);
                }
                
                reward1.SetActive(false);
                reward3.SetActive(false);

            }
            else
            {
                morphmaze.SetActive(false);
                xmaze.SetActive(true);

                if (UnityEngine.Random.value < sp.SkipTrialPcnt)
                {
                    reward3.SetActive(false);

                }
                else
                {
                    reward3.SetActive(true);
                }
                
                reward1.SetActive(false);
                reward2.SetActive(false);
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
