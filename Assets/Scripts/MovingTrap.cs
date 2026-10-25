using UnityEngine;

// Moves a trap back and forth between its starting position and an offset.
// The respawn-on-touch behaviour comes from the Trap component, which Unity
// adds automatically because of RequireComponent.
[RequireComponent(typeof(Trap))]
public class MovingTrap : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("How far the trap travels from its start position (e.g. X = 3 moves 3 units right).")]
    [SerializeField] private Vector2 travelOffset = new Vector2(3f, 0f);

    [Header("Timing")]
    [SerializeField] private float speed = 2f;
    [Tooltip("Seconds to wait at each end before turning around.")]
    [SerializeField] private float pauseAtEnds = 0.5f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 target;
    private float pauseTimer;

    private void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + (Vector3)travelOffset;
        target = endPosition;
    }

    private void Update()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        // Reached the end of the path: pause, then head back the other way
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            target = target == endPosition ? startPosition : endPosition;
            pauseTimer = pauseAtEnds;
        }
    }

    // Player lands on top: make them a child of the trap so they move with it
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") &&
            collision.transform.position.y > transform.position.y)
        {
            collision.transform.SetParent(transform);
        }
    }

    // Player jumps or walks off: stop moving them with the trap
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }

    // Shows the trap's path in the Scene view so it's easy to line up with the level
    private void OnDrawGizmosSelected()
    {
        Vector3 from = Application.isPlaying ? startPosition : transform.position;
        Vector3 to = from + (Vector3)travelOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(to, 0.2f);
    }
}
