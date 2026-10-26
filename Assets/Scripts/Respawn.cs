using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 respawnPoint;
    private CharacterController controller2D; // Or Rigidbody / Rigidbody2D

    void Start()
    {
        // Set initial spawn point to the game's start position
        respawnPoint = transform.position;
        controller2D = GetComponent<CharacterController>();
    }

    public void Respawn()
    {
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