using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour {
    [System.Serializable]
    enum AimMode
    {
        Axes,
        Direction,
        Facing,
        Linked,
        Rotation
    }

    public Shooter LinkedShooter = null;

    [SerializeField]
    private Rigidbody2D m_bulletPrefab = null;

    [SerializeField]
    [Tooltip("Movement speed for bullet")]
    private float m_fireSpeed = 5.0f;

    [SerializeField]
    private float m_fireDelaySec = 0.0f;

    [SerializeField]
    private AimMode m_aimMode = AimMode.Facing;

    [SerializeField]
    private string m_aimAxisX = "Horizontal";

    [SerializeField]
    private string m_aimAxisY = "Vertical";

    [SerializeField]
    public Vector2 AimDirection = Vector2.zero;

    [SerializeField]
    private Facing m_facing = null;

    [SerializeField]
    private float m_fireOffset = 0.0f;

    [SerializeField]
    private float m_lifeTime = 2.0f;

    [SerializeField]
    private Counter m_ammoCounter = null;

    [SerializeField]
    private float m_shotsPerSecond = Mathf.Infinity;

    [SerializeField]
    private bool m_destroyOffscreenBullets = false;

    [SerializeField]
    [Tooltip("0 = no limit")]
    private int m_bulletsOnScreenMax = 0;

    [Header( "Charging" )]

    public bool IsChargeable = false;

    [SerializeField]
    private float m_chargeTimeMaxSec = 10.0f;

    [Tooltip("Sends in charge percent [0.0f - 1.0f], always 1.0f if not chargeable")]
    public FloatEvent OnFireEvent = null;

    public float ChargePercent {
        get {
            if ( !IsChargeable ) return 1.0f;
            return m_chargeTimeSec / m_chargeTimeMaxSec;
        }
    }
    public bool IsFiring { get; private set; }

    private float m_secSinceLastShot = Mathf.Infinity;
    private float m_secPerShot = 0.0f;
    public float SecPerShot {  get { return m_secPerShot; } }
    private float m_secBeforeStop = 0.0f;

    private float m_chargeTimeSec = 0.0f;

    private int m_bulletsOnScreenCount = 0;

    public bool LockFireDirection = false;
    private Vector2 m_fireDirection = Vector2.zero;

    private Vector2 FireDirection {
        get {
            if ( LockFireDirection ) return m_fireDirection;

            switch( m_aimMode) {
                case AimMode.Axes:
                    var x = Input.GetAxis( m_aimAxisX );
                    var y = Input.GetAxis( m_aimAxisY );
                    m_fireDirection = new Vector2( x, y ).normalized;
                    break;
                case AimMode.Direction:
                    m_fireDirection = AimDirection.normalized;
                    break;
                case AimMode.Facing:
                    m_fireDirection = m_facing.DirectionVector.normalized;
                    break;
                case AimMode.Linked:
                    if ( LinkedShooter == null ) m_fireDirection = Vector2.zero;
                    else m_fireDirection = LinkedShooter.FireDirection;
                    break;
                case AimMode.Rotation:
                    m_fireDirection = Quaternion.Euler( transform.eulerAngles ) * Vector3.up;
                    Debug.Log( "Fire direction: " + m_fireDirection );
                    break;
            }

            return m_fireDirection;
        }
    }

    private Vector2 Velocity { get { return FireDirection * m_fireSpeed; } }

    public void Charge(float a_chargeTimeSec ) {
        IsCharging = true;
        m_chargeTimeSec = Mathf.Min( m_chargeTimeSec + a_chargeTimeSec, m_chargeTimeMaxSec );
    }

    public void ChargeReset() {
        m_chargeTimeSec = 0.0f;
    }

    public void ChargeToMax() {
        ChargeFor( m_chargeTimeMaxSec );
    }

    public void ChargeFor(float a_sec ) {
        if ( IsCharging ) return;
        StartCoroutine( ChargeForCoroutine( a_sec ) );
    }

    public bool IsCharging { get; private set; }

    private IEnumerator ChargeForCoroutine(float a_sec ) {
        var timeElapsed = 0.0f;
        while( timeElapsed < a_sec ) {
            timeElapsed += Time.deltaTime;
            Charge( Time.deltaTime );
            yield return null;
        }
        Fire();
    }

    public bool Fire() {
        if ( m_bulletPrefab == null ) return false;
        if ( m_secSinceLastShot < m_secPerShot ) return false;
        if( m_bulletsOnScreenMax > 0 && m_bulletsOnScreenCount >= m_bulletsOnScreenMax ) return false;
        if ( m_ammoCounter != null && m_ammoCounter.Count == 0 ) return false;

        StartCoroutine( ContinueFireAfterSec( m_fireDelaySec ) );
        return true;
    }

    private IEnumerator ContinueFireAfterSec(float a_seconds ) {
        var timeElapsed = 0.0f;
        while( timeElapsed < a_seconds ) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        ContinueFire();
    }

    private void ContinueFire() {
        IsCharging = false;

        OnFireEvent.Invoke( ChargePercent );

        ChargeReset();

        // wait a frame for OnFireEvents to complete before instantiating bullet
        StartCoroutine( CompleteFireAtNextFrame() );
    }

    private void CompleteFire() {
        if ( m_ammoCounter != null ) m_ammoCounter.Decrement();

        var bullet = Instantiate( m_bulletPrefab, transform.position + (Vector3)Velocity.normalized * m_fireOffset, 
            Quaternion.identity );
        bullet.name = name + " bullet";
        var body = bullet.GetComponent<Rigidbody2D>();
        body.velocity = Velocity;

        if( m_destroyOffscreenBullets ) {
            var offscreen = bullet.gameObject.AddComponent<OffScreenTrigger>();
            offscreen.OnExitScreen.AddListener( bullet.gameObject.DestroySelf );
        }

        if ( m_bulletsOnScreenMax > 0 ) {
            ++m_bulletsOnScreenCount;

            var offscreen = bullet.gameObject.GetComponent<OffScreenTrigger>();
            if( offscreen == null ) offscreen = bullet.gameObject.AddComponent<OffScreenTrigger>();
            offscreen.OnExitScreen.AddListener( () => { --m_bulletsOnScreenCount; } );

            var onDestroyed = bullet.gameObject.AddComponent<DestroyedTrigger>();
            onDestroyed.OnDestroyed.AddListener( () => { --m_bulletsOnScreenCount; } );

        } else Destroy( bullet.gameObject, m_lifeTime );

        m_secSinceLastShot = 0.0f;
    }

    private IEnumerator CompleteFireAtNextFrame() {
        yield return new WaitForEndOfFrame();
        CompleteFire();
    }

    public void FireForSec(float a_seconds ) {
        StartFire();
        m_secBeforeStop = a_seconds;
    }

    public void StartFire() {
        IsFiring = true;
    }

    public void StopFire() {
        IsFiring = false;
    }

    private void Awake() {
        if( m_facing == null && m_aimMode == AimMode.Facing ) {
            m_facing = GetComponent<Facing>();
            if( m_facing == null ) {
                Debug.LogErrorFormat( "{0} requires Facing or link to Facing. Disabling.", name );
                enabled = false;
                return;
            }
        }

        IsFiring = false;
        m_secPerShot = 1.0f / m_shotsPerSecond;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;

        var aimMode = m_aimMode;
        if ( aimMode == AimMode.Linked ) {
            if ( LinkedShooter == null ) return;
            aimMode = LinkedShooter.m_aimMode;
        }

        switch ( aimMode ) {
            case AimMode.Axes:
                Gizmos.DrawWireSphere( transform.position + Vector3.left * 0.1f, 0.2f );
                Gizmos.DrawWireSphere( transform.position + Vector3.right * 0.1f, 0.2f );
                break;
            case AimMode.Direction:
                Utility.GizmoArrow( transform.position + (Vector3)Velocity.normalized * m_fireOffset, Velocity.normalized );
                break;
            case AimMode.Facing:
                Gizmos.DrawWireSphere( transform.position, 0.3f );
                break;
        }
    }

    private void Update() {
        m_secSinceLastShot += Time.deltaTime;
        if ( m_secBeforeStop > 0.0f ) {
            m_secBeforeStop -= Time.deltaTime;
            if ( m_secBeforeStop <= 0.0f ) StopFire();
        }

        if ( IsFiring ) Fire();
    }

    private void OnDestroy() {
        return;
    }
}
