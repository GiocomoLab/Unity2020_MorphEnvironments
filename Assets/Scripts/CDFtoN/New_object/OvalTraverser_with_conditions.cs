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

    // Fixed experimental parameters.
    // These are public read-only properties so other scripts (including SP_new_novelobject)
    // can save them, but Unity will not expose them as editable Inspector fields.
    public float traverseSpeed { get; private set; } = 11.7f;
    public float traverseDistance { get; } = 90f; // degrees from center to each side

    public float pauseProbability { get; private set; } = 0.0f; // derived from lambda and check interval
    public float minPauseDuration { get; } = 0.0f; // retained for SP compatibility
    public float maxPauseDuration { get; } = 18.0f;
    public float centerRange { get; } = 15.0f;
    public float centerGammaShape { get; } = 0.745f;
    public float centerGammaScale { get; } = 6.789f;

    public float randomFreezeProbability { get; private set; } = 0.0f; // derived from lambda and check interval
    public float minFreezeDuration { get; } = 0.0f; // retained for SP compatibility
    public float maxFreezeDuration { get; } = 10.0f;
    public float freezeCheckInterval { get; } = 0.5f;
    public float centerLambda { get; } = 0.702f;  // high event rate (events/sec)
    public float outsideLambda { get; } = 0.363f; // low event rate (events/sec)
    public float outsideGammaShape { get; } = 0.722f;
    public float outsideGammaScale { get; } = 3.806f;

    public float centerTurnBackProbability { get; } = 0.395f;
    public float nonCenterTurnBackProbability { get; } = 0.307f;

    private const float distanceFromPlayer = 50f;
    private const float heightOffset = 10f;
    public bool useCamera { get; } = true;

    [Header("References")]
    public Transform player;

    private Transform followTarget;
    private float traverseTimer = 0f;
    private float lastAngleOffset = 0f;
    private float moveDirection = 1f;

    // Freeze/Pause state
    private bool isPaused = false;
    private float pauseEndTime = 0f;
    private bool isFrozen = false;
    private float freezeEndTime = 0f;
    private float nextFreezeCheckTime = 0f;
    private float frozenAngleRadians = 0f;
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
    /// The movement speed is currently 11.7 deg/sec for all three conditions.
    /// centerLambda is the high event rate and outsideLambda is the low event rate.
    /// Per-check probability is calculated as p = 1 - exp(-lambda * freezeCheckInterval).
    /// Condition 1 = high center + high non-center.
    /// Condition 2 = high center + low non-center.
    /// Condition 3 = low center + low non-center.
    /// </summary>
    public void ApplyCondition()
    {
        const float conditionSpeed = 11.7f;

        traverseSpeed = conditionSpeed;

        float highProbability = LambdaToProbability(centerLambda);
        float lowProbability = LambdaToProbability(outsideLambda);

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

    float LambdaToProbability(float lambda)
    {
        if (lambda <= 0f || freezeCheckInterval <= 0f)
            return 0f;

        return 1f - Mathf.Exp(-lambda * freezeCheckInterval);
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
                // Wait one full check interval after movement resumes before checking again.
                nextFreezeCheckTime = Time.time + freezeCheckInterval;
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
                // Wait one full check interval after movement resumes before checking again.
                nextFreezeCheckTime = Time.time + freezeCheckInterval;
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

        // --- PAUSE/FREEZE CHECK ---
        // Both center and non-center probabilities are checked on the same schedule.
        // Center is defined as the range -centerRange to +centerRange degrees.
        if (Time.time >= nextFreezeCheckTime)
        {
            // Schedule the next check now, regardless of whether a pause/freeze occurs.
            nextFreezeCheckTime = Time.time + freezeCheckInterval;

            bool isInCenter = Mathf.Abs(preMoveAngleOffset) <= centerRange;

            if (isInCenter)
            {
                // CENTER: check pauseProbability once per freezeCheckInterval.
                if (pauseProbability > 0f && Random.value < pauseProbability)
                {
                    isPaused = true;
                    pauseEndTime = Time.time + SampleTruncatedGamma(centerGammaShape, centerGammaScale, maxPauseDuration);
                    frozenAngleRadians = preMoveAngleRadians;

                    // Decide once, when this center pause begins, whether to turn back.
                    if (Random.value < centerTurnBackProbability)
                    {
                        moveDirection *= -1f;
                    }

                    ApplyFrozenAnglePosition();
                    return;
                }
            }
            else
            {
                // NON-CENTER: check randomFreezeProbability once per freezeCheckInterval.
                if (randomFreezeProbability > 0f && Random.value < randomFreezeProbability)
                {
                    isFrozen = true;
                    freezeEndTime = Time.time + SampleTruncatedGamma(outsideGammaShape, outsideGammaScale, maxFreezeDuration);
                    frozenAngleRadians = preMoveAngleRadians;

                    // Decide once, when this non-center pause begins, whether to turn back.
                    if (Random.value < nonCenterTurnBackProbability)
                    {
                        moveDirection *= -1f;
                    }

                    ApplyFrozenAnglePosition();
                    return;
                }
            }
        }

        // --- ADVANCE traverseTimer for movement (only when not paused/frozen) ---
        traverseTimer += Time.deltaTime * traverseSpeed * moveDirection;

        // Compute post-move angle
        float postMoveAngleOffset =
            Mathf.PingPong(traverseTimer, traverseDistance * 2f) - traverseDistance;
        float postMoveAngleRadians = (postMoveAngleOffset + 90f) * Mathf.Deg2Rad;

        // --- NORMAL MOVEMENT ---
        ApplyAnglePosition(postMoveAngleRadians);

        // Save for next frame
        lastAngleOffset = postMoveAngleOffset;
    }

    // Draw from a gamma distribution using the Marsaglia-Tsang method.
    float SampleGamma(float shape, float scale)
    {
        if (shape <= 0f || scale <= 0f)
            return 0f;

        // For shape < 1, transform a draw from Gamma(shape + 1, scale).
        if (shape < 1f)
        {
            float u = Mathf.Max(Random.value, 0.0000001f);
            return SampleGamma(shape + 1f, scale) * Mathf.Pow(u, 1f / shape);
        }

        float d = shape - 1f / 3f;
        float c = 1f / Mathf.Sqrt(9f * d);

        while (true)
        {
            float x = SampleStandardNormal();
            float v = 1f + c * x;

            if (v <= 0f)
                continue;

            v = v * v * v;
            float u = Mathf.Max(Random.value, 0.0000001f);
            float x2 = x * x;

            if (u < 1f - 0.0331f * x2 * x2)
                return scale * d * v;

            if (Mathf.Log(u) < 0.5f * x2 + d * (1f - v + Mathf.Log(v)))
                return scale * d * v;
        }
    }

    // Box-Muller transform for a standard normal draw.
    float SampleStandardNormal()
    {
        float u1 = Mathf.Max(Random.value, 0.0000001f);
        float u2 = Random.value;
        return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
    }

    // Rejection-sample the gamma distribution so durations never exceed maxDuration.
    float SampleTruncatedGamma(float shape, float scale, float maxDuration)
    {
        if (maxDuration <= 0f)
            return 0f;

        for (int i = 0; i < 1000; i++)
        {
            float sample = SampleGamma(shape, scale);
            if (sample <= maxDuration)
                return sample;
        }

        // Extremely unlikely fallback if 1000 rejected draws occur.
        return maxDuration;
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
