using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( Room ) )]
public class RoomScriptEditor : ScriptEditor
{
    private bool m_armDestruction = false;

    public override void OnInspectorGUI() {
        DrawDefaultScriptEditor();

        Room room = target as Room;
        if ( GUILayout.Button( "Generate Walls" ) ) room.CreateAllWalls();

        EditorGUILayout.LabelField( "DANGER AREA" );
        m_armDestruction = EditorGUILayout.Toggle( "Arm Wall Destroyer", m_armDestruction );
        if ( m_armDestruction ) {
            if ( GUILayout.Button( "Destroy All Walls" ) ) room.DestroyAllWalls();
        }
    }
}

#endif // UNITY_EDITOR

[RequireComponent( typeof( RectTransform ) )]
[RequireComponent( typeof( BoxCollider2D ) )]
public class Room : MonoBehaviour
{
    [System.Serializable]
    public class Exit
    {
        [HideInInspector]
        public Facing.Direction Direction = Facing.Direction.NoDirection;

        [HideInInspector]
        public Door Door = null;

        public bool enabled = false;
        public bool initiallyOpen = false;
        public bool hasDoor = false;

        public Room linkedRegion = null;
    }

    [SerializeField]
    private RoomManager.TransitionMode m_transitionMode = RoomManager.TransitionMode.Fade;

    [SerializeField]
    private string m_targetSceneName = "";

    [SerializeField]
    private UnityEvent m_onRegionDiscovered = new UnityEvent();

    [SerializeField]
    private UnityEvent m_onRegionEntered = new UnityEvent();

    [SerializeField]
    private UnityEvent m_onRegionUnloaded = new UnityEvent();

    [SerializeField]
    private bool m_showBorderGizmo = false;

    // TODO hide in editor when show border gizmo is false
    [SerializeField]
    private Color m_borderGizmoColor = Color.green;

    [Header( "Walls" )]

    [SerializeField]
    private BoxCollider2D m_wallColliderPrefab = null;
    public BoxCollider2D WallColliderPrefab { get { return m_wallColliderPrefab; } }

    [SerializeField]
    private SpriteRenderer m_wallRendererPrefab = null;
    public SpriteRenderer WallRendererPrefab { get { return m_wallRendererPrefab; } }

    [SerializeField]
    private SpriteRenderer m_wallRendererCornerPrefab = null;
    public SpriteRenderer WallRendererCornerPrefab { get { return m_wallRendererCornerPrefab; } }

    [SerializeField]
    private float m_wallSize = 1.0f;
    public float WallSize { get { return m_wallSize; } }

    public bool Discovered { get; private set; }

    public RoomManager.TransitionMode TransitionMode { get { return m_transitionMode; } }

    public List<Door> DoorList { get { return new List<Door>( gameObject.GetComponentsInChildren<Door>() ); } }

    public Rect Rect {
        get {
            if ( m_rectTransform == null ) m_rectTransform = GetComponent<RectTransform>();
            var rect = m_rectTransform.rect;
            rect.center = transform.position;
            return rect;
        }
    }

    public float Bottom { get { return Rect.yMin; } }
    public float Left { get { return Rect.xMin; } }
    public float Right { get { return Rect.xMax; } }
    public float Top { get { return Rect.yMax; } }

    private RectTransform m_rectTransform = null;

    public Vector2 Size { get { return m_rectTransform.rect.size; } }

    public void CloseAllDoors() {
        //foreach ( var exit in DoorList ) { if ( exit != null && exit.Door != null ) exit.Door.IsOpen = false; }
    }

    public void OpenAllDoors() {
        //foreach ( var exit in DoorList ) { if ( exit != null && exit.Door != null ) exit.Door.IsOpen = true; }
    }

    [SerializeField]
    private float m_doorSize = 2.0f;

    private float DoorScale { get { return m_doorSize / Mathf.Min( Size.x, Size.y ); } }

