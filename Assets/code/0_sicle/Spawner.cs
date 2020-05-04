using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spawn : MonoBehaviour {
    [SerializeField]
    private GameObject m_prefab = null;

    [SerializeField]
    private int m_maxAtOnce = 3;

    [SerializeField]
    private int m_maxSpawn = 10;

    [SerializeField]
    private float m_secBetweenSpawns = 0.5f;

    [SerializeField]
    private UnityEvent m_onLastKilled = new UnityEvent();

    private float m_timeSinceLastSpawn = 0.0f;
    private int m_spawnCount = 0;
    private int m_totalSpawnCount = 0;

    public void ResetSpawner() {
        enabled = true;
        m_spawnCount = 0;
        m_totalSpawnCount = 0;

        if( m_prefab == null ) {
            Debug.LogErrorFormat( "Spawn in {0} has no prefab. Disabling.", name );
            enabled = false;
            return;
        }
    }

    public void SpawnNext() {
        var spawn = Instantiate( m_prefab, transform.position, Quaternion.identity );
        spawn.transform.parent = transform;

        var health = spawn.GetComponent<Health>();
        if( health != null ) health.OnDeath.AddListener( SpawnDied );

        ++m_spawnCount;
        ++m_totalSpawnCount;
        if ( m_totalSpawnCount >= m_maxSpawn ) enabled = false;

        m_timeSinceLastSpawn = 0.0f;
    }

    private void Awake() {
        ResetSpawner();
        m_timeSinceLastSpawn = Mathf.Infinity;
    }

    private void Update() {
        if ( m_spawnCount >= m_maxAtOnce ) return;
        m_timeSinceLastSpawn += Time.deltaTime;
        if ( m_timeSinceLastSpawn > m_secBetweenSpawns ) SpawnNext();
    }

    private void SpawnDied() {
        --m_spawnCount;
        if ( m_spawnCount == 0 && m_totalSpawnCount == m_maxSpawn ) m_onLastKilled.Invoke();
    }
}
