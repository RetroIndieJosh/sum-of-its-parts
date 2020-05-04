using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour {
    [SerializeField]
    private ShooterController m_shooterController = null;

    [SerializeField]
    private Vector2 m_attachmentOffset = Vector2.zero;

    [SerializeField]
    private float m_attachmentRotation = 0.0f;

    private bool m_isAttached = false;

    public void AttachTo(GameObject a_core ) {
        if ( m_isAttached ) return;

        m_shooterController.GetComponent<Shooter>().LinkedShooter = a_core.GetComponentInChildren<Shooter>();

        transform.parent = a_core.transform;
        transform.localPosition = m_attachmentOffset;
        transform.localRotation = Quaternion.Euler( 0.0f, 0.0f, m_attachmentRotation );

        a_core.GetComponent<EldeeAttacher>().AttachPart( this );

        m_shooterController.enabled = true;
        m_isAttached = true;
    }

    public void Detach() {
        if ( !m_isAttached ) return;

        transform.parent.GetComponent<EldeeAttacher>().RemovePart( this );

        transform.parent = null;
        m_shooterController.enabled = false;
        m_isAttached = false;
    }

    private void OnCollisionEnter2D( Collision2D collision ) {
        AttachTo( collision.gameObject );
    }

    private void Awake() {
        if ( m_shooterController == null ) m_shooterController = GetComponent<ShooterController>();
        if( m_shooterController == null ) {
            Debug.LogErrorFormat( "Part in {0} requires Shooter Controller to be attached or set. Deactivating.", 
                name );
            this.enabled = false;
            return;
        }
    }

    private void Start() {
        m_shooterController.enabled = false;
    }
}
