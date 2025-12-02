using UnityEngine;
public class OvalTraverser : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of left-right movement")]
    public float traverseSpeed = 2f;
    [Tooltip("Maximum angle to move left/right from center (in degrees)")]
    public float traverseDistance = 45f;
    [Header("Position Settings")]
    [Tooltip("Distance in front of player")]
    public float distanceFromPlayer = 3f;
    [Tooltip("Height offset relative to player")]
    public float heightOffset = 0f;
    [Header("References")]
    [Tooltip("The player/camera to follow")]
    public Transform player;
    [Tooltip("Use the camera instead of player transform (recommended)")]
    public bool useCamera = true;
    private float traverseTimer = 0f;
    private Transform followTarget;
    void Start()
    {
        // If no player assigned, try to find the main camera
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    player = mainCam.transform;
                }
            }
        }
        if (player == null)
        {
            Debug.LogError("OvalTraverser: No player transform found!");
            return;
        }
        // Find the panoCamera specifically (not the subcameras)
        Transform panoCamera = player.transform.Find("panoCamera");
        if (panoCamera != null)
        {
            followTarget = panoCamera;
            Debug.Log("OvalTraverser: Using panoCamera as center");
        }
        else
        {
            // Fallback to player if panoCamera not found
            followTarget = player;
            Debug.LogWarning("OvalTraverser: panoCamera not found, using player transform");
        }
        // Initialize position in front of camera/player immediately
        UpdatePosition();
    }
    void UpdatePosition()
    {
        if (followTarget == null) return;
        // Update traverse timer
        traverseTimer += Time.deltaTime * traverseSpeed;
        // Calculate angle using PingPong for linear back-and-forth
        // PingPong creates movement from -traverseDistance to +traverseDistance (in degrees)
        float angleOffset = Mathf.PingPong(traverseTimer, traverseDistance * 2) - traverseDistance;
        // Convert angle to radians for calculation
        float angleRadians = (angleOffset + 90f) * Mathf.Deg2Rad;
        // Calculate position on semi-circle around panoCamera
        // The semi-circle is in the horizontal plane (X-Z plane in local space)
        // Using Sin for horizontal (right) and Cos for depth (forward)
        Vector3 horizontalComponent = followTarget.right * Mathf.Sin(angleRadians) * distanceFromPlayer;
        Vector3 depthComponent = followTarget.forward * Mathf.Cos(angleRadians) * distanceFromPlayer;
        // Combine components - this creates a true semi-circle in the horizontal plane
        Vector3 targetPosition = followTarget.position
            + horizontalComponent      // Left-right movement
            + depthComponent          // Forward-backward movement (creates the arc)
            + followTarget.up * heightOffset;
        transform.position = targetPosition;
        // Make the oval always face toward the panoCamera center
        transform.LookAt(followTarget.position + followTarget.up * heightOffset);
    }
    void Update()
    {
        UpdatePosition();
    }
}