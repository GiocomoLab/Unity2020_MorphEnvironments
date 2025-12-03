using UnityEngine;
public class OvalTraverser : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of left-right movement")]
    public float traverseSpeed = 2f;
    [Tooltip("Maximum angle to move left/right from center (in degrees)")]
    public float traverseDistance = 45f;

    [Header("center Pause Settings")]
    [Tooltip("Probability 0-1 of pausing when crossing center")]
    [Range(0f, 1f)]
    public float pauseProbability = 0.0f;
    [Tooltip("minimum pause duration")]
    public float minPauseDuration = 0.0f;
    [Tooltip("maximum pause duration")]
    public float maxPauseDuration = 1.0f;

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
    // Pause state variables

    private bool isPaused = false;
    private float pauseEndTime = 0f;
    private bool crossedCenterLastFrame = false;
    private float lastAngleOffset = 0f;
    private Vector3 pausePosition;

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
        // Check if currently paused
        if (isPaused)
        {
            if (Time.time >= pauseEndTime)
            {
                isPaused = false;
                Debug.Log("OvalTraverser: Resuming movement");
            }
            else
            {
                // Stay at center position while paused
                
                transform.position = pausePosition;
                transform.LookAt(followTarget.position + followTarget.up * heightOffset);
                return;
            }
        }
        // Update traverse timer (only when not paused)
        traverseTimer += Time.deltaTime * traverseSpeed;
        // Calculate angle using PingPong for linear back-and-forth
        // PingPong creates movement from -traverseDistance to +traverseDistance (in degrees)
        float angleOffset = Mathf.PingPong(traverseTimer, traverseDistance * 2) - traverseDistance;
        // Check if we just crossed the center (angle changes from negative to positive or vice versa)
        bool crossedCenter = (lastAngleOffset < 0 && angleOffset >= 0) || (lastAngleOffset > 0 && angleOffset <= 0);
        // If we crossed center and haven't already triggered a pause this crossing
        if (crossedCenter && !crossedCenterLastFrame && !isPaused)
        {
            // Roll the dice - should we pause?
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            if (randomValue <= pauseProbability)
            {
                // Initiate pause
                isPaused = true;
                float pauseDuration = UnityEngine.Random.Range(minPauseDuration, maxPauseDuration);
                pauseEndTime = Time.time + pauseDuration;
                pausePosition = transform.position;
                Debug.Log($"OvalTraverser: Pausing at center for {pauseDuration:F2} seconds");
                // Position at center immediately
                
                crossedCenterLastFrame = true;
                lastAngleOffset = angleOffset;
                return;
            }
        }
        // Update crossing detection
        crossedCenterLastFrame = crossedCenter;
        lastAngleOffset = angleOffset;
        // Convert angle to radians for calculation
        // Add 90 degrees to shift the semi-circle to the front-facing half
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