using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveController : MonoBehaviour {
    [SerializeField]
    private Mover m_mover = null;

    private Vector2 m_moveAxis = Vector2.zero;

    public void Stop() {
        m_moveAxis = Vector2.zero;
        m_mover.Stop();
    }

    private void Start() {
        if ( m_mover == null ) m_mover = GetComponent<Mover>();
        if( m_mover == null ) {
            Debug.LogErrorFormat( "Mover must be set or in {0}. Destroying.", name );
            Destroy( gameObject );
            return;
        }

        if ( !InputManager.instance.UseGamepad ) {
            InputManager.instance.GetPage().AddListenerHeld( KeyCode.DownArrow, MoveDown, true );
            InputManager.instance.GetPage().AddListenerHeld( KeyCode.LeftArrow, MoveLeft, true );
            InputManager.instance.GetPage().AddListenerHeld( KeyCode.RightArrow, MoveRight, true );
            InputManager.instance.GetPage().AddListenerHeld( KeyCode.UpArrow, MoveUp, true );

            InputManager.instance.GetPage().AddListenerUp( KeyCode.DownArrow, StopVertical, true );
            InputManager.instance.GetPage().AddListenerUp( KeyCode.LeftArrow, StopHorizontal, true );
            InputManager.instance.GetPage().AddListenerUp( KeyCode.RightArrow, StopHorizontal, true );
            InputManager.instance.GetPage().AddListenerUp( KeyCode.UpArrow, StopVertical, true );
        }
    }

    void Update () {
        if ( InputManager.instance.IsPaused ) return;

        if ( InputManager.instance.UseGamepad ) {
            var x = InputManager.instance.GetAxis( "LeftHorizontal" );
            var y = -InputManager.instance.GetAxis( "LeftVertical" );
            m_moveAxis = new Vector2( x, y );
        }

        m_mover.SetDirection( m_moveAxis );
	}

    private void MoveHorizontal(float a_axis ) { m_moveAxis = new Vector2( a_axis, 0.0f ); }
    private void MoveVertical(float a_axis ) { m_moveAxis = new Vector2( 0.0f, a_axis ); }

    private void MoveDown() { m_moveAxis = new Vector2( m_moveAxis.x, -1.0f ); }
    private void MoveLeft() { m_moveAxis = new Vector2( -1.0f, m_moveAxis.y ); }
    private void MoveRight() { m_moveAxis = new Vector2( 1.0f, m_moveAxis.y ); }
    private void MoveUp() { m_moveAxis = new Vector2( m_moveAxis.x, 1.0f ); }

    private void StopHorizontal() { m_moveAxis = new Vector2( 0.0f, m_moveAxis.y ); }
    private void StopVertical() { m_moveAxis = new Vector2( m_moveAxis.x, 0.0f ); }
}
