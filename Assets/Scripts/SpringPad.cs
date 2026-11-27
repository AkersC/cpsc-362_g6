using System.Collections;
using UnityEngine;

// Launches the player straight up when they land on top of the spring.
[RequireComponent(typeof(SpriteRenderer))]
public class SpringPad : MonoBehaviour
{
    [Header("Launch")]
    [Tooltip("Upward speed given to the player. The player's normal jump is 7.")]
    [SerializeField] private float launchForce = 14f;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite launchSprite;
    [Tooltip("How long the spring stays extended after a launch.")]
    [SerializeField] private float launchSpriteTime = 0.2f;

    [Header("Sound (optional)")]
    [SerializeField] private AudioClip launchSound;

    private SpriteRenderer spriteRenderer;
    private bool launching;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryLaunch(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryLaunch(collision);
    }

    private void TryLaunch(Collision2D collision)
    {
        if (launching || !collision.gameObject.CompareTag("Player") || collision.rigidbody == null)
        {
            return;
        }

        // Same check as the disappearing floor: the normal points down when the player is on top.
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y < -0.5f)
            {
                Rigidbody2D playerBody = collision.rigidbody;
                playerBody.linearVelocity = new Vector2(playerBody.linearVelocity.x, launchForce);
                StartCoroutine(PlayLaunch());
                return;
            }
        }
    }

    private IEnumerator PlayLaunch()
    {
        launching = true;

        if (launchSprite != null)
        {
            spriteRenderer.sprite = launchSprite;
        }

        if (launchSound != null)
        {
            AudioSource.PlayClipAtPoint(launchSound, transform.position);
        }

        yield return new WaitForSeconds(launchSpriteTime);

        if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }

        launching = false;
    }
}
