using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Knockback : MonoBehaviour {
    [SerializeField]
    private float m_knockbackForce = 1.0f;

    private void OnCollisionEnter2D( Collision2D collision ) {
        HandleKnockback( collision.gameObject );
    }

    private void OnTriggerEnter2D( Collider2D collision ) {
        HandleKnockback( collision.gameObject );
    }

    private void HandleKnockback(GameObject a_collider) {
        var direction = transform.position - a_collider.transform.position;
        GetComponent<Rigidbody2D>().AddForce( direction * m_knockbackForce );
    }
}
