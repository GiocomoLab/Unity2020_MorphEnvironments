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

    public int maxAngle = 180;
    public float[] trialAnglesList;

    //public float[] startAngles = {0, 45, 90, 180};
    //public int[] blockChanges = {0, 4, 8, 12};
    //int[] trialIndexList;       // Not sure if needed, keeping for now
    
    void Awake()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_2DTrack>();

        trialAnglesList = new float[sp.numTrialsTotal];

        // Create a list of all possible angles (0 to maxAngle, inclusive)
        List<int> availableAngles = new List<int>();
        for (int j = 0; j <= maxAngle; j++){
            availableAngles.Add(j);
        }

        // Shuffle and fill trials
        int angleIndex = 0;
        for (int i = 0; i < sp.numTrialsTotal; i++){
            // If we've used all angles, reshuffle
            if (angleIndex == 0){
                for (int j = availableAngles.Count - 1; j > 0; j--){
                    int k = UnityEngine.Random.Range(0, j + 1);
                    int temp = availableAngles[j];
                    availableAngles[j] = availableAngles[k];
                    availableAngles[k] = temp;
                }
            }

            trialAnglesList[i] = availableAngles[angleIndex];
            angleIndex = (angleIndex + 1) % (maxAngle + 1);  // Wrap around to 0 after maxAngle
        }

    }
    
    void Update()
    {

    }
}
