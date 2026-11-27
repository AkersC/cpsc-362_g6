using UnityEngine;

// Flips the platform upside down on a timer, so the spikes underneath end up on top.
// Put the platform and the spikes as children of this object; this script rotates the parent.
public class FlipPlatform : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds between flips.")]
    [SerializeField] private float flipInterval = 3f;
    [Tooltip("Extra wait before the first flip. Use different values to put platforms out of sync.")]
    [SerializeField] private float startDelay = 0f;

    [Header("Warning")]
    [Tooltip("How long the platform shakes before it flips.")]
    [SerializeField] private float warningTime = 0.5f;
    [SerializeField] private float shakeAmount = 0.05f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private float timer;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        PlayerRespawnEvents.Respawned += ResetPlatform;
        ResetPlatform();
    }

    private void OnDisable()
    {
        PlayerRespawnEvents.Respawned -= ResetPlatform;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        // Shake just before flipping so the player gets a warning
        if (timer <= warningTime)
        {
            transform.position = startPosition + (Vector3)(Random.insideUnitCircle * shakeAmount);
        }

        if (timer <= 0f)
        {
            transform.position = startPosition;
            transform.Rotate(0f, 0f, 180f);
            timer = flipInterval;
        }
    }

    // Every attempt starts with the same timing, so the level can be learned
    private void ResetPlatform()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        timer = flipInterval + startDelay;
    }
}
