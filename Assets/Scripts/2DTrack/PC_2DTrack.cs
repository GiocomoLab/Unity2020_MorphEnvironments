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
    private GameObject panoCam;     // This might not be needed
    private GameObject endWall;
    private GameObject startObjects;
    private GameObject anchor;

    private Rigidbody rb;

    private SP_2DTrack sp;
    private DL_2DTrack dl;
    private RR_2DTrack rotary;
    private SbxTTLs_2DTrack sbxttls;
    private TrialBlocks_2DTrack tb;

    private bool reward_dir;

    private Vector3 initialPosition;        // This might not be needed
    private Vector3 movement;           // This might not be needed
    private Vector3 playerPos;

    private static bool created = false;
    private int r;

    public int cmd = 2;
    private bool flashFlag = false;     // Not on Can's script
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
    public float teleportDistance = 10;





    public void Start()
    {
        player = GameObject.Find("Player");
        endWall = GameObject.Find("End Wall");
        startObjects = GameObject.Find("StartObjects");
        
        sp = player.GetComponent<SP_2DTrack>();
        dl = player.GetComponent<DL_2DTrack>();
        rotary = player.GetComponent<RR_2DTrack>();
        sbxttls = player.GetComponent<SbxTTLs_2DTrack>();
        tb = player.GetComponent<TrialBlocks_2DTrack>();

        StartCoroutine(FlagCheck());
        
        //Debug.Log(sp.sceneName);

        panoCam = GameObject.Find("panoCamera");
        //panoCam.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);        // Needed?
        reward = GameObject.Find("Reward");

        anchor = GameObject.Find("Anchor");

        PositionObjects(tb.trialAnglesList[sp.numTraversals], radius, anchor.transform.position);
        //Debug.Log("Start angle: " + tb.trialAnglesList[sp.numTraversals]);

        //if (UnityEngine.Random.value < sp.autoRewardPercent){
        //    sp.AutoReward = true;
        //}
        //else{
        //    sp.AutoReward = false;
        //}
        sp.AutoReward = true;

        //Debug.Log("Auto reward: " + sp.AutoReward);

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
            StartCoroutine(RewardSequence(transform.position, other.gameObject));
        }
        else if (other.tag == "Teleport")
        {
            //Debug.Log("Teleport");

            if (UnityEngine.Random.value < sp.SkipTrialPcnt)
            {
                reward.SetActive(false);
            }
            else
            {
                reward.SetActive(true);
            }
            
            sp.numTraversals += 1;
            tendFlag = 1;

            PositionObjects(tb.trialAnglesList[sp.numTraversals], radius, anchor.transform.position);
            //Debug.Log("Start angle: " + tb.trialAnglesList[sp.numTraversals]);

            //if (UnityEngine.Random.value < sp.autoRewardPercent){
            //    sp.AutoReward = true;
            //}
            //else{
            //    sp.AutoReward = false;
            //}

            if (sp.numTraversals < 10)
            {
                sp.AutoReward = true;
            }
            else
            {
                sp.AutoReward = false;
            }

            //Debug.Log("Auto reward: " + sp.AutoReward);

            bckgndOn = true;

            //StartCoroutine(InterTrialTimeout());

            LastRewardTime = Time.realtimeSinceStartup; // to avoid issues with teleports
        }
        else if (other.tag == "Start")
        {
            cmd = 0;
            tstartFlag = 1;
            
            //rflag = true;
        }
      

    }

    IEnumerator FlagCheck(){
        while (true){
            yield return new WaitForEndOfFrame();
            tendFlag = 0;
            tstartFlag = 0;
        }
        yield return null;
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

    IEnumerator RewardSequence(Vector3 rewardStart,GameObject _reward)
    {   // water reward
        rzoneFlag = 1;
        //Debug.Log("Entered reward zone: " + rewardStart);

        //Calculate end of reward zone
        Vector3 rewardEnd = GetRewardEnd(rewardStart, 50, playerPos);
       // Debug.Log("Calculated reward zone end: " + rewardEnd);

        //Calculate reward zone boundaries
        float rewardMinX = Mathf.Min(rewardStart.x, rewardEnd.x);
        float rewardMaxX = Mathf.Max(rewardStart.x, rewardEnd.x);
        float rewardMinZ = Mathf.Min(rewardStart.z, rewardEnd.z);
        float rewardMaxZ = Mathf.Max(rewardStart.z, rewardEnd.z);

        //Calculate auto reward boundary
        Vector3 rewardAuto = GetRewardEnd(rewardStart, 30, playerPos);

        //Calculate auto reward zone boundaries
        float rewardAutoMinX = Mathf.Min(rewardAuto.x, rewardEnd.x);
        float rewardAutoMaxX = Mathf.Max(rewardAuto.x, rewardEnd.x);
        float rewardAutoMinZ = Mathf.Min(rewardAuto.z, rewardEnd.z);
        float rewardAutoMaxZ = Mathf.Max(rewardAuto.z, rewardEnd.z);

        while (Vector3.Distance(transform.position, rewardStart) < 50)
        {
            if((sp.AutoReward) & (Vector3.Distance(transform.position, rewardStart) > 30)){

                //Debug.Log("Auto reward on: " + transform.position);
                cmd = 4;
                StartCoroutine(DeliverReward(1));
                sp.numRewards += 1;
                prevReward = 1;
                yield return new WaitForEndOfFrame();
                break;
            }

            if (dl.c_1 > 0)
            {
                cmd = 4;
                sp.numRewards += 1;
                prevReward = 1;
                yield return new WaitForEndOfFrame();
                break;
            }
            yield return new WaitForEndOfFrame();

        }
        //while (transform.position.x >= rewardMinX && transform.position.x <= rewardMaxX && transform.position.z >= rewardMinZ && transform.position.z <= rewardMaxZ){

        //    if((sp.AutoReward) & (transform.position.x >= rewardAutoMinX && transform.position.x <= rewardAutoMaxX && transform.position.z >= rewardAutoMinZ && transform.position.z <= rewardAutoMaxZ)){

        //        Debug.Log("Auto reward on: " + transform.position);
        //        cmd = 4;
        //        StartCoroutine(DeliverReward(1));
        //        sp.numRewards += 1;
        //        prevReward = 1;
        //        yield return new WaitForEndOfFrame();
        //        break;
        //    }

        //    if (dl.c_1 > 0) {
                    
        //        cmd = 4;
        //        sp.numRewards += 1;
        //        prevReward = 1;
        //        yield return new WaitForEndOfFrame();
        //        break;
        //    }
        //    yield return new WaitForEndOfFrame();
           
        //}
        _reward.SetActive(false);

        rzoneFlag = 0;
        //Debug.Log("Exited reward zone: " + transform.position);
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
        float radAngle = angle * Mathf.Deg2Rad;
        playerPos = new Vector3(radius*Mathf.Cos(radAngle), 0.0f, radius*Mathf.Sin(radAngle));

        Vector3 distToReward = rewardPos - playerPos;
        Vector3 distToRewardNorm = Vector3.Normalize(-distToReward);
        Vector3 playerPosInTunnel = playerPos + distToRewardNorm * teleportDistance;

        //Vector3 playerPosInTunnel = new Vector3((radius + teleportDistance) * Mathf.Cos(radAngle), 0.0f, (radius + teleportDistance) * Mathf.Sin(radAngle));
        transform.position = playerPosInTunnel;

        //Debug.Log("Player start position: " + playerPos);

        //Debug.Log("Player position in tunnel: " + transform.position);

        // Calculate end wall position and move end wall to it
        float cosTheta = Vector3.Dot(Vector3.Normalize(-playerPos), Vector3.Normalize(distToReward));
        float theta = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;
        float relativeToWall = 2 * radius * cosTheta;
        Vector3 wallPos = Vector3.Normalize(distToReward) * relativeToWall + playerPos;
        endWall.transform.position = new Vector3(wallPos.x, 0.0f, wallPos.z);
        //Debug.Log("End wall position: " + endWall.transform.position);

        // Rotate player to face towards end wall
        //float oppAngle = (-angle + 270) % 360;   // Angle so z-axis of player faces arena origin

        //Check sign
        //float tempvalue = rewardPos.x * Mathf.Cos(oppAngle*Mathf.Deg2Rad) - rewardPos.z * Mathf.Sin(oppAngle*Mathf.Deg2Rad);
        //float thetaP;
        //if (tempvalue > 0){
        //    thetaP = oppAngle + theta;
        //}
        //else{
        //    thetaP = oppAngle - theta;
        //}
        
        //transform.eulerAngles = new Vector3(0.0f, thetaP, 0.0f);
        //Debug.Log("Player rotation: " + thetaP);
        transform.rotation = Quaternion.LookRotation(distToReward);
        //Debug.Log("Player rotation: " + transform.eulerAngles);

        // Rotate end wall
        double wallAngleRad = Math.Atan2(wallPos.z, wallPos.x);
        float wallAngleRadFloat = Convert.ToSingle(wallAngleRad);
        endWall.transform.eulerAngles = new Vector3(0.0f, -wallAngleRadFloat*Mathf.Rad2Deg, 0.0f);

        // Move start objects to initial position
        startObjects.transform.position = playerPos;

        // Rotate start wall to initial position
        startObjects.transform.eulerAngles = new Vector3(0.0f, -angle, 0.0f);
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

        //Debug.Log("Current position in world space: " + transform.position);
    }

    Vector3 GetRewardEnd(Vector3 playerpos, float rewarddist, Vector3 startpos){

        //Calculate distance traversed, or between player start position and current player position
        Vector3 distTraversed = playerpos - startpos;

        //Get length of distance traversed
        float lengthTraversed = distTraversed.magnitude;

        //Add reward distance to vector length
        float rewardLength = lengthTraversed + rewarddist;

        //Make vector length the reward length
        Vector3 normVector = Vector3.Normalize(distTraversed);
        Vector3 rewardEnd = rewardLength * normVector + startpos;
        return rewardEnd;
    }

}
