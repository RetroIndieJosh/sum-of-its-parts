using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent( typeof( Collider2D ) )]
public class Damager : MonoBehaviour
{
    [System.Serializable]
    private enum DamageType
    {
        Continuous,
        OnEnter,
        OnEnterDestroy
    }

    [SerializeField]
    [Tooltip("Continuous: Damage applied every frame\n\n"
        + "Once: Apply damage each time we collide\n\n"
        + "OnceDestroy: Apply damage on collision, then destroy this"
    )]
    private DamageType m_damageType = DamageType.OnEnterDestroy;

    [SerializeField]
    [Tooltip("For Continuous, this is damage per second")]
    float m_damage = 1;

    private GameObject m_target = null;

    public UnityEvent OnDestroy = new UnityEvent();

    private void Awake() {
        if( m_damageType == DamageType.Continuous ) GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update() {
        if ( m_target == null ) return;
        ApplyDamage( m_target, Time.deltaTime );
    }

    private void OnCollisionEnter2D( Collision2D collision ) {
        if ( m_damageType == DamageType.Continuous ) return;

        HandleOnceCollision( collision.gameObject );
    }

    private void OnTriggerEnter2D( Collider2D collision ) {
        if ( m_damageType == DamageType.Continuous ) {
            m_target = collision.gameObject;
            return;
        }

        HandleOnceCollision( collision.gameObject );
    }

    private void OnTriggerExit2D( Collider2D collision ) {
        m_target = null;
    }

    private void ApplyDamage( GameObject a_target, float m_multiplier = 1.0f ) {
        var health = a_target.GetComponent<Health>();
        if ( health == null ) {
            var hitbox = a_target.GetComponent<HitBox>();
            if ( hitbox == null ) return;
            health = hitbox.TargetHealth;
            m_multiplier *= hitbox.DamageMult;
        }
        health.ApplyDamage( m_damage * m_multiplier );
    }

    private void HandleOnceCollision( GameObject a_collidedObject ) {
        ApplyDamage( a_collidedObject );
        if ( m_damageType == DamageType.OnEnterDestroy ) {
            OnDestroy.Invoke();
            Destroy( gameObject );
        }
    }
}
