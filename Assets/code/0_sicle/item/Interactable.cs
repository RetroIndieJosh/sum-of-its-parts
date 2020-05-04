using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    public GoEvent OnInteract = null;

    public void Interact() {
        OnInteract.Invoke( gameObject );
    }
}
