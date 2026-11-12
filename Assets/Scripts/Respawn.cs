using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPoint;
    private CharacterController controller2D; // Or Rigidbody / Rigidbody2D
    [Header("Audio")]
    public AudioClip DeathSound;
    private AudioSource audioSource;
    [SerializeField] private string trapTag = "Trap";

    void Start()
    {
        // Set initial spawn point to the game's start position
        respawnPoint = transform.position;
        controller2D = GetComponent<CharacterController>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(trapTag))
        {
            Respawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(trapTag))
        {
            Respawn();
        }
    }


    public void Respawn()
    {
        if (DeathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(DeathSound);
        }

        // If using a CharacterController, you must disable it temporarily to teleport
        if (controller2D != null) controller2D.enabled = false;

        // Move player to the spawn point
        transform.position = respawnPoint;

        if (controller2D != null) controller2D.enabled = true;
        PlayerRespawnEvents.NotifyRespawned();
    }

    public void UpdateCheckpoint(Vector3 newPosition)
    {
        respawnPoint = newPosition;
    }
}