using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO sync collisions to this
[ExecuteInEditMode]
public class PixelPerfect : MonoBehaviour {
    const float PIXEL_SIZE = 1.0f / 16.0f;

	void LateUpdate () {
        if ( transform.parent == null ) return;

        var pos = transform.parent.position;
        pos.x = Mathf.Round( pos.x / PIXEL_SIZE ) * PIXEL_SIZE;
        pos.y = Mathf.Round( pos.y / PIXEL_SIZE ) * PIXEL_SIZE;
        transform.position = pos;
	}
}
