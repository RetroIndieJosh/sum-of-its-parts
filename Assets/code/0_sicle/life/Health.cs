using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( Health ) )]
public class HealthScriptEditor : CounterScriptEditor
{
    public override void OnInspectorGUI() {
        var health = target as Health;
        health.CurClampMode = Counter.ClampMode.Both;

        /*
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel( "Initial Health" );
        var healthMax = EditorGUILayout.DelayedFloatField( health.Count );
        health.SetRange( 0, healthMax );
        health.Count = healthMax;
        GUILayout.EndHorizontal();
        */

        m_showCount = false;
        HideProperty( "m_counterName" );
        HideProperty( "m_clampType" );

        base.OnInspectorGUI();
    }
}
#endif //UNITY_EDITOR

public class Health : Counter
{
    [System.Serializable]
    private enum DeathType
    {
        Destroy,
        DestroyDamage,
        Respawn
    }

    [System.Serializable]
    private enum RegenerationType
    {
        None,
        Regenerate,
        Degenerate
    }

    [System.Serializable]
    private enum RespawnLocation
    {
        InPlace,
        OriginalLocation,
        SetLocation
    }

    [SerializeField]
    RegenerationType m_regenerationType = RegenerationType.None;

    [SerializeField]
    private float m_regenerateDelay = 1.0f;

    [SerializeField]
    private float m_regeneratePerSec = 0.1f;

    [SerializeField]
    private int m_lives = 1;

    [SerializeField]
    [Tooltip( "Destroy: Destroy this when health <= 0\n\n"
        + "DestroyDamage: Destroy this when health <= 0 and spawn damager"
        + "Respawn: Respawn at original position when health <= 0\n\n"
    )]
    private DeathType m_deathType = DeathType.Destroy;

    [SerializeField]
    [Tooltip( "The death type we become after running out of lives." )]
    private DeathType m_deathTypeFinal = DeathType.Destroy;

    [SerializeField]
    private float m_destroyDelay = 0.0f;

    [SerializeField]
    RespawnLocation m_respawnLocation = RespawnLocation.InPlace;

    [SerializeField]
    private Vector2 m_customSpawnPoint = Vector2.zero;

    [SerializeField]
    private Damager m_spawnOnDeathPrefab = null;

    [SerializeField]
    public UnityEvent OnDamage = new UnityEvent();

    [SerializeField]
    public UnityEvent OnDeath = new UnityEvent();

    [SerializeField]
    [Tooltip( "Triggered when we run out of lives" )]
    public UnityEvent OnDeathFinal = new UnityEvent();

    [SerializeField]
    public UnityEvent AfterRespawn = new UnityEvent();

    public bool IsInvincible = false;

    private Vector2 m_originalSpawnPoint = Vector2.zero;

    private float m_timeToRegen = 0.0f;

    private bool m_isDead = false;
    public bool IsDead { get { return m_isDead; } }

    private Timer m_timer = null;

    public void AddLife( int a_life ) {
        m_lives = Mathf.Max( 0, m_lives + a_life );
    }

    public void ApplyDamage( float a_damage ) {
        if ( m_isDead || IsInvincible ) return;

        Add( -a_damage );
        OnDamage.Invoke();
        if ( Count <= Mathf.Epsilon ) Die();
        if ( m_regenerationType == RegenerationType.Regenerate )
            m_timeToRegen = m_regenerateDelay;
    }

    public void InvincibleForSeconds(float a_seconds ) {
        var palette = GetComponentInChildren<Palette>();
        if( palette != null ) palette.FlashWhite( 0.05f, a_seconds );
        StartCoroutine( InvincibleCoroutine( a_seconds ) );
    }

    public IEnumerator InvincibleCoroutine(float a_seconds) {
        IsInvincible = true;
        yield return new WaitForSeconds( a_seconds );
        IsInvincible = false;
    }

    protected override void Awake() {
        CurClampMode = Counter.ClampMode.Both;

        base.Awake();
    }

    override protected void Start() {
        ResetToMaximum();

        m_originalSpawnPoint = transform.position;
        if ( m_regenerationType == RegenerationType.Degenerate )
            m_timeToRegen = m_regenerateDelay;

        // set up timer to delay after death
        var go = new GameObject() { name = name + " Death Delay Timer" };
        m_timer = go.AddComponent<Timer>();
        m_timer.MaxMilliseconds = Mathf.FloorToInt( m_destroyDelay * 1000.0f );
        m_timer.OnEnd.AddListener( FinishDeath );

        var damager = GetComponent<Damager>();
        if ( damager != null ) damager.OnDestroy.AddListener( () => { Die(); } );

        if ( m_spawnOnDeathPrefab != null )
            OnDeath.AddListener( () => { GameObject.Instantiate( m_spawnOnDeathPrefab ); } );

        base.Start();
    }

    override protected void Update() {
        base.Update();

        if ( m_regenerationType == RegenerationType.None ) return;

        m_timeToRegen -= Time.deltaTime;
        if ( m_timeToRegen > 0.0f ) return;

        var deltaHealth = m_regeneratePerSec * Time.deltaTime;
        if ( m_regenerationType == RegenerationType.Degenerate ) deltaHealth = -deltaHealth;
        Add( deltaHealth );
    }

    private void Die() {
        OnDeath.Invoke();

        if ( m_deathType == DeathType.Respawn ) gameObject.SetActive( false );

        m_timer.StartTimer();
    }

    public void FinishDeath() {
        AddLife( -1 );

        if ( m_lives <= 0 ) {
            gameObject.SetActive( true );
            m_deathType = m_deathTypeFinal;
            OnDeathFinal.Invoke();

            if ( m_deathTypeFinal == DeathType.Respawn ) m_lives = 1;
            m_isDead = true;
        }

        switch ( m_deathType ) {
            case DeathType.Destroy:
                if ( m_lives > 0 ) return;
                Destroy( gameObject );
                return;
            case DeathType.DestroyDamage:
                if ( m_lives > 0 ) return;
                Destroy( gameObject );
                return;
            case DeathType.Respawn:
                gameObject.SetActive( true );

                if ( m_respawnLocation == RespawnLocation.OriginalLocation )
                    transform.position = m_originalSpawnPoint;
                else if ( m_respawnLocation == RespawnLocation.SetLocation )
                    transform.position = m_customSpawnPoint;

                ResetToMaximum();
                AfterRespawn.Invoke();
                return;
        }
    }
}
