using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScroll : MonoBehaviour
{
    [SerializeField]
    private Facing.Direction m_scrollDirection = Facing.Direction.South;

    [SerializeField]
    private float m_speed = 10.0f;
    
    [SerializeField]
    int m_segmentCount = 2;

    [SerializeField]
    int m_overlapUnits = 0;

    [SerializeField]
    Sprite m_sprite = null;

    [SerializeField]
    private SpriteDrawMode m_drawMode = SpriteDrawMode.Simple;

    [SerializeField]
    private Vector2 m_size = Vector2.one;

    [SerializeField]
    Color m_color = Color.white;

    [SerializeField]
    int m_sortingOrder = 0;

    [SerializeField]
    string m_sortingLayerName = "Default";

    public Vector2 MoveVector {  get { return m_speed * Facing.DirectionToVector( m_scrollDirection ); } }

    private Rigidbody2D m_body;

    List<GameObject> m_segmentList = new List<GameObject>();

    [SerializeField]
    private float m_lerpTime = 0.0f;
    public float LerpTime { get { return m_lerpTime; } set { m_lerpTime = value; } }

    public void LerpToSpeed(float a_speed) {
        StartCoroutine( LerpVelocityCoroutine( a_speed ) );
    }

    private IEnumerator LerpVelocityCoroutine(float a_targetSpeed) {
        var originalSpeed = m_speed;
        Debug.LogFormat( "Lerping from {0} to {1}", originalSpeed, a_targetSpeed );

        var timeElapsed = 0.0f;
        while( timeElapsed < m_lerpTime ) {
            timeElapsed += Time.deltaTime;

            var t = timeElapsed / m_lerpTime;
            m_speed = Mathf.Lerp( originalSpeed, a_targetSpeed, t );
            m_body.velocity = MoveVector;

            Debug.LogFormat( "{0}% = Velocity {1} ", t * 100.0f, m_body.velocity );
            yield return null;
        }
    }

    private void Awake() {
        var length = Facing.IsVertical( m_scrollDirection ) ? m_sprite.bounds.size.y * m_size.y 
            : m_sprite.bounds.size.x * m_size.x;
        var step = length / 2.0f - m_overlapUnits;
        for( int count = 0; count < m_segmentCount; ++count ) {
            var segment = new GameObject();

            var pos = transform.position - (Vector3)MoveVector.normalized * count * step;
            segment.transform.parent = transform;
            segment.transform.position = pos;

            var sr = segment.AddComponent<SpriteRenderer>();
            sr.sprite = m_sprite;
            sr.color = m_color;
            sr.sortingOrder = m_sortingOrder;
            sr.sortingLayerName = m_sortingLayerName;
            sr.drawMode = m_drawMode;
            if( m_drawMode != SpriteDrawMode.Simple ) sr.size = m_size;

            var iss = segment.AddComponent<InfiniteScrollSegment>();
            iss.SegmentCount = m_segmentCount;
            iss.OverlapUnits = m_overlapUnits;
            iss.ScrollDirection = m_scrollDirection;
            iss.SegmentSize = step;

            m_segmentList.Add( segment );
        }

        m_body = gameObject.AddComponent<Rigidbody2D>();
        m_body.gravityScale = 0.0f;
    }

    private void Start() {
        m_body.velocity = MoveVector;
    }
}
