using System.Collections;
using UnityEngine;

public class ChaseSequence : MonoBehaviour
{
    [Header("Level Objects")]
    [SerializeField] private Transform endpoint;
    [SerializeField] private SpriteRenderer spawnDoor;
    [SerializeField] private Transform spawnPlatform;
    [SerializeField] private SpriteRenderer chaseRenderer;
    [SerializeField] private Collider2D chaseCollider;
    [SerializeField] private Sprite[] runFrames;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Transform[] transportPath;

    [Header("Platform")]
    [SerializeField] private float platformLeftX = -6f;
    [SerializeField] private float surfaceY = 1f;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float emergeDuration = 0.4f;
    [SerializeField] private float growDuration = 0.6f;
    [SerializeField] private float emergeHeight = 2f;
    [SerializeField] private float chaseSize = 3.6f;
    [SerializeField] private float finalSize = 4f;
    [SerializeField] private float frameRate = 10f;
    [SerializeField] private float idleStepTime = 0.3f;

    [Header("Transport and Ending")]
    [SerializeField] private float transportSpeed = 7f;
    [SerializeField] private float finalAnimationTime = 1.2f;
    [SerializeField] private float shrinkDuration = 0.6f;

    private Transform player;
    private Rigidbody2D playerBody;
    private Collider2D playerCollider;
    private PlayerMovement playerMovement;

    private Collider2D endpointCollider;
    private SpriteRenderer endpointRenderer;
    private Collider2D platformCollider;
    private Rigidbody2D platformBody;
    private Vector3 endpointStartPosition;
    private Vector3 endpointStartScale;
    private Vector3 platformStartPosition;
    private Vector2 doorSize;

    private bool sequenceRunning;
    private bool sequenceFinished;
    private bool transporting;
    private bool transportDone;
    private bool playerOnPlatform;

    private bool runAnimation;
    private float frameTimer;
    private int frameIndex;

    private void OnEnable()
    {
        PlayerRespawnEvents.Respawned += OnPlayerRespawned;
    }

    private void OnDisable()
    {
        PlayerRespawnEvents.Respawned -= OnPlayerRespawned;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerBody = playerObject.GetComponent<Rigidbody2D>();
            playerCollider = playerObject.GetComponent<Collider2D>();
            playerMovement = playerObject.GetComponent<PlayerMovement>();
        }

        endpointCollider = endpoint.GetComponent<Collider2D>();
        endpointRenderer = endpoint.GetComponent<SpriteRenderer>();
        platformCollider = spawnPlatform.GetComponent<Collider2D>();
        platformBody = spawnPlatform.GetComponent<Rigidbody2D>();
        if (platformBody == null)
        {
            platformBody = spawnPlatform.gameObject.AddComponent<Rigidbody2D>();
        }

        platformBody.bodyType = RigidbodyType2D.Kinematic;
        platformBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        platformBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (runFrames == null || runFrames.Length == 0)
        {
            Debug.LogWarning("ChaseSequence: Run Frames is empty. Assign monochrome_tilemap_transparent_301 to 304.");
        }

        if (idleFrames == null || idleFrames.Length == 0)
        {
            Debug.LogWarning("ChaseSequence: Idle Frames is empty. Assign monochrome_tilemap_transparent_300 and 305.");
        }
        endpointStartPosition = endpoint.position;
        endpointStartScale = endpoint.localScale;
        platformStartPosition = spawnPlatform.position;
        Sprite doorSprite = endpointRenderer.sprite;
        Vector2 spriteSize = doorSprite.rect.size / doorSprite.pixelsPerUnit;
        doorSize = new Vector2(spriteSize.x * endpointStartScale.x, spriteSize.y * endpointStartScale.y);

