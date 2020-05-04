using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pushable : MonoBehaviour {
    [SerializeField]
    private float m_pushDelaySec = 0.5f;

    [SerializeField]
    private float m_slideSpeed = 0.1f;

    private float m_pushTimeSec = 0.0f;
    private Vector2 m_pushVec = Vector2.zero;

    private bool m_isSliding = false;

    private void OnCollisionExit2D( Collision2D collision ) {
        m_pushTimeSec = 0.0f;
    }

    private void OnCollisionStay2D( Collision2D collision ) {
        if ( m_isSliding ) return;

        var hit = Physics2D.Raycast( transform.position, m_pushVec, 1.0f );
        if ( hit ) return;

        m_pushTimeSec += Time.deltaTime;
        if ( m_pushTimeSec > m_pushDelaySec ) StartCoroutine( StartSlide() );
    }

    private void OnCollisionEnter2D( Collision2D collision ) {
        m_pushVec = collision.gameObject.GetComponent<Facing>().DirectionVector;
    }

    private IEnumerator StartSlide() {
        if ( m_isSliding ) yield break;

        m_isSliding = true;
        var moveAmount = 0.0f;

        while( moveAmount < 1.0f ) {
            moveAmount += m_slideSpeed * Time.deltaTime;
            transform.position += (Vector3)m_pushVec * m_slideSpeed * Time.deltaTime;
            yield return null;
        }

        m_isSliding = false;
    }
}
