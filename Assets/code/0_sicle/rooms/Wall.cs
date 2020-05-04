using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor(typeof(Wall))]
public class WallScriptEditor : ScriptEditor
{
    public override void OnInspectorGUI() {
        DrawDefaultScriptEditor();

        Wall wall = target as Wall;
        if ( GUILayout.Button( "Split" ) ) wall.Split();
    }
}

#endif // UNITY_EDITOR

[RequireComponent(typeof(BoxCollider2D))]
public class Wall : MonoBehaviour {
    public Facing.Direction Direction = Facing.Direction.NoDirection;

    public void Split() {
        if ( !Facing.IsCardinal( Direction ) ) {
            Debug.LogWarningFormat( "Cannot split a wall in direction {0}.", Direction );
            return;
        }

        var wallMin = this;

        var collider = GetComponent<BoxCollider2D>();
        var size = collider.size;
        var shift = 0.0f;
        size.x = size.x / 2.0f;
        shift = size.x / 2.0f;
        collider.size = size;

        var renderer = GetComponent<SpriteRenderer>();
        if ( renderer != null ) renderer.size = size;

        var wallMax = Instantiate( wallMin );
        wallMax.transform.parent = transform.parent;
        wallMax.transform.position = wallMin.transform.position;

        var baseName = name;

        if ( Facing.IsVertical( Direction ) ) {
            wallMin.name = baseName + " (West)";
            wallMin.transform.position += Vector3.left * shift;

            wallMax.name = baseName + " (East)";
            wallMax.transform.position += Vector3.right * shift;
        } else {
            wallMin.name = baseName + " (South)";
            wallMin.transform.position += Vector3.down * shift;

            wallMax.name = baseName + " (North)";
            wallMax.transform.position += Vector3.up * shift;
        }
    }
}
