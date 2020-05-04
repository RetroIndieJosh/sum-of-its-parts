using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using KeyDict = System.Collections.Generic.Dictionary<UnityEngine.KeyCode, UnityEngine.Events.UnityEvent>;
using AxisDict = System.Collections.Generic.Dictionary<string, FloatEvent>;

[System.Serializable]
public class InputPage
{
    public string Name = "Default";

    private KeyDict m_keyDownDict = new KeyDict();
    private KeyDict m_keyHeldDict = new KeyDict();
    private KeyDict m_keyUpDict = new KeyDict();
    private AxisDict m_axisDict = new AxisDict();

    public KeyDict KeyDownDict {  get { return m_keyDownDict; } }
    public KeyDict KeyHeldDict {  get { return m_keyHeldDict; } }
    public KeyDict KeyUpDict {  get { return m_keyUpDict; } }
    public AxisDict AxisDict {  get { return m_axisDict; } }

    public InputPage(string a_name ) {
        Name = a_name;
    }

    // axis

    public void AddAxis( string a_axis ) { m_axisDict[a_axis] = new FloatEvent(); }

    public void AddAxisListener( string a_axis, UnityAction<float> a_listener, bool a_newEvent = false ) {
        if ( !m_axisDict.ContainsKey( a_axis ) || a_newEvent ) AddAxis( a_axis );
        m_axisDict[a_axis].AddListener( a_listener );
    }

    public void RemoveAxis( string a_axis ) {
        if ( !m_axisDict.ContainsKey( a_axis ) ) return;
        m_axisDict.Remove( a_axis );
    }

    // key down

    public void AddKeyDown( KeyCode a_key ) { AddKey( m_keyDownDict, a_key ); }

    public void AddListenerDown( KeyCode a_key, UnityAction a_listener, bool a_newEvent = false ) {
        if ( a_newEvent ) AddKeyDown( a_key );
        AddListener( m_keyDownDict, a_key, a_listener );
    }

    public void RemoveListenerDown(KeyCode a_key, UnityAction a_listener ) {
        RemoveListener( m_keyDownDict, a_key, a_listener );
    }

    public void RemoveKeyDown( KeyCode a_key ) { RemoveKey( m_keyDownDict, a_key ); }

    public void SetKeyDownEvent( KeyCode a_key, UnityEvent a_event = null ) {
        SetEvent( m_keyDownDict, a_key, a_event );
    }

    // key held

    public void AddKeyHeld( KeyCode a_key ) { AddKey( m_keyHeldDict, a_key ); }

    public void AddListenerHeld( KeyCode a_key, UnityAction a_listener, bool a_newEvent = false ) {
        if ( a_newEvent ) AddKeyHeld( a_key );
        AddListener( m_keyHeldDict, a_key, a_listener );
    }

    public void RemoveListenerHeld(KeyCode a_key, UnityAction a_listener ) {
        RemoveListener( m_keyHeldDict, a_key, a_listener );
    }

    public void RemoveKeyHeld( KeyCode a_key ) { RemoveKey( m_keyHeldDict, a_key ); }

    public void SetKeyHeldEvent( KeyCode a_key, UnityEvent a_event = null ) {
        SetEvent( m_keyHeldDict, a_key, a_event );
    }

    // key up

    public void AddKeyUp( KeyCode a_key ) { AddKey( m_keyUpDict, a_key ); }

    public void AddListenerUp( KeyCode a_key, UnityAction a_listener, bool a_newEvent = false ) {
        if ( a_newEvent ) AddKeyUp( a_key );
        AddListener( m_keyUpDict, a_key, a_listener );
    }

    public void RemoveListenerUp(KeyCode a_key, UnityAction a_listener ) {
        RemoveListener( m_keyUpDict, a_key, a_listener );
    }

    public void RemoveKeyUp( KeyCode a_key ) { RemoveKey( m_keyUpDict, a_key ); }

    public void SetKeyUpEvent( KeyCode a_key, UnityEvent a_event = null ) {
        SetEvent( m_keyUpDict, a_key, a_event );
    }

    // general

    private void AddKey( KeyDict a_dict, KeyCode a_key ) {
        a_dict[a_key] = new UnityEvent();
    }

    private void AddListener( KeyDict a_dict, KeyCode a_key, UnityAction a_listener ) {
        if ( !a_dict.ContainsKey( a_key ) ) AddKey( a_dict, a_key );
        a_dict[a_key].AddListener( a_listener );
    }

