using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotator : MonoBehaviour {
    [SerializeField]
    Rigidbody2D m_target = null;

    [SerializeField]
    float m_angularVelocity = 0.0f;

    private void Start() {
        m_target = Utility.RequireComponent( this, m_target );
        if( m_target == null ) return;
        m_target.angularVelocity = m_angularVelocity;
    }
}
