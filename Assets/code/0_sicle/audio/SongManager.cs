using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SongManager : MonoBehaviour
{
    public const int NO_SONG = -1;
    static public SongManager instance = null;

    [System.Serializable]
    public enum TransitionMode {
        Immediate,
        OnBar,
        OnBeat,
        OnLoop
    }

    public TransitionMode CurTransitionMode = TransitionMode.Immediate;

    [SerializeField]
    private AudioMixerGroup m_musicMixerGroup = null;
    public AudioMixerGroup MusicMixerGroup {  get { return m_musicMixerGroup; } }

    [SerializeField]
    private bool m_rewindBeforeTransition = false;

    [SerializeField]
    private float m_crossfadeTimeSec = 0.0f;
    public float CrossfadeTimeSec { set { m_crossfadeTimeSec = value; } }

    [SerializeField]
    private Song m_startSong = null;

    private Song m_nextSong = null;
    private Song m_curSong = null;

    private float m_transitionTimeElapsed = 0.0f;
    private bool m_isTransitioning = false;

    public void FadeOut() {
        m_curSong = null;
        GoToNextSong();
    }

    public void GoToNextSong() {
        if( m_nextSong == null ) {
            Debug.LogError( "Tried to go to null song" );
            return;
        }

        if( m_curSong != null ) m_curSong.Stop();
        m_curSong = m_nextSong;
        m_curSong.Volume = m_curSong.MaxVolume;

        m_nextSong = null;
        m_isTransitioning = false;
    }

    public void Pause() {
        if( m_isTransitioning ) {
            Debug.LogWarning( "Cannot pause while transitioning. Ignoring." );
            return;
        }

        if ( m_curSong == null ) return;
        m_curSong.Pause();
    }

    public void Resume() {
        if ( m_curSong == null ) return;
        m_curSong.Resume();
    }

    public void SetTransitionMode( string a_transitionMode ) {
        try {
            CurTransitionMode = (TransitionMode)System.Enum.Parse( typeof( TransitionMode ), a_transitionMode );
        } catch ( System.ArgumentException ) {
            Debug.LogErrorFormat( "Unknown transition mode {0} in song manager.", a_transitionMode );
            return;
        }
    }

    // for OnLoop which is a UnityEvent (no args)
    private void StartTransition( ) { StartTransition( 0 ); }

    private void StartTransition( int a_beatOrBar ) {
        if ( m_nextSong == null ) {
            Debug.LogError( "Tried to transition in SongManager but next song not set." );
            return;
        }

        Debug.LogFormat( "Start transition from {0} to {1}", m_curSong, m_nextSong );
        if ( m_curSong != null ) m_curSong.ClearEvents();

        if ( m_rewindBeforeTransition ) m_nextSong.Rewind();
        m_nextSong.Play();

        // no crossfade? play next immediately
        if ( m_crossfadeTimeSec < Mathf.Epsilon ) {
            GoToNextSong();
            return;
        }

        m_nextSong.Volume = 0.0f;
        m_transitionTimeElapsed = 0.0f;
        m_isTransitioning = true;
    }

    public void PlayImmediate(Song a_song) {
        m_nextSong = a_song;

        if ( m_rewindBeforeTransition ) m_nextSong.Rewind();
        m_nextSong.Play();

        GoToNextSong();
    }

    public void Play( Song a_song ) {
        if( a_song == null ) {
            Debug.LogError( "Tried to play null song." );
            return;
        }

        if ( m_nextSong != null ) {
            m_nextSong.ClearEvents();
            Debug.LogWarning( "Overwrote next song to play." );
        }

        if( m_curSong != null && a_song == m_curSong ) {
            Debug.LogWarning( "Tried to change from current song to itself. Ignoring." );
            return;
        }

        if( m_isTransitioning ) {
            Debug.LogWarning( "Tried to set song but still transitioning." );
            return;
        }

        Debug.Log( "Play song " + a_song.name );

        // force immediate transition if this is the first song
        if ( m_curSong == null ) {
            PlayImmediate( a_song );
            return;
        }

        m_nextSong = a_song;
        switch ( CurTransitionMode ) {
            case TransitionMode.Immediate:
                StartTransition();
                return;
            case TransitionMode.OnBar:
                m_curSong.OnBar.AddListener( StartTransition );
                return;
            case TransitionMode.OnBeat:
                m_curSong.OnBeat.AddListener( StartTransition );
                return;
            case TransitionMode.OnLoop:
                m_curSong.OnLoop.AddListener( StartTransition );
                return;
        }
    }

    public void Stop() {
        if ( m_curSong == null ) return;

        m_curSong.ClearEvents();
        m_curSong.Silence();
    }

    private void Awake() {
        if ( instance != null ) {
            Debug.LogErrorFormat( "{0} is a duplicate Song Manager. Destroying.", gameObject );
            Destroy( this );
            return;
        }

        instance = this;
    }

    private void Update() {
        if ( m_curSong == null && m_startSong != null ) Play( m_startSong );

        if ( !m_isTransitioning ) return;

        m_transitionTimeElapsed += Time.deltaTime;
        var t = m_transitionTimeElapsed / m_crossfadeTimeSec;
        t = 1f - Mathf.Cos( t * Mathf.PI * 0.5f );
        if ( m_curSong != null ) m_curSong.Volume = Mathf.Lerp( m_curSong.MaxVolume, 0.0f, t );
        if ( m_nextSong != null ) m_nextSong.Volume = Mathf.Lerp( 0.0f, m_nextSong.MaxVolume, t );

        if( m_transitionTimeElapsed >= m_crossfadeTimeSec) GoToNextSong();
    }
}
