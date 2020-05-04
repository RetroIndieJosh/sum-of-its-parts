using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MatchCameraRectangle : MonoBehaviour {
    private void Start() {
        if ( CameraManager.instance == null ) return;

        GetComponent<RectTransform>().sizeDelta = CameraManager.instance.Rectangle.size * 2.0f;
    }
}
