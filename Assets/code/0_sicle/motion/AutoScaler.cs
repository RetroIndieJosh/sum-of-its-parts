using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScaler : MonoBehaviour {
    [SerializeField]
    float m_scalePerSecond = 0.0f;

    [SerializeField]
    float m_stopAtScale = 0.0f;

    private bool m_applyScale = true;

    private void Update() {
        if ( !m_applyScale ) return;
        transform.localScale += Vector3.one * m_scalePerSecond * Time.deltaTime;
        if ( transform.localScale.magnitude < m_stopAtScale ) m_applyScale = false;
    }
}
