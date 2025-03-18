using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Data;
using Mono.Data.Sqlite;

public class GetPositions_2DTask : MonoBehaviour
{
    public string path = "I:/My Drive/";

    private GameObject player;
    private PC_2DTrack pc;
    int reward_diameter = 50;

    private IDbConnection _connection;
    private IDbCommand _command;

    void Start()
    {
        player = GameObject.Find("Player");
        pc = player.GetComponent<PC_2DTrack>();

        Vector3 rewardPos = new Vector3(-70.0f, 0.0f, 70.0f);
        int[] anglesList = Enumerable.Range(0, 359).ToArray();
        int arrayLength = anglesList.Length;

        // Initialize database
        SqliteConnection.CreateFile(path + "positions2.sqlite");
        string connectionString = "Data Source=" + path + "positions2.sqlite;Version=3;";
        _connection = (IDbConnection) new SqliteConnection(connectionString);
        _connection.Open();
        _command = _connection.CreateCommand();

        // Populate metadata table
        _command.CommandText = "create table metadata (reward_center_x REAL, reward_center_z REAL, reward_diameter INT, arena_diameter REAL)";
        _command.ExecuteNonQuery();

        
        _command.CommandText = "insert into metadata (reward_center_x, reward_center_z, reward_diameter, arena_diameter) values (" + rewardPos.x + "," + rewardPos.z + "," + reward_diameter + "," + pc.radius + ")";
        _command.ExecuteNonQuery();

        // Initialize positions table
        _command.CommandText = "create table positions (angle REAL, start_posx REAL, start_posz REAL, end_posx REAL, end_posz REAL, rzone_start_posx REAL, rzone_start_posz REAL, rzone_end_posx REAL, rzone_end_posz REAL)";

        _command.ExecuteNonQuery();

        for (int i = 0; i < arrayLength; i++){

            float[] positions_list = CalculatePositions(anglesList[i], rewardPos);

            _command.CommandText = "insert into positions (angle, start_posx, start_posz, end_posx, end_posz, rzone_start_posx, rzone_start_posz, rzone_end_posx, rzone_end_posz) values (" + anglesList[i] + "," + positions_list[0] + "," + positions_list[1] + "," + positions_list[2] + "," + positions_list[3] + "," + positions_list[4] + "," + positions_list[5] +  "," + positions_list[6] + "," + positions_list[7] + ")";

            _command.ExecuteNonQuery();
        }

        _command.Dispose();
        _command = null;

        _connection.Close();
        _connection = null;
    }


    float[] CalculatePositions(float angle, Vector3 rewardpos){

        float[] coord_list = new float[8];

        float angle_radians = angle * Mathf.Deg2Rad;
        
        // Get x and z coordinates for player start position given angle and radius
        float start_posx = pc.radius * Mathf.Cos(angle_radians);
        float start_posz = pc.radius * Mathf.Sin(angle_radians);

        Vector3 start_pos = new Vector3(start_posx, 0.0f, start_posz);

        // Get x and z coordiantes for player end position given angle and radius
        Vector3 distToReward = rewardpos - start_pos;
        float cosTheta = Vector3.Dot(Vector3.Normalize(-start_pos), Vector3.Normalize(distToReward));
        float theta = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;
        float relativeToWall = 2 * pc.radius * cosTheta;
        Vector3 wallPos = Vector3.Normalize(distToReward) * relativeToWall + start_pos;

        float end_posx = wallPos.x;
        float end_posz = wallPos.z;

        // Get x and z coordinates for reward boundary given angle and radius

        float distToRewardZoneLen = distToReward.magnitude;
        Vector3 rzone_start = Vector3.Normalize(distToReward) * (distToRewardZoneLen - reward_diameter/2) + start_pos;
        Vector3 rzone_end = Vector3.Normalize(distToReward) * (distToRewardZoneLen + reward_diameter/2) + start_pos;

        float rzone_start_posx = rzone_start.x;
        float rzone_start_posz = rzone_start.z;
        float rzone_end_posx = rzone_end.x;
        float rzone_end_posz = rzone_end.z;

        // Populate array
        coord_list[0] = start_posx;
        coord_list[1] = start_posz;
        coord_list[2] = end_posx;
        coord_list[3] = end_posz;
        coord_list[4] = rzone_start_posx;
        coord_list[5] = rzone_start_posz;
        coord_list[6] = rzone_end_posx;
        coord_list[7] = rzone_end_posz;

        return coord_list;
    }

}
