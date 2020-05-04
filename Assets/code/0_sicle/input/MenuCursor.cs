using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuCursor : MonoBehaviour {
    [SerializeField]
    int m_startIndex = 0;

    [SerializeField]
    private List<MenuOption> m_optionList = new List<MenuOption>();

    [SerializeField]
    Facing.Direction m_placementDirection = Facing.Direction.West;

    [SerializeField]
    private bool m_wrapAround = false;

    [SerializeField]
    private GoEvent m_onChanged = new GoEvent();

    public int CurIndex {
        get { return m_curIndex; }
        set {
            m_curIndex = value;

            if ( m_curIndex < 0 ) {
                if ( m_wrapAround ) m_curIndex = m_optionList.Count - 1;
                else m_curIndex = 0;
            }

            if ( m_curIndex >= m_optionList.Count ) {
                if ( m_wrapAround ) m_curIndex = 0;
                else m_curIndex = m_optionList.Count - 1;
            }

            if ( CurOption == null ) return;
            StartCoroutine( OnIndexChanged() );
        }
    }

    public MenuOption CurOption {
        get {
            if ( m_optionList.Count == 0 ) return null;
            return m_optionList[m_curIndex];
        }
    }

    private Vector2 m_halfCursorSize = Vector2.zero;

    private int m_curIndex = 0;

    public void ClearOptions() {
        m_optionList.Clear();
    }

    public void SetOptions(List<MenuOption> a_newOptionList ) {
        ClearOptions();
        for( var i = 0; i < a_newOptionList.Count; ++i ) {
            a_newOptionList[i].Id = i;
            m_optionList.Add( a_newOptionList[i] );
        }
    }

    private void Start() {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if ( spriteRenderer != null ) m_halfCursorSize = spriteRenderer.GetActualSize() / 2.0f;

        CurIndex = m_startIndex;
    }

    private void Update() {
        if ( Input.GetKeyDown( KeyCode.DownArrow ) ) ++CurIndex;
        if ( Input.GetKeyDown( KeyCode.UpArrow ) ) --CurIndex;
        if ( Input.GetKeyDown( KeyCode.Return ) ) CurOption.Select();
    }

    private IEnumerator OnIndexChanged() {
        yield return new WaitForEndOfFrame();

        var halfTargetSize = Vector2.zero;
        var spriteRenderer = CurOption.GetComponent<SpriteRenderer>();
        if ( spriteRenderer != null ) {
            halfTargetSize = spriteRenderer.GetActualSize() / 2.0f;
        } else {
            var textMeshPro = CurOption.GetComponent<TextMeshPro>();
            if ( textMeshPro != null )
                halfTargetSize = new Vector2( textMeshPro.renderedWidth / 2.0f, textMeshPro.renderedHeight / 2.0f );
        }

        var horizontal = Facing.GetHorizontal( m_placementDirection );
        var vertical = Facing.GetVertical( m_placementDirection );

        var pos = CurOption.transform.position;

        var offset = halfTargetSize + m_halfCursorSize;

        if ( horizontal == Facing.Direction.East ) pos.x += offset.x;
        if( horizontal == Facing.Direction.West ) pos.x -= offset.x;

        if( vertical == Facing.Direction.North ) pos.y += offset.y;
        if( vertical == Facing.Direction.South ) pos.y -= offset.y;

        transform.position = pos;

        m_onChanged.Invoke( CurOption.gameObject );
    }
}
