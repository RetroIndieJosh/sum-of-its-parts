using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour {
    [System.Serializable]
    public class SpriteAnimation
    {
        public string Name = "";
        public List<Sprite> SpriteList = new List<Sprite>();
        public Vector2 FlipVector = Vector2.zero;

        [Tooltip("Frames/second")]
        public int Framerate = 30;
        public bool Loop = true;
    }

    [SerializeField]
    private int m_initialAnimationIndex = 0;

    [SerializeField]
    private SpriteRenderer m_spriteRenderer = null;

    [SerializeField]
    private List<SpriteAnimation> m_animationList = new List<SpriteAnimation>();

    public bool IsPaused = false;

    private int m_curAnimationIndex = 0;
    private int m_curFrameIndex = 0;

    private float m_timePerFrame = 1.0f / 30.0f;
    private float m_timeElapsed = 0.0f;

    public Sprite CurFrame {
        get { return CurAnimation.SpriteList[CurFrameIndex]; }
    }

    public int CurAnimationIndex {
        set {
            if ( value >= m_animationList.Count ) {
                Debug.LogErrorFormat( "Animation index {0} out of range", value );
                return;
            }

            m_curAnimationIndex = value;
            CurFrameIndex = 0;
            m_timePerFrame = 1.0f / CurAnimation.Framerate;
            m_timeElapsed = 0.0f;
        }
    }

    public int CurFrameIndex {
        get { return m_curFrameIndex; }
        set {
            m_curFrameIndex = value;
            if ( m_curFrameIndex >= FrameCount ) {
                if ( CurAnimation.Loop ) m_curFrameIndex = 0;
                else m_curFrameIndex = FrameCount - 1;
            }
            m_spriteRenderer.sprite = CurFrame;

            if( CurAnimation.FlipVector != Vector2.zero ) transform.localScale = CurAnimation.FlipVector;
        }
    }

    public int FrameCount {
        get { return CurAnimation.SpriteList.Count; }
    }

    public void AddAnimation(SpriteAnimation a_animation) {
        foreach( var animation in m_animationList ) {
            if( animation.Name == a_animation.Name ) {
                Debug.LogErrorFormat( "Duplicate animation in {0} with name {1}", gameObject, animation.Name );
                return;
            }
        }

        m_animationList.Add( a_animation );
    }

    public bool SetAnimation(string a_name) {
        for( int i = 0; i < m_animationList.Count; ++i ) {
            if ( m_animationList[i].Name == a_name ) {
                CurAnimationIndex = i;
                return true;
            }
        }

        return false;
    }

    private SpriteAnimation CurAnimation {
        get { return m_animationList[m_curAnimationIndex]; }
    }

    private void Start() {
        if ( m_spriteRenderer == null ) m_spriteRenderer = GetComponent<SpriteRenderer>();
        if( m_spriteRenderer == null ) {
            Debug.LogErrorFormat( "No Sprite Renderer set for Sprite Animator in {0}. Destroying {0}.", name );
            Destroy( gameObject );
            return;
        }

        CurAnimationIndex = m_initialAnimationIndex;
    }

    private void Update() {
        if ( IsPaused || m_animationList.Count == 0 ) return;

        m_timeElapsed += Time.deltaTime;
        if ( m_timeElapsed <= m_timePerFrame ) return;

        m_timeElapsed = 0.0f;
        ++CurFrameIndex;
    }
}
