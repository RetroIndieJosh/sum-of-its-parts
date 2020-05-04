using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(AiMover))]
public class AiMoverEditor : ScriptEditor
{
    public override void OnInspectorGUI() {
        var mode = ( target as AiMover ).CurAiMode;
        if ( mode == AiMover.AiMode.Flee || mode == AiMover.AiMode.Seek ) HideProperty( "m_minDistance" );
        else ShowProperty( "m_minDistance" );
        DrawDefaultScriptEditor();
    }
}
#endif // UNITY_EDITOR

[System.Serializable]
public enum AiTargetType
{
    GameObject,
    LayerMask,
    Player
}

public class AiMover : MonoBehaviour
{
    [System.Serializable]
    public enum AiMode
    {
        Avoid,
        Flee,
        Follow,
        Seek
    }

    [SerializeField]
    private Mover m_mover = null;

    [SerializeField]
    public AiMode CurAiMode = AiMode.Avoid;

    [SerializeField]
    private AiTargetType m_targetType = AiTargetType.GameObject;

    public GameObject Target;

    [SerializeField]
    private LayerMask m_targetLayerMask = 0;

    [SerializeField]
    private float m_minDistance = 1.0f;

    public void OnSwitchedToAiBehaviour() {
        Debug.LogFormat( "Activate {0} AI Behavior", name );
    }

    public void OnSwitchedFromAiBehaviour() {
        Debug.LogFormat( "Deactivate {0} AI Behavior", name );
    }

    private void Awake() {
        if ( m_targetType == AiTargetType.Player ) Target = GameObject.FindGameObjectWithTag( "Player" );

        if( m_mover == null ) m_mover = GetComponent<Mover>();
        if( m_mover == null ) {
            Debug.LogErrorFormat( "Ai Mover in {0} requires Mover to be attached or set. Deactivating.", name );
            this.enabled = false;
            return;
        }
    }

    private void Update() {
        if ( InputManager.instance.IsPaused ) return;

        if ( m_targetType == AiTargetType.LayerMask ) Target = GetTargetForLayerMask();

        if ( Target == null ) return;

        switch ( CurAiMode ) {
            case AiMode.Avoid: Avoid(); break;
            case AiMode.Flee: Flee(); break;
            case AiMode.Follow: Follow(); break;
            case AiMode.Seek: Seek(); break;
        }
    }

    private void Avoid() {
        var diff = Target.transform.position - transform.position;
        if ( diff.magnitude <= m_minDistance ) Flee();
        else m_mover.Stop();
    }

    private void Flee() {
        var diff = Target.transform.position - transform.position;

        m_mover.SetDirection( -diff.normalized );
    }

    private void Follow() {
        var diff = Target.transform.position - transform.position;
        if ( diff.magnitude >= m_minDistance ) Seek();
        else m_mover.Stop();
    }

    private void Seek() {
        var diff = Target.transform.position - transform.position;
        m_mover.SetDirection( diff.normalized );
    }

    private GameObject GetTargetForLayerMask() {
        GameObject target = null;
        var closestDistance = Mathf.Infinity;
        foreach ( var collider in FindObjectsOfType<Collider2D>() ) {
            if ( !m_targetLayerMask.ContainsLayer( collider.gameObject.layer ) ) continue;

            var distance = ( collider.transform.position - transform.position ).magnitude;
            if ( distance < closestDistance ) {
                target = collider.gameObject;
                closestDistance = distance;
            }
        }
        return target;
    }
}
