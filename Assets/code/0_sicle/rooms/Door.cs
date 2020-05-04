using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Door : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer m_spriteRenderer = null;

    [SerializeField]
    private Sprite m_closedSprite = null;

    [SerializeField]
    private Sprite m_openSprite = null;

    [SerializeField]
    private bool m_isOpen = false;

    private Collider2D m_collider = null;

    public bool IsOpen {
        get { return m_isOpen; }
        set {
            m_isOpen = value;
            m_collider.isTrigger = m_isOpen;
            if ( m_spriteRenderer != null ) m_spriteRenderer.sprite = m_isOpen ? m_openSprite : m_closedSprite;

            AudioClip sound = null;
            if ( value ) sound = RoomManager.instance.DoorOpenSound;
            else sound = RoomManager.instance.DoorCloseSound;
            if ( sound == null ) return;

            SoundManager.instance.PlaySound( sound, transform.position );
        }
    }

    private void Awake() {
        if ( m_spriteRenderer == null ) m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_collider = GetComponent<Collider2D>();
        IsOpen = m_isOpen;
    }
}
