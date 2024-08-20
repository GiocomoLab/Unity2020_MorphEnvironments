using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;




public class PC_2DTrack : MonoBehaviour
{

    private GameObject player;
    private GameObject reward;
    private GameObject panoCam;
    private GameObject endWall;

    private Rigidbody rb;

    private SP_2DTrack sp;
    private DL_2DTrack dl;
    private RR_2DTrack rotary;
    private SbxTTLs_2DTrack sbxttls;
    private TrialBlocks_2DTrack tb;

    private bool reward_dir;


    private Vector3 initialPosition;
    private Vector3 initialRotation;
    private Vector3 movement;

    private static bool created = false;
    private int r;

    public int cmd = 2;
    private bool flashFlag = false;
    private float LastRewardTime;
    public int prevReward = 0;

    public ArrayList LickHistory;
    public bool bckgndOn = true;



    public int mRewardFlag = 0;
    public int rzoneFlag = 0;
    public int toutzoneFlag = 0;
    public int tendFlag = 0;
    public int tstartFlag = 0;

    private static int localPort;
    private static string IP = "10.124.53.26";  // define in init
    private static int port = 7000;  // define in init

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;

    public float radius = 200;





    public void Start()
    {
        player = GameObject.Find("Player");
        endWall = GameObject.Find("End Wall");
        
        sp = player.GetComponent<SP_2DTrack>();
        dl = player.GetComponent<DL_2DTrack>();
        rotary = player.GetComponent<RR_2DTrack>();
        sbxttls = player.GetComponent<SbxTTLs_2DTrack>();
        tb = player.GetComponent<TrialBlocks_2DTrack>();

        Debug.Log(sp.sceneName);

        panoCam = GameObject.Find("panoCamera");
        //panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);        // Needed?
        reward = GameObject.Find("Reward");

        PositionObjects(tb.trialAnglesList[sp.numTraversals], radius, reward.transform.position);

        LickHistory = new ArrayList();

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
    }

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




    void Update()
    {

        // end game after appropriate number of trials
        if ((sp.numTraversals >= sp.numTrialsTotal) | (sp.numRewards >= sp.maxRewards & transform.position.z < 0f))
        {
            UnityEditor.EditorApplication.isPlaying = false;

        }

        if (dl.r > 0 & dl.rflag < 1) { StartCoroutine(DeliverReward(dl.r)); dl.rflag = 1; }; // deliver appropriate reward

        // manual rewards and punishments
        mRewardFlag = 0;
        if (Input.GetKeyDown(KeyCode.Q) | Input.GetMouseButtonDown(0)) // reward left - sweetened condensed milk
        {
            mRewardFlag = 1;
            StartCoroutine(DeliverReward(4));
            sp.numRewards += 1;

        }



    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);


