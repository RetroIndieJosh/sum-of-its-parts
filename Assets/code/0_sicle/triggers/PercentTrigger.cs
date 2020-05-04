using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PercentTrigger : MonoBehaviour {
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_triggerPercent = 1.0f;

    [SerializeField]
    private UnityEvent m_onTriggered = new UnityEvent();

    [SerializeField]
    private bool m_triggerOnce = false;

    [SerializeField]
    private Counter m_counter = null;

    private void Start() {
        if ( m_counter == null ) m_counter = GetComponent<Counter>();
        if( m_counter == null ) {
            Debug.LogErrorFormat( "Percent trigger in {0} has no linked or linkable Counter. Disabling.", name );
            enabled = false;
            return;
        }
    }

    private void Update() {
        if ( m_counter.Percent < m_triggerPercent ) return;
        m_onTriggered.Invoke();
        if ( m_triggerOnce ) Destroy( this );
    }
}
