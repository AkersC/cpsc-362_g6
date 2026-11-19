using UnityEngine;

// Place this on a separate floor object above a pit, not on the ground tilemap.
[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class DisappearingFloor : MonoBehaviour
{
    private BoxCollider2D floorCollider;
    private SpriteRenderer floorRenderer;
    private bool disappeared;

    private void Awake()
    {
        floorCollider = GetComponent<BoxCollider2D>();
        floorRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        PlayerRespawnEvents.Respawned += RestoreFloor;
        RestoreFloor();
    }

    private void OnDisable()
    {
        PlayerRespawnEvents.Respawned -= RestoreFloor;
        RestoreFloor();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForPlayerOnTop(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckForPlayerOnTop(collision);
    }

    private void CheckForPlayerOnTop(Collision2D collision)
    {
        if (disappeared || !collision.gameObject.CompareTag("Player"))
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            // On the floor, a contact normal points down when the player is above.
            if (collision.GetContact(i).normal.y < -0.5f)
            {
                disappeared = true;
                floorCollider.enabled = false;
                floorRenderer.enabled = false;
                return;
            }
        }
    }

    private void RestoreFloor()
    {
        disappeared = false;
        if (floorCollider != null)
            floorCollider.enabled = true;
        if (floorRenderer != null)
            floorRenderer.enabled = true;
    }
}