    private void RemoveKey( KeyDict a_dict, KeyCode a_key ) {
        if ( !a_dict.ContainsKey( a_key ) ) {
            Debug.LogWarningFormat( "Tried to remove key {0} but it's not set in {1}.", a_key, a_dict );
            return;
        }
        a_dict.Remove( a_key );
    }

    private void RemoveListener( KeyDict a_dict, KeyCode a_key, UnityAction a_listener ) {
        if ( !a_dict.ContainsKey( a_key ) ) return;
        a_dict[a_key].RemoveListener( a_listener );
    }

    private void SetEvent( KeyDict a_dict, KeyCode a_key, UnityEvent a_event ) {
        if( a_dict.ContainsKey(a_key)) Debug.LogWarningFormat( "Overwrote key {0} on page {1}", a_key, Name );
        a_dict[a_key] = a_event;
    }
}

[System.Serializable]
public enum GamepadInput
{
    ButtonA,
    ButtonB,
    ButtonX,
    ButtonY,
    ButtonBack,
    ButtonStart,
    ButtonLB,
    ButtonRB,
    ButtonLeftStick,
    ButtonRightStick,
    ButtonDpadDown,
    ButtonDpadLeft,
    ButtonDpadRight,
    ButtonDpadUp,
}

public class InputManager : MonoBehaviour
{
    static public InputManager instance = null;

    private const string DEFAULT_PAGE_NAME = "Default";
    private const string PAUSED_PAGE_NAME = "Paused";

    [SerializeField]
    private bool m_useGamepad = false;
    public bool UseGamepad { get { return m_useGamepad; } set { m_useGamepad = value; } }

    static public KeyCode GetKeyCodeForGamepadInput( GamepadInput a_input, int a_playerNum = -1 ) {
        switch( Application.platform) {
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.LinuxEditor:
                return GetKeyCodeForGamepadInputLinux( a_input, a_playerNum );
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                return GetKeyCodeForGamepadInputOsx( a_input, a_playerNum );
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return GetKeyCodeForGamepadInputWindows( a_input, a_playerNum );
            default: return KeyCode.None;
        }
    }

