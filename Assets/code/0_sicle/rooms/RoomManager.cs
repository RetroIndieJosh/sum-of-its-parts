using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class RoomManager : MonoBehaviour {
    static public RoomManager instance = null;

    [System.Serializable]
    public enum TransitionMode
    {
        Fade,
        Scroll
    }

    [Header( "Audio" )]

    [SerializeField]
    private AudioClip m_doorCloseSound = null;
    public AudioClip DoorCloseSound {  get { return m_doorCloseSound; } }

    [SerializeField]
    private AudioClip m_doorOpenSound = null;
    public AudioClip DoorOpenSound {  get { return m_doorOpenSound; } }

    [Header( "Transition" )]

    [SerializeField]
    private Color m_fadeColor = Color.black;

    [SerializeField]
    private float m_transitionHoldPercent = 0.3f;

    [SerializeField]
    private float m_transitionTime = 1.0f;

    public float TransitionTime { get { return m_transitionTime; } }

    [SerializeField]
    private SpriteRenderer m_fadeScreenSprite = null;

    [Header( "Debug" )]

    [SerializeField]
    private bool m_useEmulatedLoadTime = false;

    [SerializeField]
    private float m_emulatedLoadTimeSec = 1.0f;

    [Header("Events")]

    public UnityEvent OnRegionLoaded = new UnityEvent();

    public bool IsTransitioning { get; private set; }

    public Room CurRegion {
        get { return m_curRegion; }
        set {
            if ( IsTransitioning ) {
                Debug.LogWarning( "Tried to load region during transition. Ignoring." );
                return;
            }

            if ( value == null ) {
                if ( m_curRegion != null ) m_curRegion.Unload();
                m_curRegion = null;
                return;
            }

            IsTransitioning = true;
            if ( m_curRegion == null ) SetRegion( value );
            else StartCoroutine( Transition( value ) );
        }
    }

    private Room m_curRegion = null;
    private void Awake() {
        if( instance != null ) {
            Destroy( gameObject );
            return;
        }

        instance = this;

        IsTransitioning = false;
    }

    private void Start() {
        bool error = false;
        var regionList = FindObjectsOfType<Room>();
        foreach( var region in regionList ) {
            foreach( var other in regionList ) {
                if ( region == other ) continue;
                if ( region.Rect.Overlaps( other.Rect ) ) {
                    Debug.LogErrorFormat( "Rectangles for regions {0} and {1} overlap. Regions cannot overlap.", 
                        region, other );
                    error = true;
                    break;
                }
            }
            if ( error ) break;
        }

        if ( error ) {
            Destroy( gameObject );
            foreach ( var region in regionList ) Destroy( region, 0.1f );
            return;
        }

        ClearRegions();

        m_fadeScreenSprite.drawMode = SpriteDrawMode.Tiled;
        m_fadeScreenSprite.size = Vector2.one * 100.0f;
    }

    private void ClearRegions() {
        // unload all but the main scene
        for( var i = 1; i < SceneManager.sceneCount; ++i)
            SceneManager.UnloadSceneAsync( SceneManager.GetSceneAt( i ) );
        foreach ( var region in FindObjectsOfType<Room>() ) region.Unload();
    }

    // immediately set the region - use this for region stuff including the initial region
    // NOTE this skips unloading previous region - use with caution!
    private void SetRegion(Room a_region) {
        StartCoroutine( FinishLoad(a_region) );
    }

    // the scene isn't fully loaded until the next frame so we have to wait a bit before triggering event
    private IEnumerator FinishLoad(Room a_newRegion ) {
        yield return new WaitForSeconds( 0.1f );
        if( m_useEmulatedLoadTime ) yield return new WaitForSeconds( m_emulatedLoadTimeSec );

        var prevRegion = CurRegion;
        m_curRegion = a_newRegion;
        CurRegion.Load();

        var initialCameraPos = Camera.main.transform.position;
        if ( m_curRegion != null ) CameraManager.instance.OnRegionChanged();
        var targetCameraPos = Camera.main.transform.position;

        if ( CurRegion.TransitionMode == TransitionMode.Scroll ) {
            var prevClamp = CameraManager.instance.ClampAtRegionEdges;
            CameraManager.instance.ClampAtRegionEdges = false;

            // slide transition
            var timeElapsed = 0.0f;
            while( timeElapsed < m_transitionTime ) {
                Camera.main.transform.position = Vector3.Lerp( initialCameraPos, targetCameraPos, 
                    timeElapsed / m_transitionTime );
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            CameraManager.instance.ClampAtRegionEdges = prevClamp;
        if ( prevRegion != null ) prevRegion.Unload();
        } else if ( CurRegion.TransitionMode == TransitionMode.Fade ) {
            if ( prevRegion != null ) prevRegion.Unload();
            yield return new WaitForSeconds( m_transitionTime * m_transitionHoldPercent - 0.1f );

            // fade in
            var timeElapsed = 0.0f;
            var fadeTime = m_transitionTime * ( 1.0f - m_transitionHoldPercent / 2 );
            while( timeElapsed < fadeTime ) {
                timeElapsed += Time.deltaTime;
                var color = m_fadeColor;
                m_fadeColor.a = Mathf.Lerp( 1.0f, 0.0f, timeElapsed / fadeTime );
                m_fadeScreenSprite.color = color;
                yield return null;
            }
        }

        CurRegion.OnEntered();
        OnRegionLoaded.Invoke();
        IsTransitioning = false;
    }

    private IEnumerator Transition( Room a_newRegion ) {
        if ( a_newRegion.TransitionMode == TransitionMode.Fade ) {

            // fade out
            var fadeTime = m_transitionTime * ( 1.0f - m_transitionHoldPercent / 2 );
            var timeElapsed = 0.0f;
            while ( timeElapsed < fadeTime ) {
                timeElapsed += Time.deltaTime;
                var color = m_fadeColor;
                m_fadeColor.a = Mathf.Lerp( 0.0f, 1.0f, timeElapsed / fadeTime );
                m_fadeScreenSprite.color = color;
                yield return null;
            }
        }

        SetRegion( a_newRegion );
    }

    public UnityEvent OnFadedOut = new UnityEvent();
    public void FadeOut( float a_seconds ) {
        StartCoroutine( FadeOutCoroutine( a_seconds ) );
    }

    private IEnumerator FadeOutCoroutine( float a_seconds ) {
        var timeElapsed = 0.0f;
        while ( timeElapsed < a_seconds ) {
            timeElapsed += Time.deltaTime;
            var color = m_fadeColor;
            m_fadeColor.a = Mathf.Lerp( 0.0f, 1.0f, timeElapsed / a_seconds );
            m_fadeScreenSprite.color = color;
            Debug.Log( "Color: " + m_fadeScreenSprite.color );
            yield return null;
        }
        OnFadedOut.Invoke();
    }
}
