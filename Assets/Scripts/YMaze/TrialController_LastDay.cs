using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class TrialController_LastDay: MonoBehaviour
{

    private SP_YMazeTrain sp;
    private RR_YMazeTrain rr;
    private GameObject player;



    private GameObject LCurtain;
    private GameObject RCurtain;
    private GameObject LReward;
    private GameObject RReward;

    public int blockNumber = 0;
    private int blockTrial = -1;
    private float blockStartTime = -1;
    private float maxBlockTime = 60f * 5f;
    private int maxTrialNum = 20;
    public int numFamBlocks = 5;
    private float blockTimeout = 60f;
    public bool blankLaser = true;
    public bool longLastBlock = true;
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
        rr = player.GetComponent<RR_YMazeTrain>();

        
        LCurtain = GameObject.Find("Walls/LeftArm/Curtain");
        LReward = GameObject.Find("RewardL");
        RCurtain = GameObject.Find("Walls/RightArm/Curtain");
        RReward = GameObject.Find("RewardR");
        sp.LR = -1;

        
        trialOrder = new float[maxTrialNum * 2 + 1];
        for (int i = 0; i < maxTrialNum * 2 + 1; i++)
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

        LCurtain.SetActive(false);
        RCurtain.SetActive(false);

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
            sp.LR = trialOrder[blockTrial];



            if (blockNumber < numFamBlocks)
            {
                if ((blockTrial >= maxTrialNum) | ((Time.time - blockStartTime) > maxBlockTime))
                {
                    blockNumber++;
                    trialOrder = FisherYates(trialOrder);
                    StartCoroutine(InterBlockInterval());

                }
            }
            else
            {
                if (longLastBlock)
                {
                    if ((blockTrial >= (2 * maxTrialNum)) | ((Time.time - blockStartTime) > 2 * maxBlockTime))
                    {
                        UnityEditor.EditorApplication.isPlaying = false;
                    }
                } else
                {
                    if ((blockTrial >= (maxTrialNum)) | ((Time.time - blockStartTime) > maxBlockTime))
                    {
                        UnityEditor.EditorApplication.isPlaying = false;
                    }
                }

                

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
        if (blankLaser) {
            sendString("L0");
        }
        
        yield return new WaitForSeconds(blockTimeout - 5f);
        if (blankLaser)
        {
            sendString("L1");
        }
        
        yield return new WaitForSeconds(5f);

        //if (blockNumber == numFamBlocks)
        //{
        //    NovCurtain.SetActive(false);
        //}

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
            LReward.SetActive(true);
            RReward.SetActive(true);

        }
        else
        {
            LReward.SetActive(false);
            RReward.SetActive(false);
        }

        rr.toutBool = 1f;
        yield return null;

    }
}
