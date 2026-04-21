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
    private int numTrialsInit = 200;
    private int angleIdx = 0;

    private int numBlocks;
    private int anglesPerBlock;

    //public float[] startAngles = {0, 45, 90, 180};
    //public int[] blockChanges = {0, 4, 8, 12};
    //int[] trialIndexList;       // Not sure if needed, keeping for now

    void Awake()
    {
        player = GameObject.Find("Player");
        sp = player.GetComponent<SP_2DTrack>();

        // Initialize empty list to hold shuffled angles
        trialAnglesList = new float[numTrialsInit];

        // Create blocks depending on max angle, fills blocks with list of possible angles, angles in order (1 to max angle in block, inclusive)
        List<List<int>> blocks = CreateBlocks(maxAngle);

        if (anglesPerBlock <= 0)
        {
            Debug.LogError("TrialBlocks_2DTrack: anglesPerBlock is 0, check max angle value.");
            return;
        }

        while (angleIdx < numTrialsInit)
        {

            // Shuffle each block in list of blocks
            for (int b = 0; b < blocks.Count; b++)
            {
                blocks[b] = ShuffleList(blocks[b]);
            }

            // Repeat every block size
            for (int i = 0; i < anglesPerBlock; i++)
            {

                // Shuffle block order
                List<int> blockOrder = ShuffleList(CreateRange(blocks.Count));

                // Assign i val in each block to overall angle list
                foreach (int blockIdx in blockOrder)
                {
                    if (angleIdx >= numTrialsInit) break;
                    trialAnglesList[angleIdx++] = blocks[blockIdx][i];
                }
                if (angleIdx >= numTrialsInit) break;

            }
        }
    }

    List<List<int>> CreateBlocks(int maxAngle)
    {
        if (maxAngle == 45) numBlocks = 1;
        else if (maxAngle == 90) numBlocks = 2;
        else if (maxAngle == 120 || maxAngle == 150) numBlocks = 3;
        else if (maxAngle == 180) numBlocks = 4;
        else numBlocks = 1;

        anglesPerBlock = maxAngle / numBlocks;
        List<List<int>> blocks = new List<List<int>>();

        for (int b = 0; b < numBlocks; b++)
        {
            List<int> block = new List<int>();
            int start = b * anglesPerBlock + 1;
            int end = (b == numBlocks - 1) ? maxAngle : (b + 1) * anglesPerBlock;
            for (int angle = start; angle <= end; angle++)
                block.Add(angle);
            blocks.Add(block);
        }
        return blocks;
    }

    List<int> CreateRange(int count)
    {
        List<int> range = new List<int>();
        for (int i = 0; i < count; i++) range.Add(i);
        return range;
    }

    List<int> ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return list;
    }

    void Update()
    {

    }
}