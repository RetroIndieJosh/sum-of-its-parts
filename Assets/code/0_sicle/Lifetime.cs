using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour {
    [SerializeField]
    private float a_lifetime = 1.0f;

    private void Start() {
        Destroy( gameObject, a_lifetime );
    }
}