    public Vector2 GetWallCenter( Facing.Direction a_direction, bool a_relative = false ) {
        var offset = -Facing.DirectionToVector( a_direction ) * WallSize / 2.0f;
        float attachPoint = 0.0f;
        switch ( a_direction ) {
            case Facing.Direction.East: attachPoint = Right - ( a_relative ? transform.position.x : 0.0f ); break;
            case Facing.Direction.North: attachPoint = Top - ( a_relative ? transform.position.y : 0.0f ); break;
            case Facing.Direction.South: attachPoint = Bottom - ( a_relative ? transform.position.y : 0.0f ); break;
            case Facing.Direction.West: attachPoint = Left - ( a_relative ? transform.position.x : 0.0f ); break;
        }
        if ( !Facing.IsVertical( a_direction ) ) return new Vector2( attachPoint + offset.x, 0.0f );
        return new Vector2( 0.0f, attachPoint + offset.y );
    }

    public bool IsInBounds( Vector2 a_pos ) {
        return a_pos.x > Left && a_pos.x < Right && a_pos.y > Bottom && a_pos.y < Top;
    }

    public bool IsInBounds( Rect a_rect ) {
        return a_rect.xMin > Left && a_rect.xMax < Right && a_rect.yMin > Bottom && a_rect.yMax < Top;
    }

    public void Discover() {
        m_onRegionDiscovered.Invoke();
        Discovered = true;
    }

    public void Load() {
        foreach ( Transform child in transform ) child.gameObject.SetActive( true );
        gameObject.SetNonvisualComponentEnabled( false );

        if ( !Discovered ) Discover();

        if ( !string.IsNullOrEmpty( m_targetSceneName ) ) {
            // TODO load scene
        }
    }

    public void OnEntered() {
        gameObject.SetNonvisualComponentEnabled( true );
        m_onRegionEntered.Invoke();
    }

    public void Unload() {
        foreach ( Transform child in transform ) child.gameObject.SetActive( false );
        if ( !string.IsNullOrEmpty( m_targetSceneName ) ) {
            // TODO unload scene
        }

        m_onRegionUnloaded.Invoke();
    }

    private void Awake() {
        m_rectTransform = GetComponent<RectTransform>();
        Discovered = false;
    }

    private void OnTriggerEnter2D( Collider2D collision ) {
        if ( RoomManager.instance.CurRegion == this || RoomManager.instance.IsTransitioning ) return;
        if ( collision.gameObject.tag != "Player" ) return;

        // if we aren't transitioning from another region, don't bother
        if ( RoomManager.instance.CurRegion != null ) StartCoroutine( LerpToInside( collision.gameObject ) );

        RoomManager.instance.CurRegion = this;
    }