    static private KeyCode GetKeyCodeForGamepadInputLinux( GamepadInput a_input, int a_playerNum ) {
        switch ( a_input ) {
            case GamepadInput.ButtonA:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button0;
                    case 2: return KeyCode.Joystick2Button0;
                    case 3: return KeyCode.Joystick3Button0;
                    case 4: return KeyCode.Joystick4Button0;
                    default: return KeyCode.JoystickButton0;
                }
            case GamepadInput.ButtonB:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button1;
                    case 2: return KeyCode.Joystick2Button1;
                    case 3: return KeyCode.Joystick3Button1;
                    case 4: return KeyCode.Joystick4Button1;
                    default: return KeyCode.JoystickButton1;
                }
            case GamepadInput.ButtonX:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button2;
                    case 2: return KeyCode.Joystick2Button2;
                    case 3: return KeyCode.Joystick3Button2;
                    case 4: return KeyCode.Joystick4Button2;
                    default: return KeyCode.JoystickButton2;
                }
            case GamepadInput.ButtonY:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button3;
                    case 2: return KeyCode.Joystick2Button3;
                    case 3: return KeyCode.Joystick3Button3;
                    case 4: return KeyCode.Joystick4Button3;
                    default: return KeyCode.JoystickButton3;
                }
            case GamepadInput.ButtonBack:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button6;
                    case 2: return KeyCode.Joystick2Button6;
                    case 3: return KeyCode.Joystick3Button6;
                    case 4: return KeyCode.Joystick4Button6;
                    default: return KeyCode.JoystickButton6;
                }
            case GamepadInput.ButtonStart:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button7;
                    case 2: return KeyCode.Joystick2Button7;
                    case 3: return KeyCode.Joystick3Button7;
                    case 4: return KeyCode.Joystick4Button7;
                    default: return KeyCode.JoystickButton7;
                }
            case GamepadInput.ButtonLB:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button4;
                    case 2: return KeyCode.Joystick2Button4;
                    case 3: return KeyCode.Joystick3Button4;
                    case 4: return KeyCode.Joystick4Button4;
                    default: return KeyCode.JoystickButton4;
                }
            case GamepadInput.ButtonRB:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button5;
                    case 2: return KeyCode.Joystick2Button5;
                    case 3: return KeyCode.Joystick3Button5;
                    case 4: return KeyCode.Joystick4Button5;
                    default: return KeyCode.JoystickButton5;
                }
            case GamepadInput.ButtonLeftStick:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button9;
                    case 2: return KeyCode.Joystick2Button9;
                    case 3: return KeyCode.Joystick3Button9;
                    case 4: return KeyCode.Joystick4Button9;
                    default: return KeyCode.JoystickButton9;
                }
            case GamepadInput.ButtonRightStick:
                switch ( a_playerNum ) {
                    case 1: return KeyCode.Joystick1Button10;
                    case 2: return KeyCode.Joystick2Button10;
                    case 3: return KeyCode.Joystick3Button10;
                    case 4: return KeyCode.Joystick4Button10;
                    default: return KeyCode.JoystickButton10;
                }
            default: return KeyCode.None;
        }
    }

    static private KeyCode GetKeyCodeForGamepadInputOsx( GamepadInput a_input, int a_playerNum ) {
        switch(a_input) {
            case GamepadInput.ButtonA: 
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button16;
                    case 2: return KeyCode.Joystick2Button16;
                    case 3: return KeyCode.Joystick3Button16;
                    case 4: return KeyCode.Joystick4Button16;
                    default: return KeyCode.JoystickButton16;
                }
            case GamepadInput.ButtonB:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button17;
                    case 2: return KeyCode.Joystick2Button17;
                    case 3: return KeyCode.Joystick3Button17;
                    case 4: return KeyCode.Joystick4Button17;
                    default: return KeyCode.JoystickButton17;
                }
            case GamepadInput.ButtonX:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button18;
                    case 2: return KeyCode.Joystick2Button18;
                    case 3: return KeyCode.Joystick3Button18;
                    case 4: return KeyCode.Joystick4Button18;
                    default: return KeyCode.JoystickButton18;
                }
            case GamepadInput.ButtonY:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button19;
                    case 2: return KeyCode.Joystick2Button19;
                    case 3: return KeyCode.Joystick3Button19;
                    case 4: return KeyCode.Joystick4Button19;
                    default: return KeyCode.JoystickButton19;
                }
            case GamepadInput.ButtonBack:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button10;
                    case 2: return KeyCode.Joystick2Button10;
                    case 3: return KeyCode.Joystick3Button10;
                    case 4: return KeyCode.Joystick4Button10;
                    default: return KeyCode.JoystickButton10;
                }
            case GamepadInput.ButtonStart:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button9;
                    case 2: return KeyCode.Joystick2Button9;
                    case 3: return KeyCode.Joystick3Button9;
                    case 4: return KeyCode.Joystick4Button9;
                    default: return KeyCode.JoystickButton9;
                }
            case GamepadInput.ButtonLB:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button13;
                    case 2: return KeyCode.Joystick2Button13;
                    case 3: return KeyCode.Joystick3Button13;
                    case 4: return KeyCode.Joystick4Button13;
                    default: return KeyCode.JoystickButton13;
                }
            case GamepadInput.ButtonRB:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button14;
                    case 2: return KeyCode.Joystick2Button14;
                    case 3: return KeyCode.Joystick3Button14;
                    case 4: return KeyCode.Joystick4Button14;
                    default: return KeyCode.JoystickButton14;
                }
            case GamepadInput.ButtonLeftStick:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button11;
                    case 2: return KeyCode.Joystick2Button11;
                    case 3: return KeyCode.Joystick3Button11;
                    case 4: return KeyCode.Joystick4Button11;
                    default: return KeyCode.JoystickButton11;
                }
            case GamepadInput.ButtonRightStick:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button12;
                    case 2: return KeyCode.Joystick2Button12;
                    case 3: return KeyCode.Joystick3Button12;
                    case 4: return KeyCode.Joystick4Button12;
                    default: return KeyCode.JoystickButton12;
                }
            case GamepadInput.ButtonDpadDown:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button6;
                    case 2: return KeyCode.Joystick2Button6;
                    case 3: return KeyCode.Joystick3Button6;
                    case 4: return KeyCode.Joystick4Button6;
                    default: return KeyCode.JoystickButton6;
                }
            case GamepadInput.ButtonDpadLeft:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button7;
                    case 2: return KeyCode.Joystick2Button7;
                    case 3: return KeyCode.Joystick3Button7;
                    case 4: return KeyCode.Joystick4Button7;
                    default: return KeyCode.JoystickButton7;
                }
            case GamepadInput.ButtonDpadRight:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button8;
                    case 2: return KeyCode.Joystick2Button8;
                    case 3: return KeyCode.Joystick3Button8;
                    case 4: return KeyCode.Joystick4Button8;
                    default: return KeyCode.JoystickButton8;
                }
            case GamepadInput.ButtonDpadUp:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button5;
                    case 2: return KeyCode.Joystick2Button5;
                    case 3: return KeyCode.Joystick3Button5;
                    case 4: return KeyCode.Joystick4Button5;
                    default: return KeyCode.JoystickButton5;
                }
            default: return KeyCode.None;
        }
    }

    static private KeyCode GetKeyCodeForGamepadInputWindows( GamepadInput a_input, int a_playerNum ) {
        switch(a_input) {
            case GamepadInput.ButtonA: 
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button0;
                    case 2: return KeyCode.Joystick2Button0;
                    case 3: return KeyCode.Joystick3Button0;
                    case 4: return KeyCode.Joystick4Button0;
                    default: return KeyCode.JoystickButton0;
                }
            case GamepadInput.ButtonB:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button1;
                    case 2: return KeyCode.Joystick2Button1;
                    case 3: return KeyCode.Joystick3Button1;
                    case 4: return KeyCode.Joystick4Button1;
                    default: return KeyCode.JoystickButton1;
                }
            case GamepadInput.ButtonX:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button2;
                    case 2: return KeyCode.Joystick2Button2;
                    case 3: return KeyCode.Joystick3Button2;
                    case 4: return KeyCode.Joystick4Button2;
                    default: return KeyCode.JoystickButton2;
                }
            case GamepadInput.ButtonY:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button3;
                    case 2: return KeyCode.Joystick2Button3;
                    case 3: return KeyCode.Joystick3Button3;
                    case 4: return KeyCode.Joystick4Button3;
                    default: return KeyCode.JoystickButton3;
                }
            case GamepadInput.ButtonBack:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button6;
                    case 2: return KeyCode.Joystick2Button6;
                    case 3: return KeyCode.Joystick3Button6;
                    case 4: return KeyCode.Joystick4Button6;
                    default: return KeyCode.JoystickButton6;
                }
            case GamepadInput.ButtonStart:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button7;
                    case 2: return KeyCode.Joystick2Button7;
                    case 3: return KeyCode.Joystick3Button7;
                    case 4: return KeyCode.Joystick4Button7;
                    default: return KeyCode.JoystickButton7;
                }
            case GamepadInput.ButtonLB:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button4;
                    case 2: return KeyCode.Joystick2Button4;
                    case 3: return KeyCode.Joystick3Button4;
                    case 4: return KeyCode.Joystick4Button4;
                    default: return KeyCode.JoystickButton4;
                }
            case GamepadInput.ButtonRB:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button5;
                    case 2: return KeyCode.Joystick2Button5;
                    case 3: return KeyCode.Joystick3Button5;
                    case 4: return KeyCode.Joystick4Button5;
                    default: return KeyCode.JoystickButton5;
                }
            case GamepadInput.ButtonLeftStick:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button8;
                    case 2: return KeyCode.Joystick2Button8;
                    case 3: return KeyCode.Joystick3Button8;
                    case 4: return KeyCode.Joystick4Button8;
                    default: return KeyCode.JoystickButton8;
                }
            case GamepadInput.ButtonRightStick:
                switch(a_playerNum) {
                    case 1: return KeyCode.Joystick1Button9;
                    case 2: return KeyCode.Joystick2Button9;
                    case 3: return KeyCode.Joystick3Button9;
                    case 4: return KeyCode.Joystick4Button9;
                    default: return KeyCode.JoystickButton9;
                }
            default: return KeyCode.None;
        }
    }

    [SerializeField]
    private float m_axisDeadzone = 0.2f;

    public InputPage CurPage {
        get {
            if ( m_curPage == null ) return null;
            return instance.m_curPage;
        }
    }

    public bool IsPaused {
        get { return m_isPaused; }
        set {
            m_isPaused = value;
            if ( m_isPaused ) {
                ReleaseAll();
                Time.timeScale = 0.0f;

                PushPage( PAUSED_PAGE_NAME );
            } else {
                Time.timeScale = 1.0f;
                PopPage();
            }

            Debug.Log( "Paused: " + m_isPaused );
        }
    }

    public InputPage DefaultPage {  get { return GetPage( DEFAULT_PAGE_NAME ); } }
    public InputPage PausePage {  get { return GetPage( PAUSED_PAGE_NAME ); } }

    private InputPage m_curPage = null;

    private List<InputPage> m_inputPageList = new List<InputPage>();
    private Stack<string> m_inputPageStack = new Stack<string>();

    private bool m_isPaused = false;
    private List<KeyCode> m_pauseKeyList = new List<KeyCode>();

    public InputPage AddPage(string a_pageName ) {
        var page = new InputPage( a_pageName );
        m_inputPageList.Add( page );
        return page;
    }

    public void AddPauseKey( KeyCode a_key ) {
        m_pauseKeyList.Add( a_key );
    }

    public void ClearPauseKeys() {
        m_pauseKeyList.Clear();
    }

    public void RemovePauseKey( KeyCode a_key ) {
        m_pauseKeyList.Remove( a_key );
    }

    public void TogglePause() {
        IsPaused = !IsPaused;
    }

    public float GetAxis( string a_axisName ) {
        var axis = Input.GetAxis( a_axisName );
        if ( Mathf.Abs( axis ) < m_axisDeadzone ) return 0.0f;
        return axis;
    }

    public InputPage GetPage( string a_pageName = DEFAULT_PAGE_NAME, bool a_addIfNotFound = false ) {
        if ( string.IsNullOrEmpty( a_pageName ) ) a_pageName = DEFAULT_PAGE_NAME;

        if ( instance == null ) return null;

        foreach ( var page in m_inputPageList ) if ( page.Name == a_pageName ) return page;

        if ( a_addIfNotFound ) return AddPage( a_pageName );

        Debug.LogErrorFormat( "No page named '{0}' in input page stack.", a_pageName );
        return null;
    }

    public void PopPage() {
        if ( m_inputPageStack.Count == 1 ) return;

        m_inputPageStack.Pop();
        OnPageChanged();
    }

    public void PushPage( string a_pageName ) {
        if ( CurPage != null && CurPage.Name == a_pageName ) return;
        m_inputPageStack.Push( a_pageName );
        OnPageChanged();
    }

    private void OnPageChanged() {
        var pageName = m_inputPageStack.Peek();
        m_curPage = GetPage( pageName );
    }

    private void Awake() {
        if ( instance != null ) {
            Debug.LogWarningFormat( "Duplicate Input Manager on {0}. Destroying.", gameObject );
            Destroy( gameObject );
            return;
        }
        instance = this;

        DontDestroyOnLoad( gameObject );
        AddPage( DEFAULT_PAGE_NAME );
        PushPage( DEFAULT_PAGE_NAME );

        AddPage( PAUSED_PAGE_NAME );
    }

    [SerializeField]
    private bool m_debugInput = false;

    // TODO does this work?
    // trigger key up for all down/held keys (no duplicates)
    public void ReleaseAll() {
        var keyList = new List<KeyCode>();
        foreach( var key in m_curPage.KeyDownDict.Keys ) {
            if ( m_curPage.KeyUpDict.ContainsKey( key ) ) continue;
            keyList.Add( key );
        }

        keyList.AddRange( new List<KeyCode>( m_curPage.KeyHeldDict.Keys ) );

        foreach ( KeyCode key in keyList ) {
            if ( !m_curPage.KeyUpDict.ContainsKey( key ) ) continue;
            m_curPage.KeyUpDict[key].Invoke();
        }
    }

    private void Update() {
        if ( RoomManager.instance != null && RoomManager.instance.IsTransitioning ) return;

        ProcessKeyDown();
        ProcessKeyHeld();
        ProcessKeyUp();
        ProcessAxes();
    }

    private void ProcessKeyDown() {
        var keyList = new List<KeyCode>( m_curPage.KeyDownDict.Keys );
        foreach ( KeyCode key in keyList ) {
            if ( !Input.GetKeyDown( key ) ) continue;
            m_curPage.KeyDownDict[key].Invoke();
            if ( m_debugInput ) Debug.LogFormat( "Key down: {0}", key );
        }
    }

    private void ProcessKeyHeld() {
        var keyList = new List<KeyCode>( m_curPage.KeyHeldDict.Keys );
        foreach ( KeyCode key in keyList ) {
            if ( !Input.GetKey( key ) ) continue;
            m_curPage.KeyHeldDict[key].Invoke();
            if ( m_debugInput ) Debug.LogFormat( "Key held: {0}", key );
        }
    }

    private void ProcessKeyUp() {
        var keyList = new List<KeyCode>( m_curPage.KeyUpDict.Keys );
        foreach ( KeyCode key in keyList ) {
            if ( !Input.GetKeyUp( key ) ) continue;
            m_curPage.KeyUpDict[key].Invoke();
            if ( m_debugInput ) Debug.LogFormat( "Key up: {0}", key );
        }
    }

    private void ProcessAxes() {
        var axisList = new List<string>( m_curPage.AxisDict.Keys );
        foreach( string axis in axisList ) {
            var axisValue = Input.GetAxis( axis );
            if ( axisValue < m_axisDeadzone ) continue;
            if ( m_debugInput ) Debug.LogFormat( "Axis {0} = {1}", axis, axisValue );
            m_curPage.AxisDict[axis].Invoke( axisValue );
        }
    }
}
