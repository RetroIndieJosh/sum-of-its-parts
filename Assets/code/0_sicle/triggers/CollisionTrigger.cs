using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class CollisionTrigger : MonoBehaviour {
    [SerializeField]
    private UnityEvent OnTrigger = new UnityEvent();

    [SerializeField]
    private UnityEvent OnUntrigger = new UnityEvent();

    [SerializeField]
    private bool m_triggerOnce = false;

    [SerializeField]
    private float m_triggerDelay = 0.0f;

    public void Trigger() {
        StartCoroutine( TriggerCoroutine() );
    }

    public void Untrigger() {
        OnUntrigger.Invoke();
    }

    private void OnCollisionEnter2D( Collision2D collision ) {
        if ( !enabled ) return;
        Trigger();
    }

    private void OnCollisionExit2D( Collision2D collision ) {
        if ( !enabled ) return;
        Untrigger();
    }

    private void OnTriggerEnter2D( Collider2D collision ) {
        if ( !enabled ) return;
        Trigger();
    }

    private void OnTriggerExit2D( Collider2D collision ) {
        if ( !enabled ) return;
        Untrigger();
    }

    private IEnumerator TriggerCoroutine() {
        yield return new WaitForSeconds( m_triggerDelay );

        OnTrigger.Invoke();
        if ( m_triggerOnce ) Destroy( this );
    }

    private void Update() { }
}
