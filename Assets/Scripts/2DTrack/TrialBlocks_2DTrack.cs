using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class TrialBlocks_2DTrack : MonoBehaviour
{
    private SP_2DTrack sp;
    private GameObject player;

    public float[] startAngles = {0, 45, 90, 180};
    public int[] blockChanges = {0, 4, 8, 12};
    int[] trialIndexList;       // Not sure if needed, keeping for now
    public float[] trialAnglesList;
    int randomAngle;
    
    void Awake()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_2DTrack>();

        trialIndexList = new int[sp.numTrialsTotal];
        trialAnglesList = new float[sp.numTrialsTotal];

        for (int i = 0; i < sp.numTrialsTotal; i++){
            randomAngle = UnityEngine.Random.Range(0, 46);
            trialAnglesList[i] = randomAngle;

            // int tempInd = 0;
            // for (int j = 0; j < blockChanges.Length; j++){
            //     if (i >= blockChanges[j]){
            //         tempInd = j;
            //     }
            // }
            // trialIndexList[i] = tempInd;
            // trialAnglesList[i] = startAngles[tempInd];
        }
    }
    
    void Update()
    {

    }
}
