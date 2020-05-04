using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class DialogueWindow : MonoBehaviour {
    static public DialogueWindow instance = null;

    [Header("Font Settings")]

    public Color textColor = Color.white;
    
    [SerializeField]
    private TextMeshPro m_textMesh = null;

    [SerializeField]
    private TextMeshPro m_portraitLabel = null;

    [SerializeField]
    private MenuCursor m_menuCursor = null;

    [SerializeField]
    private TextMeshPro m_optionPrefab = null;

    [Header( "Background Settings" )]

    [SerializeField]
    [Tooltip( "Number of milliseconds between each character printed" )]
    private float m_charWaitTimeMs = 50;

    [SerializeField]
    [Range(1.0f, 10.0f)]
    [Tooltip( "Multiplier for speed at which characters appear when 'go faster' button is held" )]
    float m_charWaitTimeFastMult = 5.0f;

    [Header("Sprites")]

    [SerializeField]
    private SpriteRenderer m_portraitBackground = null;

    [SerializeField]
    private SpriteRenderer m_portraitRenderer = null;

    [SerializeField]
    private SpriteRenderer m_textBackground = null;

    [SerializeField]
    private SpriteRenderer m_optionsBackground = null;

    [SerializeField]
    private SpriteRenderer m_endPageSymbol = null;

    [SerializeField]
    private SpriteRenderer m_endTextSymbol = null;

    [SerializeField]
    private float m_endSymbolBlinkTimeMs = 500.0f;

    public bool IsOnLastPage { get { return m_curDialogue.IsEnd; } }
    public bool IsFullPageRevealed { get; private set; }

    public bool IsShowing { get; private set; }

    public bool AcceleratePrint { private get; set; }

    private float m_timeElapsed = 0.0f;

    private int m_charIndex = 0;

    private Speaker m_speaker = null;

    private DialoguePage m_curDialogue = null;
    public DialoguePage CurDialogue {
        private get { return m_curDialogue; }
        set {
            Clear();

            Debug.LogFormat( "set text to {0}", value.Text );

            m_textBackground.color = Color.white;
            m_portraitBackground.color = Color.white;
            m_portraitRenderer.color = Color.white;

            m_charIndex = 0;

            m_textMesh.color = textColor;
            m_textMesh.text = "";

            IsFullPageRevealed = false;
            IsShowing = true;

            m_curDialogue = value;

            InputManager.instance.PushPage( "Dialogue" );
        }
    }

    public void Clear() {
        m_textMesh.text = "";
        m_textMesh.color = Color.clear;

        m_textBackground.color = Color.clear;
        m_portraitBackground.color = Color.clear;
        m_portraitRenderer.color = Color.clear;
        m_endPageSymbol.color = Color.clear;
        m_endTextSymbol.color = Color.clear;
        m_optionsBackground.color = Color.clear;

        m_menuCursor.ClearOptions();
        var cursorSprite = m_menuCursor.GetComponent<SpriteRenderer>();
        if ( cursorSprite != null ) cursorSprite.color = Color.clear;
        m_menuCursor.enabled = false;

        var children = new List<GameObject>();
        foreach ( Transform child in m_optionsBackground.transform ) children.Add( child.gameObject );
        children.ForEach(child => Destroy(child));

        IsShowing = false;
        m_portraitLabel.text = "";
        m_timeElapsed = 0.0f;
    }

    public void EndPage() {
        if ( !IsShowing ) return;
        InputManager.instance.PopPage();
        Clear();
    }

    public void SetSpeaker( Speaker a_speaker, string a_name, Sprite a_portrait = null ) {
        m_speaker = a_speaker;
        m_portraitLabel.text = a_name;
        m_portraitRenderer.sprite = a_portrait;
    }

    private void Awake() {
        if ( instance != null ) {
            Destroy( gameObject );
            return;
        }

        instance = this;

        if ( m_textMesh == null ) m_textMesh = GetComponent<TextMeshPro>();
        if( m_textMesh == null ) {
            Debug.LogErrorFormat( "Must set (or have) text mesh in {0}. Destroying.", name );
            Destroy( gameObject );
            return;
        }
        m_textMesh.enableWordWrapping = true;
        m_textMesh.gameObject.AddComponent<SortingOrder>().sortingName = "Text";

        if( m_portraitLabel == null ) {
            Debug.LogErrorFormat( "Must set text mesh in {0} for speaker. Destroying.", name );
            Destroy( gameObject );
            return;
        }
        m_portraitLabel.gameObject.AddComponent<SortingOrder>().sortingName = "Text";

        IsFullPageRevealed = false;

        Clear();
    }

    private void Start() {
        var inputPage = InputManager.instance.GetPage( "Dialogue", true );
        inputPage.AddListenerDown( KeyCode.X, AccelerateDialogue, true );
        inputPage.AddListenerDown( KeyCode.Z, AdvanceDialogue, true );
    }

    private void Update() {
        if ( !IsShowing ) return;

        m_timeElapsed += Time.deltaTime;

        if( IsFullPageRevealed) {
            HandleSymbolBlink();
            return;
        }

        HandleNextChar();
    }

    private void AccelerateDialogue() {
        m_timeElapsed += Time.deltaTime * ( m_charWaitTimeFastMult - 1.0f );
    }

    private void AdvanceDialogue() {
        if ( !IsShowing ) return;
        if ( IsFullPageRevealed && IsOnLastPage && m_curDialogue.IsBranching ) return;
        RevealOrNextPage();
    }

    private SpriteRenderer CreateSprite( Sprite a_sprite = null ) {
        var obj = new GameObject();
        obj.transform.parent = transform;

        var spriteRenderer = obj.AddComponent<SpriteRenderer>();
        if( a_sprite != null ) spriteRenderer.sprite = a_sprite;
        return spriteRenderer;
    }

    private TextMeshPro CreateTextMesh() {
        var obj = new GameObject();
        obj.transform.parent = transform;

        transform.transform.transform.transform.transform.transform.transform.position = Vector3.zero;

        var textMesh = obj.AddComponent<TextMeshPro>();
        return textMesh;
    }

    private void HandleNextChar() {
        if ( m_timeElapsed < m_charWaitTimeMs / 1000.0f ) return;

        m_textMesh.text += m_curDialogue.Text[m_charIndex];
        ++m_charIndex;

        if ( m_charIndex >= m_curDialogue.Text.Length ) Reveal();

        m_timeElapsed = 0.0f;
    }

    private void HandleSymbolBlink() {
        if ( IsFullPageRevealed && m_curDialogue.IsBranching ) return;
        if ( m_timeElapsed < m_endSymbolBlinkTimeMs / 1000.0f ) return;

        if ( IsOnLastPage ) {
            if ( m_endTextSymbol.color == Color.white ) m_endTextSymbol.color = Color.clear;
            else m_endTextSymbol.color = Color.white;
        } else {
            if ( m_endPageSymbol.color == Color.white ) m_endPageSymbol.color = Color.clear;
            else m_endPageSymbol.color = Color.white;
        }

        m_timeElapsed -= m_endSymbolBlinkTimeMs / 1000.0f;
    }

    private List<MenuOption> m_optionList = new List<MenuOption>();

    private void Reveal() {
        m_charIndex = m_curDialogue.Text.Length;
        m_textMesh.text = m_curDialogue.Text;

        IsFullPageRevealed = true;

        if ( !m_curDialogue.IsBranching ) return;

        m_optionsBackground.color = Color.white;

        if ( m_optionPrefab == null ) {
            Debug.LogErrorFormat( "Cannot show options in {0} without option prefab.", name );
            return;
        }

        foreach( var optionName in m_curDialogue.OptionList ) {
            var option = Instantiate( m_optionPrefab );
            option.text = optionName;
            option.transform.SetParent( m_optionsBackground.transform, true );

            var menuOption = option.GetComponent<MenuOption>();
            menuOption.OnSelected.AddListener( ChooseOption );

            option.GetComponent<TextMeshPro>().color = Color.clear;

            m_optionList.Add( menuOption );
        }

        m_optionsBackground.size = Vector2.zero;

        StartCoroutine( RevealFinish() );
    }

    [Header("Options Display")]

    [SerializeField]
    private Vector2 m_optionsTextMargin = Vector2.one * 0.2f;

    [SerializeField]
    private float m_optionsTextVerticalSpacing = 0.1f;

    [SerializeField]
    private float m_optionsDisplayLerpTime = 0.5f;

    private IEnumerator RevealFinish() {
        yield return new WaitForEndOfFrame();

        var optionWidthMax = 0.0f;
        foreach ( var option in m_optionList ) {
            optionWidthMax = Mathf.Max( option.GetComponent<TextMeshPro>().renderedWidth, optionWidthMax );
            Debug.Log( "Width max: " + optionWidthMax );
        }
        var optionHeight = m_optionList[0].GetComponent<TextMeshPro>().renderedHeight;

        var windowWidth = optionWidthMax + m_optionsTextMargin.x * 2.0f;
        var insideHeight = ( optionHeight + m_optionsTextVerticalSpacing ) * m_optionList.Count;
        var windowHeight = insideHeight + m_optionsTextMargin.y * 2.0f;
        var targetSize = new Vector2( windowWidth, windowHeight );
        Debug.Log( "Background size: " + m_optionsBackground.size );

        var x = m_optionsBackground.transform.position.x;
        var y = m_optionsBackground.transform.position.y + insideHeight / 2.0f - optionHeight / 2.0f;
        foreach( var option in m_optionList ) {
            option.transform.position = new Vector2( x, y );
            y -= optionHeight + m_optionsTextVerticalSpacing;
        }

        // lerp the window size
        var timeElapsed = 0.0f;
        while ( timeElapsed < m_optionsDisplayLerpTime ) {
            m_optionsBackground.size = Vector2.Lerp( Vector2.zero, targetSize, timeElapsed / m_optionsDisplayLerpTime );
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // set up the cursor
        if( m_menuCursor == null ) {
            Debug.LogErrorFormat( "Cannot show options in {0} without a menu cursor set.", name );
            yield break;
        }
        m_menuCursor.SetOptions( m_optionList );
        m_menuCursor.CurIndex = 0;

        // reveal options and cursor
        foreach ( var option in m_optionList ) option.GetComponent<TextMeshPro>().color = Color.white;
        m_menuCursor.GetComponent<SpriteRenderer>().color = Color.white;
        m_menuCursor.enabled = true;
    }

    private void ChooseOption(GameObject a_option ) {
        m_optionList.Clear();

        var option = a_option.GetComponent<MenuOption>();
        if ( option == null ) return;
        CurDialogue.ChooseOption( option.Id, m_speaker );
    }

    private void RevealOrNextPage() {
        if ( !IsShowing ) return;

        // reveal
        m_endPageSymbol.color = Color.clear;
        m_endTextSymbol.color = Color.clear;
        if( !IsFullPageRevealed ) {
            StopAllCoroutines();
            Reveal();
            return;
        }

        if ( m_curDialogue.IsBranching ) return;

        // next page
        EndPage();

        if( IsOnLastPage ) return;
        m_curDialogue.ChooseOption( 0, m_speaker );
    }

}
