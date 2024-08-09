using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class TrialBlocks_2DTrack : MonoBehaviour
{
    private GameObject player;
    private GameObject reward;
    private GameObject endWall;

    public int[] startAngles = {0, 45, 90, 180};
    public int[] blockChanges = {0, 4, 8, 12};

    private Vector3 startPosition;
    private Vector3 rotation;
    private float radius = 200;

    private SP_2DTrack sp;
    
    void Start()
    {
        // Reference objects
        player = GameObject.Find("Player");
        reward = GameObject.Find("Reward");
        endWall = GameObject.Find("End Wall");

        // Reference methods
        sp = player.GetComponent<SP_2DTrack>();

        // Positions player and end wall according to first location on list
        PositionPlayer(startAngles[0]);
        PositionEndWall(startPosition, reward.transform.position);
    }


    // How to make this update happen only once at the beginning of the trial???
    
    void Update()
    {
        // Changes position of player and end wall if traversal number is in the list of block changes
        int index = Array.IndexOf(blockChanges, sp.numTraversals);
        if (index > -1){
            PositionPlayer(startAngles[index]);
            PositionEndWall(startPosition, reward.transform.position);
        }
    }

    // This method moves the player to the position defined by the angle given in degrees in respect to the circular arena.
    void PositionPlayer(int angle)
    {
        // Rotates player by angle degrees in respect to center of arena
        transform.RotateAround(Vector3.zero, Vector3.up, angle);
        
        // Store updated start position
        startPosition = transform.position;
        
        // Store updated rotation
        rotation = transform.rotation.eulerAngles;
    }

    // This method moves the end wall by taking in the player position and reward position and drawing a line through them to put the end wall at the other end of the arena
    void PositionEndWall(Vector3 playerpos, Vector3 rewardpos)
    {
         // Get end wall position
        Vector3 relativeToReward = rewardpos - playerpos;
        float cosTheta = Vector3.Dot(Vector3.Normalize(-playerpos), Vector3.Normalize(relativeToReward));
        float relativeToWall = 2 * radius * cosTheta;
        Vector3 endWallPos = Vector3.Normalize(relativeToReward) * relativeToWall + playerpos;

        // Get end wall rotation, which is a double that gets converted to float
        double endWallAngleRad = Math.Atan2(endWallPos.x, endWallPos.z);
        float endWallAngleRadFloat = Convert.ToSingle(endWallAngleRad);
        float endWallAngleDeg = endWallAngleRadFloat * Mathf.Rad2Deg;

        // Move end wall to new position and rotation
        endWall.transform.position = endWallPos;
        endWall.transform.eulerAngles = new Vector3 (0.0f, endWallAngleDeg, 0.0f);
    }

}
