using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiShooter : MonoBehaviour {
    [SerializeField]
    private AiTargetType m_targetType = AiTargetType.GameObject;

    [SerializeField]
    private GameObject m_target = null;

    [SerializeField]
    private Facing m_facing = null;

    [SerializeField]
    private Shooter m_shooter = null;

    [SerializeField]
    [Tooltip("The maximum percentage by which we will miss the target & maximum delay percentage (of shooting time)")]
    [Range(0.0f, 0.2f)]
    float m_maxFudge = 0.5f;

    float m_nextFireTime = 0.0f;

    public bool HasTarget {  get { return m_target != null; } }

    private void Awake() {
        if ( m_facing == null ) m_facing = GetComponent<Facing>();
        if( m_facing == null ) {
            Debug.LogErrorFormat( "Ai Shooter in {0} requires Facing to be attached or set. Deactivating.", name );
            this.enabled = false;
            return;
        }

        if ( m_shooter == null ) m_shooter = GetComponent<Shooter>();
        if( m_shooter == null ) {
            Debug.LogErrorFormat( "Ai Shooter in {0} requires Shooter to be attached or set. Deactivating.", name );
            this.enabled = false;
            return;
        }
    }

    private void Start() {
        if ( m_targetType == AiTargetType.Player ) m_target = GameObject.FindGameObjectWithTag( "Player" );
        else if( m_targetType == AiTargetType.LayerMask ) {
            Debug.LogErrorFormat( "AI shooter does not currently support targeting layer mask. Disabling {0}. ", 
                name );
            enabled = false;
            return;
        }

        CalculateFireTime();
    }

    private void Update() {
        if ( m_target == null ) return;

        m_nextFireTime -= Time.deltaTime;
        if ( m_nextFireTime > 0.0f ) return;

        var towardTarget = ( m_target.transform.position - transform.position ).normalized;
        var angle = Random.Range( -m_maxFudge, m_maxFudge ) * 180.0f;
        var facingVec = Quaternion.Euler( 0.0f, 0.0f, angle ) * towardTarget;

        m_shooter.AimDirection = facingVec;

        // keep trying to shoot until we succeed
        if ( m_shooter.Fire() ) {
            m_facing.DirectionVector = facingVec;
            CalculateFireTime();
        }
    }

    private void CalculateFireTime() {
        var multiplier = 1.0f + Random.Range( 0.0f, m_maxFudge );
        m_nextFireTime = m_shooter.SecPerShot * multiplier;
        Debug.Log( "Firing in " + m_nextFireTime + " seconds" );
    }
}