        ResetSequence();
    }

    private void Update()
    {
        AnimateRun();

        if (sequenceRunning && !transporting && !transportDone && IsPlayerOnPlatform())
        {
            StartCoroutine(TransportPlatform());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (sequenceRunning || sequenceFinished || !other.CompareTag("Player"))
        {
            return;
        }

        StartCoroutine(RunChase());
    }

    private IEnumerator RunChase()
    {
        sequenceRunning = true;
        transportDone = false;
        endpointCollider.enabled = false;
        spawnPlatform.gameObject.SetActive(true);
        spawnDoor.enabled = false;

        chaseRenderer.enabled = true;
        chaseCollider.enabled = true;
        runAnimation = true;

        float rightEdge = endpointStartPosition.x + doorSize.x * 0.5f;
        float elapsed = 0f;
        while (elapsed < emergeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / emergeDuration);
            float width = Mathf.Lerp(0.1f, doorSize.x, t);
            SetChaseRect(rightEdge - width, width, Mathf.Lerp(0.1f, emergeHeight, t));
            yield return null;
        }

        endpointRenderer.enabled = false;

        float startLeft = rightEdge - doorSize.x;
        float left = startLeft;
        float width2 = doorSize.x;
        float height2 = emergeHeight;
        elapsed = 0f;
        while (left > platformLeftX)
        {
            elapsed += Time.deltaTime;
            left = Mathf.Max(platformLeftX, left - chaseSpeed * Time.deltaTime);
            float grow = Mathf.Clamp01(elapsed / growDuration);
            float travel = Mathf.InverseLerp(startLeft, platformLeftX, left);
            float extra = (finalSize - chaseSize) * 0.5f * travel;
            width2 = Mathf.Lerp(doorSize.x, chaseSize, grow) + extra;
            height2 = Mathf.Lerp(emergeHeight, chaseSize, grow) + extra;
            SetChaseRect(left, width2, height2);
            yield return null;
        }

        runAnimation = false;
        float startWidth = width2;
        float startHeight = height2;
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.3f);
            SetIdleFrame(0);
            SetChaseRect(platformLeftX, Mathf.Lerp(startWidth, finalSize, t), Mathf.Lerp(startHeight, finalSize, t));
            yield return null;
        }

        elapsed = 0f;
        while (!transportDone || elapsed < finalAnimationTime)
        {
            elapsed += Time.deltaTime;
            int step = Mathf.FloorToInt(elapsed / idleStepTime);
            float bounce = step % 2 == 1 ? finalSize / 16f : 0f;
            SetIdleFrame(step);
            SetChaseRect(platformLeftX, finalSize, finalSize, bounce);
            yield return null;
        }

        float shrinkStart = elapsed;
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / shrinkDuration));
            SetIdleFrame(Mathf.FloorToInt((shrinkStart + elapsed) / idleStepTime));
            SetChaseRect(platformLeftX, Mathf.Lerp(finalSize, doorSize.x, t), Mathf.Lerp(finalSize, doorSize.y, t));
            yield return null;
        }

        chaseRenderer.enabled = false;
        chaseCollider.enabled = false;

        endpoint.position = new Vector3(platformLeftX + doorSize.x * 0.5f, endpointStartPosition.y, endpointStartPosition.z);
        endpointRenderer.enabled = true;
        endpointCollider.enabled = true;

        sequenceRunning = false;
        sequenceFinished = true;
    }

    private IEnumerator TransportPlatform()
    {
        transporting = true;
        WaitForFixedUpdate waitForPhysics = new WaitForFixedUpdate();

        foreach (Transform point in transportPath)
        {
            while (((Vector2)platformBody.position - (Vector2)point.position).sqrMagnitude > 0.0001f)
            {
                yield return waitForPhysics;
                Vector2 current = platformBody.position;
                Vector2 next = Vector2.MoveTowards(current, point.position, transportSpeed * Time.fixedDeltaTime);
                playerOnPlatform = IsPlayerOnPlatform();
                platformBody.MovePosition(next);
                if (playerOnPlatform)
                {
                    playerBody.position += next - current;
                }
            }
        }

        transporting = false;
        transportDone = true;
    }

    private bool IsPlayerOnPlatform()
    {
        if (player == null || playerCollider == null || !spawnPlatform.gameObject.activeSelf)
        {
            return false;
        }

        Bounds p = playerCollider.bounds;
        Bounds b = platformCollider.bounds;
        bool onTop = p.min.y > b.max.y - 0.1f && p.min.y < b.max.y + 0.1f;
        bool overlapsX = p.max.x > b.min.x + 0.1f && p.min.x < b.max.x - 0.1f;
        return onTop && overlapsX && (transporting || playerBody.linearVelocity.y <= 0.01f);
    }

    private void SetChaseRect(float left, float width, float height, float lift = 0f)
    {
        Transform chase = chaseRenderer.transform;
        chase.position = new Vector3(left + width * 0.5f, surfaceY + lift + height * 0.5f, chase.position.z);
        chase.localScale = new Vector3(width, height, 1f);
    }

    private void AnimateRun()
    {
        if (!runAnimation || !chaseRenderer.enabled || runFrames == null || runFrames.Length == 0)
        {
            return;
        }

        frameTimer += Time.deltaTime;
        float frameTime = 1f / Mathf.Max(frameRate, 0.01f);
        while (frameTimer >= frameTime)
        {
            frameTimer -= frameTime;
            frameIndex = (frameIndex + 1) % runFrames.Length;
            chaseRenderer.sprite = runFrames[frameIndex];
        }
    }

    private void SetIdleFrame(int step)
    {
        if (idleFrames != null && idleFrames.Length > 0)
        {
            chaseRenderer.sprite = idleFrames[step % idleFrames.Length];
        }
    }

    private void OnPlayerRespawned()
    {
        StopAllCoroutines();
        ResetSequence();
    }

    private void ResetSequence()
    {
        sequenceRunning = false;
        sequenceFinished = false;
        transporting = false;
        transportDone = false;
        playerOnPlatform = false;
        runAnimation = false;

        spawnPlatform.position = platformStartPosition;
        if (platformBody != null)
        {
            platformBody.position = platformStartPosition;
        }

        spawnPlatform.gameObject.SetActive(false);
        spawnDoor.enabled = true;

        chaseRenderer.enabled = false;
        chaseCollider.enabled = false;
        frameIndex = 0;
        frameTimer = 0f;
        if (runFrames != null && runFrames.Length > 0)
        {
            chaseRenderer.sprite = runFrames[0];
        }

        endpoint.position = endpointStartPosition;
        endpoint.localScale = endpointStartScale;
        endpointRenderer.enabled = true;
        endpointCollider.enabled = false;
    }
}
