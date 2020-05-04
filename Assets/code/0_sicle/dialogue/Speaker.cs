using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : MonoBehaviour {
    [SerializeField]
    private string m_name = "Unknown";

    [SerializeField]
    private Sprite m_portrait = null;

    [SerializeField]
    private DialoguePage m_initialDialogue = null;

    [SerializeField]
    private SpeechBubble m_speechBubblePrefab = null;

    [SerializeField]
    private int m_maxSpeechBubbleLength = 60;

    [HideInInspector]
    public DialoguePage CurDialogue = null;

    public bool IsVisible {  get { return DialogueWindow.instance.IsShowing || m_displayedOnSpeechBubble; } }

    private SpeechBubble m_speechBubble = null;

    private bool m_displayedOnSpeechBubble = false;

    public void Clear() {
        DialogueWindow.instance.EndPage();
        if( m_speechBubble != null ) m_speechBubble.Clear();
        if( CurDialogue != null ) CurDialogue.Stop();
    }

    public void ResetDialogue() {
        CurDialogue = m_initialDialogue;
    }

    public void Speak() {
        Clear();

        Debug.Log( "Dialogue length: " + CurDialogue.Text.Length );
        if ( CurDialogue.IsBranching || CurDialogue.Text.Length > m_maxSpeechBubbleLength ) {
            DialogueWindow.instance.CurDialogue = CurDialogue;
            DialogueWindow.instance.SetSpeaker( this, m_name, m_portrait );
        } else {
            var speechShowTimeSec = Mathf.Max( CurDialogue.AudioLength, CurDialogue.Text.Length / 15.0f );
            m_speechBubble.Display( this, CurDialogue, 0.0f, speechShowTimeSec );
            m_displayedOnSpeechBubble = true;
        }

        CurDialogue.Play();
    }

    public void Speak( DialoguePage a_dialogue ) {
        CurDialogue = a_dialogue;
        Speak();
    }

    public void Say( string a_text ) {
        Debug.LogWarning( "Say(text) is deprecated" );
    }

    void Start() {
        m_speechBubble = Instantiate( m_speechBubblePrefab );
        m_speechBubble.transform.parent = transform;
        m_speechBubble.transform.position = transform.position;

        ResetDialogue();
    }

    private void Update() {
        if ( CurDialogue == null ) return;
        if ( m_displayedOnSpeechBubble && !m_speechBubble.IsVisible ) m_displayedOnSpeechBubble = false;
    }
}
