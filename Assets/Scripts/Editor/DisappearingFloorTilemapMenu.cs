using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class DisappearingFloorTilemapMenu
{
    [MenuItem("GameObject/2D Object/Tilemap/Disappearing Floor", false, 20)]
    private static void CreateTilemap(MenuCommand command)
    {
        GameObject context = command.context as GameObject;
        Grid grid = context != null ? context.GetComponentInParent<Grid>() : null;
        if (grid == null)
            grid = Object.FindAnyObjectByType<Grid>();
        if (grid == null)
        {
            GameObject gridObject = new GameObject("Grid", typeof(Grid));
            Undo.RegisterCreatedObjectUndo(gridObject, "Create Grid");
            grid = gridObject.GetComponent<Grid>();
        }

        GameObject floor = new GameObject("DisappearingFloorTilemap");
        Undo.RegisterCreatedObjectUndo(floor, "Create Disappearing Floor Tilemap");
        Undo.SetTransformParent(floor.transform, grid.transform, "Parent Tilemap");
        floor.transform.localPosition = Vector3.zero;
        floor.transform.localRotation = Quaternion.identity;
        floor.transform.localScale = Vector3.one;
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0)
            floor.layer = groundLayer;

        Undo.AddComponent<Tilemap>(floor);
        Undo.AddComponent<TilemapRenderer>(floor);
        Rigidbody2D body = Undo.AddComponent<Rigidbody2D>(floor);
        body.bodyType = RigidbodyType2D.Static;
        TilemapCollider2D collider = Undo.AddComponent<TilemapCollider2D>(floor);
        Undo.AddComponent<CompositeCollider2D>(floor);
        collider.compositeOperation = Collider2D.CompositeOperation.Merge;
        Undo.AddComponent<DisappearingFloorTilemap>(floor);
        Selection.activeGameObject = floor;
    }
}
