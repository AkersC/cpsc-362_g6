using System;

// Shared notification after a player has been moved back to a spawn point.
public static class PlayerRespawnEvents
{
    public static event Action Respawned;

    public static void NotifyRespawned()
    {
        Respawned?.Invoke();
    }
}
