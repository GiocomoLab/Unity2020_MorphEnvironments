using UnityEngine;
public class OvalTraverser : MonoBehaviour
{
    [Header("Movement Settings")]
    public float traverseSpeed = 2f;
    public float traverseDistance = 45f; // degrees from center to each side (center at 0 offset)
    [Header("Center Pause Settings")]
    [Range(0f, 1f)] public float pauseProbability = 0.0f;
    public float minPauseDuration = 0.0f;
    public float maxPauseDuration = 1.0f;
    [Header("Random Freeze Settings")]
    [Range(0f, 1f)] public float randomFreezeProbability = 0.0f;
    public float minFreezeDuration = 0.0f;
    public float maxFreezeDuration = 1.0f;
    public float freezeCheckInterval = 0.5f;
    [Header("Position Settings")]
    public float distanceFromPlayer = 3f;
    public float heightOffset = 0f;
    [Header("References")]
    public Transform player;
    public bool useCamera = true;
    private Transform followTarget;
    private float traverseTimer = 0f;      // drives PingPong
    private float lastAngleOffset = 0f;   // degrees, in range [-traverseDistance, +traverseDistance]
    // Freeze/Pause state
    private bool isPaused = false;
    private float pauseEndTime = 0f;
    private bool isFrozen = false;
    private float freezeEndTime = 0f;
    private float nextFreezeCheckTime = 0f;
    private float frozenAngleRadians = 0f;
    private bool crossedCenterLastFrame = false;
    // Tolerance (degrees) to help robust center detection
    private const float centerTolerance = 0.001f;
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else if (Camera.main != null)
                player = Camera.main.transform;
        }
        if (player == null)
        {
            Debug.LogError("OvalTraverser: No player transform found!");
            return;
        }
        Transform panoCamera = player.transform.Find("panoCamera");
        followTarget = panoCamera != null ? panoCamera : player;
        lastAngleOffset = Mathf.PingPong(traverseTimer, traverseDistance * 2f) - traverseDistance;
        nextFreezeCheckTime = Time.time + freezeCheckInterval;
        UpdatePosition(); // position at start
    }
    void Update()
    {
        UpdatePosition();
    }
    void UpdatePosition()
    {
        if (followTarget == null) return;
        // If currently frozen, check expiration first
        if (isFrozen)
        {
            if (Time.time >= freezeEndTime)
            {
                isFrozen = false;
            }
            else
            {
                ApplyFrozenAnglePosition();
                return;
            }
        }
        // If currently paused at center, check expiration
        if (isPaused)
        {
            if (Time.time >= pauseEndTime)
            {
                isPaused = false;
            }
            else
            {
                ApplyFrozenAnglePosition();
                return;
            }
        }
        // --- Compute current (pre-move) angle from current traverseTimer ---
        float preMoveAngleOffset = Mathf.PingPong(traverseTimer, traverseDistance * 2f) - traverseDistance; // degrees
        float preMoveAngleRadians = (preMoveAngleOffset + 90f) * Mathf.Deg2Rad;
        // --- RANDOM FREEZE CHECK (uses pre-move angle) ---
        if (Time.time >= nextFreezeCheckTime && randomFreezeProbability > 0f)
        {
            // schedule next check now (regardless of result) so checks are spaced
            nextFreezeCheckTime = Time.time + freezeCheckInterval;
            if (Random.value < randomFreezeProbability)
            {
                // Trigger freeze at the current visual angle (preMove)
                isFrozen = true;
                freezeEndTime = Time.time + Random.Range(minFreezeDuration, maxFreezeDuration);
                frozenAngleRadians = preMoveAngleRadians; // lock the current visual angle
                ApplyFrozenAnglePosition();
                // do not advance traverseTimer this frame; remain frozen
                return;
            }
        }
        // --- ADVANCE traverseTimer for movement (only when not paused/frozen) ---
        traverseTimer += Time.deltaTime * traverseSpeed;
        // Compute post-move angle (this is the ball's new intended position this frame)
        float postMoveAngleOffset = Mathf.PingPong(traverseTimer, traverseDistance * 2f) - traverseDistance;
        float postMoveAngleRadians = (postMoveAngleOffset + 90f) * Mathf.Deg2Rad;
        // --- CENTER CROSSING DETECTION (detect crossing of angleOffset == 0) ---
        bool crossedCenter = false;
        // Only consider crossing if there was actual motion across zero between pre and post offsets
        if (!Mathf.Approximately(preMoveAngleOffset, postMoveAngleOffset))
        {
            crossedCenter = (preMoveAngleOffset < 0f && postMoveAngleOffset >= 0f)
                            || (preMoveAngleOffset > 0f && postMoveAngleOffset <= 0f);
        }
        if (crossedCenter && !crossedCenterLastFrame && !isPaused)
        {
            if (Random.value <= pauseProbability)
            {
                // Pause at the post-move angle (this represents the center or immediately after crossing)
                isPaused = true;
                pauseEndTime = Time.time + Random.Range(minPauseDuration, maxPauseDuration);
                frozenAngleRadians = postMoveAngleRadians;
                ApplyFrozenAnglePosition();
                lastAngleOffset = postMoveAngleOffset;
                crossedCenterLastFrame = true;
                return;
            }
        }
        crossedCenterLastFrame = crossedCenter;
        // --- NORMAL MOVEMENT: apply computed post-move position ---
        ApplyAnglePosition(postMoveAngleRadians);
        // Save for next frame
        lastAngleOffset = postMoveAngleOffset;
    }
    // Apply the normal (moving) position for a given angleRadians
    void ApplyAnglePosition(float angleRadians)
    {
        Vector3 horizontal = followTarget.right * Mathf.Sin(angleRadians) * distanceFromPlayer;
        Vector3 depth = followTarget.forward * Mathf.Cos(angleRadians) * distanceFromPlayer;
        transform.position =
            followTarget.position +
            horizontal +
            depth +
            followTarget.up * heightOffset;
        transform.LookAt(followTarget.position + followTarget.up * heightOffset);
    }
    // Apply frozen angle (angle locked) but player movement still affects world position
    void ApplyFrozenAnglePosition()
    {
        Vector3 horizontal = followTarget.right * Mathf.Sin(frozenAngleRadians) * distanceFromPlayer;
        Vector3 depth = followTarget.forward * Mathf.Cos(frozenAngleRadians) * distanceFromPlayer;
        transform.position =
            followTarget.position +
            horizontal +
            depth +
            followTarget.up * heightOffset;
        transform.LookAt(followTarget.position + followTarget.up * heightOffset);
    }
    // Optional helper for debugging / external queries
    public float GetCurrentAngleOffsetDegrees()
    {
        return Mathf.PingPong(traverseTimer, traverseDistance * 2f) - traverseDistance;
    }
}