        if (other.tag == "Reward")
        {
           StartCoroutine(RewardSequence(transform.position.z,other.gameObject)); 
        }
        else if (other.tag == "Teleport")
        {
            sp.numTraversals += 1;
            tendFlag = 1;

            PositionObjects(tb.trialAnglesList[sp.numTraversals], radius, reward.transform.position);
            
            bckgndOn = true;

            StartCoroutine(InterTrialTimeout());

            LastRewardTime = Time.realtimeSinceStartup; // to avoid issues with teleports
        }
        else if (other.tag == "Start")
        {
            cmd = 0;
            tstartFlag = 1;
            
            //rflag = true;
        }
      

    }

    IEnumerator InterTrialTimeout()
    {

        rotary.toutBool = 0f;
        if (sbxttls.scanning>0)
        {
            if (prevReward == 0) // omission or probe trial 
            {
                sendString("L0");
                yield return new WaitForSeconds(5f + UnityEngine.Random.value * 4f);
                sendString("L1");
                yield return new WaitForSeconds(1f);

            }
            else
            {
                sendString("L0");
                yield return new WaitForSeconds(UnityEngine.Random.value * 4f);
                sendString("L1");
                yield return new WaitForSeconds(1f);
            }
        } else
        {
            if (prevReward == 0) // omission or probe trial 
            {
                yield return new WaitForSeconds(5f + UnityEngine.Random.value * 4f);
                yield return new WaitForSeconds(1f);

            }
            else
            {
                yield return new WaitForSeconds(UnityEngine.Random.value * 4f);
                yield return new WaitForSeconds(1f);
            }

        }

        rotary.toutBool = 1f;
        prevReward = 0;
        yield return null;
    }

    void OnApplicationQuit()
    {
        panoCam.SetActive(false);
    }

    IEnumerator RewardSequence(float pos,GameObject _reward)
    {   // water reward
        rzoneFlag = 1;
       
        
        while ((transform.position.z <= pos + 75)  )
        { 
            
            
            if ((sp.AutoReward) & (transform.position.z > pos + 50))
            { 
     
               
                cmd = 4;
                StartCoroutine(DeliverReward(1));
                sp.numRewards += 1;
                prevReward = 1;
                yield return new WaitForEndOfFrame();
                break;
                   
               
            }

            if (dl.c_1 > 0) {
                    
                cmd = 4;
                sp.numRewards += 1;
                prevReward = 1;
                yield return new WaitForEndOfFrame();
                break;
            }
            yield return new WaitForEndOfFrame();
           
        }
        _reward.SetActive(false);
       
        //if ((sp.sceneName == "NeuroMods_LocationA"))
        //{
        //    
        //    reward.SetActive(false);
//
  //      } else if ((sp.sceneName == "NeuroMods_LocationB"))
    //    {
      //      reward.SetActive(false);
        //} else
        //{
         //   reward_a.SetActive(false);
          //  reward_b.SetActive(false);
           // reward_c.SetActive(false);
        //}
        
        
        
        
        rzoneFlag = 0;
        yield return new WaitForEndOfFrame();
        cmd = 2;
        yield return new WaitForEndOfFrame();
        cmd = 0;

    }

    

    IEnumerator LightsOn()
    {
        panoCam.SetActive(true);
        yield return null;
    }


    IEnumerator DeliverReward(int r)
    { // deliver


        if (r ==4)
        {
            cmd = 4;
            //sp.numRewards += 1;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            cmd = 0;
            yield return new WaitForEndOfFrame();
        }
        {
            yield return null;
        };



    }

    // Moves player and end wall to proper positions relative to start angle given by user and reward location
    void PositionObjects(float angle, float radius, Vector3 rewardPos){

        // Move player to initial posiiton
        float angleRad = angle * Mathf.Deg2Rad;
        Vector3 playerPos = new Vector3(radius*Mathf.Cos(angleRad), 0.0f, radius*Mathf.Sin(angleRad));
        transform.position = playerPos;

        Debug.Log("Current player position in world space: " + transform.position);

        // Calculate end wall position and move end wall to it
        Vector3 distToReward = rewardPos - playerPos;
        float cosTheta = Vector3.Dot(Vector3.Normalize(-playerPos), Vector3.Normalize(distToReward));
        float theta = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;
        float relativeToWall = 2 * radius * cosTheta;
        Vector3 wallPos = Vector3.Normalize(distToReward) * relativeToWall + playerPos;
        endWall.transform.position = wallPos;

        Debug.Log("Current end wall position in world space: " + endWall.transform.position);

        // Rotate player to face towards end wall
        float oppAngle = (-90 - angle) % 360;   // Angle so z-axis of player faces arena origin
        float thetaP = oppAngle + theta;    // Angle z-axis of player faces end wall
        transform.eulerAngles = new Vector3(0.0f, thetaP, 0.0f);

        Debug.Log("Current player rotation in world space: " + transform.eulerAngles);

        // Rotate end wall position
        //float thetaW = Mathf.Rad2Deg* Mathf.Atan2(wallPos.x, wallPos.z);
        //endWall.transform.eulerAngles = new Vector3(0.0f, -thetaW, 0.0f);
    }

    //Moves end wall to position in arena in relation to the initial player position and reward location
    void PositionEndWall(Vector3 playerpos, Vector3 rewardpos, float radius){

        //Calculate end wall position
        Vector3 distToReward = rewardpos - playerpos;
        float cosTheta = Vector3.Dot(Vector3.Normalize(-playerpos), Vector3.Normalize(distToReward));
        float relativeToWall = 2 * radius * cosTheta;
        Vector3 wallPos = Vector3.Normalize(distToReward) * relativeToWall + playerpos;

        //Calculate end wall rotation
        double wallAngleRad = Math.Atan2(wallPos.x, wallPos.z);
        float wallAngleRadFloat = Convert.ToSingle(wallAngleRad);
        float wallAngleDeg = wallAngleRadFloat * Mathf.Rad2Deg;

        //Move and rotate end wall
        endWall.transform.position = wallPos;
        endWall.transform.eulerAngles = new Vector3(0.0f, wallAngleDeg, 0.0f);
    }

    void PositionPlayer(float angle, float radius){
        
        //Move to initial position
        float angleRad = angle *Mathf.Deg2Rad;
        transform.position = new Vector3(radius*Mathf.Cos(angleRad), 0.0f, radius*Mathf.Sin(angleRad));

        //Rotate
        float angleTransform = (angle + 180) % 360;
        transform.eulerAngles = new Vector3(0.0f, -angleTransform, 0.0f);

        Debug.Log("Current position in world space: " + transform.position);
    }

}
