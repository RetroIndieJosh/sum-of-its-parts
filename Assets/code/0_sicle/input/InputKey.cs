using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( InputKey ) )]
public class InputKeyScriptEditor : ScriptEditor
{
    public override void OnInspectorGUI() {
        var inputKey = target as InputKey;

        if ( inputKey.HasKeyDownEvent ) ShowProperty( "m_onKeyDown" );
        else HideProperty( "m_onKeyDown" );

        if ( inputKey.HasKeyHeldEvent ) ShowProperty( "m_onKeyHeld" );
        else HideProperty( "m_onKeyHeld" );

        if ( inputKey.HasKeyUpEvent ) ShowProperty( "m_onKeyUp" );
        else HideProperty( "m_onKeyUp" );

        DrawDefaultScriptEditor();
    }
}

#endif // UNITY_EDITOR

public class InputKey : MonoBehaviour {
    [SerializeField]
    private string m_inputPage = "";

    [SerializeField]
    private bool m_usableWhenPaused = false;

    [SerializeField]
    private KeyCode a_key;

    [SerializeField]
    private GamepadInput m_gamepadKey = GamepadInput.ButtonA;

    public bool HasKeyDownEvent = false;

    [SerializeField]
    private UnityEvent m_onKeyDown = new UnityEvent();

    public bool HasKeyHeldEvent = false;

    [SerializeField]
    private UnityEvent m_onKeyHeld = new UnityEvent();

    public bool HasKeyUpEvent = false;

    [SerializeField]
    private UnityEvent m_onKeyUp = new UnityEvent();

    private void Start() {
        var page = InputManager.instance.GetPage( m_inputPage );

        if ( HasKeyDownEvent ) {
            page.SetKeyDownEvent( a_key, m_onKeyDown );
            if( m_usableWhenPaused ) InputManager.instance.PausePage.SetKeyDownEvent( a_key, m_onKeyDown );
            if( InputManager.instance.UseGamepad ) {
                var buttonKey = InputManager.GetKeyCodeForGamepadInput( m_gamepadKey );
                page.SetKeyDownEvent( buttonKey, m_onKeyDown );
                if( m_usableWhenPaused ) InputManager.instance.PausePage.SetKeyDownEvent( buttonKey, m_onKeyDown );
            }
        }

        if ( HasKeyHeldEvent ) {
            page.SetKeyHeldEvent( a_key, m_onKeyHeld );
            if( m_usableWhenPaused ) InputManager.instance.PausePage.SetKeyHeldEvent( a_key, m_onKeyHeld );
            if( InputManager.instance.UseGamepad ) {
                var buttonKey = InputManager.GetKeyCodeForGamepadInput( m_gamepadKey );
                page.SetKeyHeldEvent( buttonKey, m_onKeyHeld );
                if( m_usableWhenPaused ) InputManager.instance.PausePage.SetKeyHeldEvent( buttonKey, m_onKeyHeld );
            }
        }

        if ( HasKeyUpEvent ) {
            page.SetKeyUpEvent( a_key, m_onKeyUp );
            if( m_usableWhenPaused ) InputManager.instance.PausePage.SetKeyUpEvent( a_key, m_onKeyUp );
            if( InputManager.instance.UseGamepad ) {
                var buttonKey = InputManager.GetKeyCodeForGamepadInput( m_gamepadKey );
                page.SetKeyUpEvent( buttonKey, m_onKeyUp );
                if( m_usableWhenPaused ) InputManager.instance.PausePage.SetKeyUpEvent( buttonKey, m_onKeyUp );
            }
        }
    }
}
