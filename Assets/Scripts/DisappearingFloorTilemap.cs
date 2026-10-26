using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Paint only breakaway tiles on this separate, rectangular tilemap.
[DisallowMultipleComponent]
[RequireComponent(typeof(Tilemap), typeof(TilemapCollider2D))]
public class DisappearingFloorTilemap : MonoBehaviour
{
    private Tilemap tilemap;
    private TilemapCollider2D tilemapCollider;
    private readonly Dictionary<Vector3Int, RemovedTile> removedTiles =
        new Dictionary<Vector3Int, RemovedTile>();
    private readonly HashSet<Vector3Int> contactedCells = new HashSet<Vector3Int>();

    private struct RemovedTile
    {
        public TileBase tile;
        public Color color;
        public Matrix4x4 transform;
        public TileFlags flags;
    }

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
    }

    private void OnEnable()
    {
        PlayerRespawnEvents.Respawned += RestoreTiles;
    }

    private void OnDisable()
    {
        PlayerRespawnEvents.Respawned -= RestoreTiles;
        RestoreTiles();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        RemoveTilesUnderPlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        RemoveTilesUnderPlayer(collision);
    }

    private void RemoveTilesUnderPlayer(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        contactedCells.Clear();
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (contact.normal.y >= -0.5f)
                continue;

            // Sample just inside the floor, away from the tile boundary.
            Vector3 point = contact.point + contact.normal * 0.01f;
            Vector3Int cell = tilemap.WorldToCell(point);
            if (tilemap.HasTile(cell))
                contactedCells.Add(cell);
        }

        // Gather contacts first; rebuilding geometry can invalidate contacts.
        foreach (Vector3Int cell in contactedCells)
        {
            if (removedTiles.ContainsKey(cell))
                continue;

            removedTiles.Add(cell, new RemovedTile
            {
                tile = tilemap.GetTile(cell),
                color = tilemap.GetColor(cell),
                transform = tilemap.GetTransformMatrix(cell),
                flags = tilemap.GetTileFlags(cell)
            });
            tilemap.SetTile(cell, null);
        }

        if (contactedCells.Count > 0)
            tilemapCollider.ProcessTilemapChanges();
    }

    private void RestoreTiles()
    {
        if (tilemap == null || removedTiles.Count == 0)
            return;

        foreach (KeyValuePair<Vector3Int, RemovedTile> entry in removedTiles)
        {
            tilemap.SetTile(entry.Key, entry.Value.tile);
            tilemap.SetTileFlags(entry.Key, TileFlags.None);
            tilemap.SetColor(entry.Key, entry.Value.color);
            tilemap.SetTransformMatrix(entry.Key, entry.Value.transform);
            tilemap.SetTileFlags(entry.Key, entry.Value.flags);
        }
        removedTiles.Clear();
        tilemapCollider.ProcessTilemapChanges();
    }
}
