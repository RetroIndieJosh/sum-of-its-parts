using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EldeeGameManager : MonoBehaviour {
    [SerializeField]
    private Song m_battleSong = null;

    [SerializeField]
    private Song m_bossSong = null;

    [SerializeField]
    private Song m_exploreSong = null;

    [SerializeField]
    private Room m_bossRegion = null;

    [SerializeField]
    private float m_crossfadeTimeSec = 4.0f;

    [SerializeField]
    private float m_doorToggleDelaySec = 2.0f;

    [SerializeField]
    private Health m_boss = null;

    private bool m_inBattle = false;

    private bool m_playedInitial = false;

    public void SetTextToButtonForPartBoxes(TextMeshPro a_textMesh ) {
        if ( GameObject.FindGameObjectWithTag( "Player" ).GetComponent<EldeeAttacher>().PartCount == 0 ) return;
        a_textMesh.text = InputManager.instance.UseGamepad ? "(A)" : "[Z]";
    }

	void Start () {
        if ( InputManager.instance == null ) return;

        InputManager.instance.DefaultPage.AddListenerDown( KeyCode.Escape, InputManager.instance.TogglePause );
        InputManager.instance.PausePage.AddListenerDown( KeyCode.Escape, InputManager.instance.TogglePause );

        if ( InputManager.instance.UseGamepad ) {
            var buttonStart = InputManager.GetKeyCodeForGamepadInput( GamepadInput.ButtonStart );
            InputManager.instance.DefaultPage.AddListenerDown( buttonStart, InputManager.instance.TogglePause );
            InputManager.instance.PausePage.AddListenerDown( buttonStart, InputManager.instance.TogglePause );
        }
	}

    private void EndBattle() {
        m_inBattle = false;

        Debug.Log( "End battle" );

        if ( m_boss.IsDead ) return;

        if ( SongManager.instance != null ) {
            SongManager.instance.CurTransitionMode = SongManager.TransitionMode.OnBar;
            SongManager.instance.CrossfadeTimeSec = m_crossfadeTimeSec;
            SongManager.instance.Play( m_exploreSong );
        }

        StartCoroutine( SetDoorsOpenAfterSec( true, m_doorToggleDelaySec ) );
    }

    private void StartBattle() {
        m_inBattle = true;

        Debug.Log( "Start battle" );

        if ( SongManager.instance != null ) {
            SongManager.instance.CurTransitionMode = SongManager.TransitionMode.OnBar;
            SongManager.instance.CrossfadeTimeSec = m_crossfadeTimeSec;

            if ( RoomManager.instance.CurRegion == m_bossRegion ) SongManager.instance.Play( m_bossSong );
            else SongManager.instance.Play( m_battleSong );
        }

        StartCoroutine( SetDoorsOpenAfterSec( false, m_doorToggleDelaySec ) );
    }

    private IEnumerator SetDoorsOpenAfterSec( bool a_open, float a_seconds ) {
        yield return new WaitForSeconds( a_seconds );
        if ( a_open ) {
            if( RoomManager.instance != null ) RoomManager.instance.CurRegion.OpenAllDoors();
            if( SoundManager.instance != null ) SoundManager.instance.PlaySound( "door open" );
        } else {
            if( RoomManager.instance != null ) RoomManager.instance.CurRegion.CloseAllDoors();
            if( SoundManager.instance != null ) SoundManager.instance.PlaySound( "door close" );
        }
    }

    private void Update() {
        if ( !m_playedInitial && SongManager.instance != null ) {
            SongManager.instance.PlayImmediate( m_exploreSong );
            m_playedInitial = true;
        }

        //Debug.Log( "Enemy count: " + GameObject.FindGameObjectsWithTag( "Enemy" ).Length );
        if ( GameObject.FindGameObjectsWithTag( "Enemy" ).Length > 0 ) {
            if ( !m_inBattle ) StartBattle();
        } else if ( m_inBattle ) EndBattle();
    }
}
