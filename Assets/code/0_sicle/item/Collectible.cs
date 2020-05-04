using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour {
    [SerializeField]
    private string m_collectibleType = "";

    [SerializeField]
    private bool m_destroyOnCollected = false;

    private void OnCollisionEnter2D( Collision2D collision ) {
        var collector = collision.gameObject.GetComponent<Collector>();
        if ( collector == null ) return;

        collector.Collect( gameObject, m_collectibleType );
        if ( m_destroyOnCollected ) Destroy( gameObject );
    }
}
