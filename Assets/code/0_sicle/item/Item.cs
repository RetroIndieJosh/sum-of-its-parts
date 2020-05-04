using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Speaker))]
public class Item : MonoBehaviour {
    public string displayName = "Unknown Item";

    public Sprite displaySprite = null;

    public bool snapToGrid = false;

    [Tooltip("Whether to place the item in-world on drop")]
    public bool droppable = false;
    public float slideSpeed = 1.0f;

    public bool isPushable = false;

    [Header("Collect")]

    [SerializeField]
    private UnityEvent m_onCollect = null;

    public int Id {  get { return m_id; } }

    [SerializeField, HideInInspector]
    private int m_id = ItemDatabase.NONE;

    private bool m_isDropping = false;

    private Counter m_collectCounter = null;

    public void ChangeCounter(int a_change ) {
        if ( m_collectCounter == null ) return;
        m_collectCounter.Add( a_change );
    }

    private void OnCollisionEnter2D( Collision2D a_collision ) {
        if ( m_isDropping ) CheckDropOn( a_collision.gameObject );
        CheckCollect( a_collision.gameObject );
    }

    private void CheckCollect( GameObject a_collector ) {
        Debug.LogFormat( "{0} collected {1}", a_collector, gameObject );
        m_onCollect.Invoke();
        Destroy( gameObject );
    }

    private void CheckDropOn(GameObject a_target) {
    }

    private void Start() {
        SnapToGrid();
    }

    public void Drop(GameObject a_dropper, Vector2 a_pos) {
        m_isDropping = true;

        transform.position = a_pos;
        SnapToGrid();
        if ( droppable ) return;

        Destroy( gameObject, 0.5f );
    }

    public bool SetId(int a_id) {
        if( a_id != ItemDatabase.instance.GetId(this) ) {
            Debug.LogErrorFormat( "Item DB ID mismatch: expected {0}, got {1}", 
                ItemDatabase.instance.GetId( this ), a_id ); 
            return false;
        }

        m_id = a_id;
        return true;
    }

    private void SnapToGrid() {
        if ( !snapToGrid ) return;

        var prevPos = transform.position;

        var pos = transform.position;
        pos.x = Mathf.Floor( pos.x ) + 0.5f;
        pos.y = Mathf.Floor( pos.y ) + 0.5f;
        transform.position = pos;
    }

    private void OnDestroy() {
        if ( RoomManager.instance == null || RoomManager.instance.IsTransitioning ) return;
    }
}
