using UnityEngine;

public class OvalTraverser_with_conditions : MonoBehaviour
{
    public enum TraversalCondition
    {
        HighCenterAndNonCenter = 1,
        HighCenterOnly = 2,
        LowCenterAndNonCenter = 3
    }

    [Header("Experimental Condition")]
    [Tooltip("1 = high center + high non-center, 2 = high center + low non-center, 3 = low center + low non-center")]
    [Range(1, 3)]
    public int conditionID = 1;

    [Header("Movement Settings")]
    public float traverseSpeed = 10f;
    public float traverseDistance = 90f; // degrees from center to each side (center at 0 offset)

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
    public float distanceFromPlayer = 50f;
    public float heightOffset = 10f;

    [Header("References")]
    public Transform player;
    public bool useCamera = true;

    private Transform followTarget;
    private float traverseTimer = 0f;
    private float lastAngleOffset = 0f;

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

    void Awake()
    {
        ApplyCondition();
    }

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

    /// <summary>
    /// Applies one of the three predefined experimental conditions.
    /// The movement speed is currently 2 deg/sec for all three conditions.
    /// High probability = 0.40, low probability = 0.05.
    /// Duration ranges are left at their existing Inspector values for now.
    /// </summary>
    public void ApplyCondition()
    {
        const float highProbability = 0.40f;
        const float lowProbability = 0.05f;
        const float conditionSpeed = 10.0f;

        traverseSpeed = conditionSpeed;

        switch (conditionID)
        {
            case 1:
                // High center + high non-center
                pauseProbability = highProbability;
                randomFreezeProbability = highProbability;
                break;

            case 2:
                // High center + low non-center
                pauseProbability = highProbability;
                randomFreezeProbability = lowProbability;
                break;

            case 3:
                // Low center + low non-center
                pauseProbability = lowProbability;
                randomFreezeProbability = lowProbability;
                break;

            default:
                Debug.LogWarning("OvalTraverser: Invalid conditionID " + conditionID + ". Using condition 1.");
                conditionID = 1;
                pauseProbability = highProbability;
                randomFreezeProbability = highProbability;
                break;
        }
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
        float preMoveAngleOffset =
            Mathf.PingPong(traverseTimer, traverseDistance * 2f) - traverseDistance;
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
                frozenAngleRadians = preMoveAngleRadians;
                ApplyFrozenAnglePosition();

                // do not advance traverseTimer this frame; remain frozen
                return;
            }
        }

        // --- ADVANCE traverseTimer for movement (only when not paused/frozen) ---
        traverseTimer += Time.deltaTime * traverseSpeed;

        // Compute post-move angle
        float postMoveAngleOffset =
            Mathf.PingPong(traverseTimer, traverseDistance * 2f) - traverseDistance;
        float postMoveAngleRadians = (postMoveAngleOffset + 90f) * Mathf.Deg2Rad;

        // --- CENTER CROSSING DETECTION ---
        bool crossedCenter = false;

        if (!Mathf.Approximately(preMoveAngleOffset, postMoveAngleOffset))
        {
            crossedCenter = (preMoveAngleOffset < 0f && postMoveAngleOffset >= 0f)
                            || (preMoveAngleOffset > 0f && postMoveAngleOffset <= 0f);
        }

        if (crossedCenter && !crossedCenterLastFrame && !isPaused)
        {
            if (Random.value <= pauseProbability)
            {
                // Pause at the post-move angle
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

        // --- NORMAL MOVEMENT ---
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
