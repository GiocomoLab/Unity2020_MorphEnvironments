using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class TrialController_YMaze : MonoBehaviour
{

    private SP_YMazeTrain sp;
    private RR_YMazeTrain rr;
    private GameObject player;
    

    public int NovelArm = 1;
    private GameObject FamCurtain;
    private GameObject NovCurtain;
    private GameObject FamReward;
    private GameObject NovReward;

    public int blockNumber = 0;
    private int blockTrial = -1;
    private float blockStartTime = -1;
    private float maxBlockTime = 60f * 5f;
    private int maxTrialNum = 20;
    private int numFamBlocks = 5;
    private float blockTimeout = 60f;

    private float[] trialOrder;


    private int numTraversalsLocal = -1;

    private static int localPort;
    private static string IP = "171.65.17.36";  // define in init
    private static int port = 7000;  // define in init


    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;
    // sendData
    private void sendString(string message)
    {
        try
        {
            if (message != "")
            {

                // get UTF8 encoding of string
                byte[] data = Encoding.UTF8.GetBytes(message);

                // send message
                client.Send(data, data.Length, remoteEndPoint);
            }
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    public void Awake()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();

    }


    // Start is called before the first frame update
    void Start()
    {
        // for saving data
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_YMazeTrain>();
        //pc = player.GetComponent<PC_YMazeTrain>();
        rr = player.GetComponent<RR_YMazeTrain>();

        if (NovelArm==1)
        {
            FamCurtain = GameObject.Find("Walls/LeftArm/Curtain");
            FamReward = GameObject.Find("RewardL");
            NovCurtain = GameObject.Find("Walls/RightArm/Curtain");
            NovReward = GameObject.Find("RewardR");
            sp.LR = -1;

        } else
        {
            FamCurtain = GameObject.Find("Walls/RightArm/Curtain");
            FamReward = GameObject.Find("RewardR");
            NovCurtain = GameObject.Find("Walls/LeftArm/Curtain");
            NovReward = GameObject.Find("RewardL");
            sp.LR = 1;
        }

        FamCurtain.SetActive(false);

        trialOrder = new float[maxTrialNum*2 + 1];
        for (int i = 0; i < maxTrialNum*2 + 1; i++)
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
        if (blockStartTime < 0f)
        {
            blockStartTime = rr.startTime;
        }

        if ((player.transform.position.z < 0) & (numTraversalsLocal != sp.numTraversals))
        {
            numTraversalsLocal++;
            blockTrial++;
            StartCoroutine(InterTrialTimeout()); // random timeout and turn rewards on/off


            

            if (blockNumber < numFamBlocks)
            {
                if ((blockTrial >=maxTrialNum) | ((Time.time - blockStartTime) > maxBlockTime))
                {
                    blockNumber++;
                    StartCoroutine(InterBlockInterval());

                }
            }
            else
            {

                if ((blockTrial>=(2*maxTrialNum)) | ((Time.time - blockStartTime) > 2 * maxBlockTime))
                {
                    UnityEditor.EditorApplication.isPlaying = false;
                }

               

                StartCoroutine(InterTrialTimeout());
                sp.LR = trialOrder[blockTrial];
                //if (UnityEngine.Random.value <= .5f)
                //{
                //    sp.LR = -1;
                //}  
                //else
                //{
                 //   sp.LR = 1;
                //}
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




    IEnumerator InterBlockInterval()
    {
        rr.blockBool = 0f;
        sendString("L0");
        yield return new WaitForSeconds(blockTimeout - 5f);
        sendString("L1");
        yield return new WaitForSeconds(5f);

        if (blockNumber == numFamBlocks)
        {
            NovCurtain.SetActive(false);
        }

        blockStartTime = Time.time;
        blockTrial = 0;
        rr.blockBool = 1f;
    }

    IEnumerator InterTrialTimeout()
    {

        rr.toutBool = 0f;
        yield return new WaitForSeconds(UnityEngine.Random.value * 5f);
        if (UnityEngine.Random.value > sp.skipTrialPcnt)
        {
            NovReward.SetActive(true);
            FamReward.SetActive(true);

        }
        else
        {
            NovReward.SetActive(false);
            FamReward.SetActive(false);
        }

        rr.toutBool = 1f;
        yield return null;

    }
}
