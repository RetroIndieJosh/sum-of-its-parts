using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorSpriteToRect : MonoBehaviour {
    [SerializeField]
    private bool m_anchorBottom = false;

    [SerializeField]
    private bool m_anchorLeft = false;

    [SerializeField]
    private bool m_anchorRight = false;

    [SerializeField]
    private bool m_anchorTop = false;

    [SerializeField]
    private SpriteRenderer m_spriteRenderer = null;

    [SerializeField]
    protected Rect m_anchorRect = new Rect();

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube( m_anchorRect.center, m_anchorRect.size );
    }

    private void Start() {
        if ( m_spriteRenderer == null ) m_spriteRenderer = GetComponent<SpriteRenderer>();
        if( m_spriteRenderer.drawMode == SpriteDrawMode.Simple ) m_spriteRenderer.drawMode = SpriteDrawMode.Tiled;
    }

    protected virtual void Update() {
        if ( m_anchorBottom ) AnchorBottom();
        if ( m_anchorLeft ) AnchorLeft();
        if ( m_anchorRight ) AnchorRight();
        if ( m_anchorTop ) AnchorTop();
    }

    private void AnchorBottom() {
        var pos = transform.position;
        pos.y = m_anchorRect.yMin + m_spriteRenderer.size.y / 2.0f;
        transform.position = pos;
    }

    private void AnchorLeft() {
        var pos = transform.position;
        pos.x = m_anchorRect.xMin + m_spriteRenderer.size.x / 2.0f;
        transform.position = pos;
    }

    private void AnchorRight() {
        if ( m_anchorLeft ) {
            var size = m_spriteRenderer.size;
            size.x = m_anchorRect.width;
            m_spriteRenderer.size = size;

            var pos = transform.position;
            pos.x = m_anchorRect.center.x;
            transform.position = pos;
        } else {
            var pos = transform.position;
            pos.x = m_anchorRect.xMax - m_spriteRenderer.size.x / 2.0f;
            transform.position = pos;
        }
    }

    private void AnchorTop() {
        if ( m_anchorBottom ) {
            var size = m_spriteRenderer.size;
            size.y = m_anchorRect.height;
            m_spriteRenderer.size = size;

            var pos = transform.position;
            pos.y = m_anchorRect.center.y;
            transform.position = pos;
        } else {
            var pos = transform.position;
            pos.y = m_anchorRect.yMax - m_spriteRenderer.size.y / 2.0f;
            transform.position = pos;
        }
    }

}
