using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class VersionString : MonoBehaviour {
    private void Start() { GetComponent<TextMeshPro>().text = "Version " + BuildTools.VERSION_STRING; }
}