    private IEnumerator LerpToInside( GameObject a_player ) {
        var moveController = a_player.gameObject.GetComponentInChildren<MoveController>();
        if ( moveController != null ) moveController.Stop();

        var roomSideVec = (Vector2)a_player.transform.position - Rect.center;
        var enterDirection = Facing.VectorToDirectionCardinal( roomSideVec );

        var offset = Mathf.Max( 0, m_wallSize );
        var spriteRenderer = a_player.GetComponentInChildren<SpriteRenderer>();
        if ( spriteRenderer != null ) {
            if ( Facing.IsVertical( enterDirection ) )
                offset += a_player.transform.localScale.y * ( spriteRenderer.drawMode == SpriteDrawMode.Simple ? 1.0f : spriteRenderer.size.y / 2.0f );
            else offset += a_player.transform.localScale.x * ( spriteRenderer.drawMode == SpriteDrawMode.Simple ? 1.0f : spriteRenderer.size.x / 2.0f );
        }

        var x = a_player.transform.position.x;
        var y = a_player.transform.position.y;
        switch ( enterDirection ) {
            // the direction they're coming from
            case Facing.Direction.East: x = Right - offset; break;
            case Facing.Direction.North: y = Top - offset; break;
            case Facing.Direction.South: y = Bottom + offset; break;
            case Facing.Direction.West: x = Left + offset; break;
        }

        var startPos = a_player.transform.position;
        var targetPos = new Vector2( x, y );
        var timeElapsed = 0.0f;
        while ( timeElapsed < RoomManager.instance.TransitionTime ) {
            var t = timeElapsed / RoomManager.instance.TransitionTime;
            a_player.transform.position = Vector3.Lerp( startPos, targetPos, t );
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void Start() {
        var collider = GetComponent<BoxCollider2D>();
        collider.size = Size;
        collider.isTrigger = true;
    }

    private void OnDrawGizmos() {
        if ( !m_showBorderGizmo ) return;
        var rectTransform = GetComponent<RectTransform>();
        Gizmos.color = m_borderGizmoColor;
        Gizmos.DrawWireCube( rectTransform.position, rectTransform.rect.size );
    }

    [ExecuteInEditMode]
    private void CreateWallCorner( Facing.Direction a_direction ) {
        var x = GetWallCenter( Facing.GetHorizontal( a_direction ) ).x;
        var y = GetWallCenter( Facing.GetVertical( a_direction ) ).y;
        var corner = Instantiate( WallRendererCornerPrefab );
        corner.transform.position = new Vector2( x, y );

        switch ( a_direction ) {
            case Facing.Direction.Northwest: corner.transform.Rotate( Vector3.forward, 90.0f ); break;
            case Facing.Direction.Southeast: corner.transform.Rotate( Vector3.forward, -90.0f ); break;
            case Facing.Direction.Southwest: corner.transform.Rotate( Vector3.forward, 180.0f ); break;
        }

        var sr = corner.GetComponent<SpriteRenderer>();
        if ( sr.drawMode == SpriteDrawMode.Simple )
            sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = Vector2.one * WallSize;

        corner.name = a_direction + " Wall Corner";
        corner.transform.parent = m_wallContainer.transform;

        var wall = corner.gameObject.AddComponent<Wall>();
        wall.Direction = a_direction;
    }

    [ExecuteInEditMode]
    private void CreateWall( Facing.Direction a_direction ) {
        var wallCollider = Instantiate( m_wallColliderPrefab );

        var thickness = WallSize;
        var collider = wallCollider.GetComponent<BoxCollider2D>();
        collider.size = new Vector2( Size.x, Mathf.Abs( thickness ) ) + Vector2.left * 2.0f * WallSize;

        wallCollider.transform.parent = m_wallContainer.transform;
        wallCollider.name = a_direction + " Wall";
        wallCollider.transform.Rotate( Vector3.forward, Facing.DirectionToRotation( a_direction ) );

        wallCollider.transform.localPosition = GetWallCenter( a_direction, true );

        wallCollider.gameObject.isStatic = true;

        if ( m_wallRendererPrefab != null ) {
            var wallSpriteRenderer = Instantiate( m_wallRendererPrefab ).GetComponent<SpriteRenderer>();
            wallSpriteRenderer.name = a_direction + " Wall Renderer";
            wallSpriteRenderer.transform.parent = wallCollider.transform;
            wallSpriteRenderer.transform.rotation = wallCollider.transform.rotation;
            wallSpriteRenderer.gameObject.isStatic = true;
            if ( wallSpriteRenderer != null ) {
                if ( wallSpriteRenderer.drawMode == SpriteDrawMode.Simple )
                    wallSpriteRenderer.drawMode = SpriteDrawMode.Tiled;

                wallSpriteRenderer.size = collider.size;
            }
            wallSpriteRenderer.transform.position = wallCollider.transform.position;
        }

        var wall = wallCollider.gameObject.AddComponent<Wall>();
        wall.Direction = a_direction;
    }

    private GameObject m_wallContainer = null;

    [ExecuteInEditMode]
    public void CreateAllWalls() {
        if ( WallColliderPrefab == null ) return;

        m_wallContainer = new GameObject();
        m_wallContainer.name = "Walls";
        m_wallContainer.transform.parent = transform;

        CreateWall( Facing.Direction.East );
        CreateWall( Facing.Direction.North );
        CreateWall( Facing.Direction.South );
        CreateWall( Facing.Direction.West );

        if ( m_wallRendererCornerPrefab != null ) {
            CreateWallCorner( Facing.Direction.Northeast );
            CreateWallCorner( Facing.Direction.Northwest );
            CreateWallCorner( Facing.Direction.Southeast );
            CreateWallCorner( Facing.Direction.Southwest );
        }
    }

    [ExecuteInEditMode]
    public void DestroyAllWalls() {
        if ( m_wallContainer == null ) return;
        DestroyImmediate( m_wallContainer );
    }

    private Vector3Int GetEntrancePos( Transform a_entrance ) {
        return a_entrance == null ? Vector3Int.zero
            : Vector3Int.FloorToInt( a_entrance.position );
    }
}
