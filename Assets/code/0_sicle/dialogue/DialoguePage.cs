using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguePage : MonoBehaviour {
    [System.Serializable]
    class DialogueOption
    {
        public string Name = "Unknown";
        public GoEvent OnChosen = null;
    }

    [SerializeField]
    [TextArea(4, 10)]
    private string m_text = "A mysterious, nondescript nothing.";

    [SerializeField]
    private AudioClip m_voiceClip = null;

    [SerializeField]
    private List<DialogueOption> m_optionList = new List<DialogueOption>();

    // TODO also end if the only option isn't saying a dialogue
    public bool IsEnd {  get { return m_optionList.Count == 0; } }
    public bool IsBranching {  get { return m_optionList.Count > 1; } }
    public bool HasAudio {  get { return m_voiceClip != null; } }
    public string Text {  get { return m_text; } }

    public float AudioLength {  get { return m_voiceClip == null ? 0.0f : m_voiceClip.length; } }

    private AudioSource m_voiceSource = null;

    public List<string> OptionList {
        get {
            var optionList = new List<string>();
            foreach ( var option in m_optionList ) optionList.Add( option.Name );
            return optionList;
        }
    }

    public void NextPage( GameObject a_speaker ) {
        if ( m_optionList.Count != 1 ) return;
        ChooseOption( 0, a_speaker.GetComponent<Speaker>() );
    }

    public void ChooseOption( int a_index, Speaker a_chooser ) {
        if( a_index < 0 || a_index >= m_optionList.Count ) {
            Debug.LogErrorFormat( "Illegal dialogue option index {0}.", a_index );
            return;
        }

        if( a_chooser == null ) {
            Debug.LogWarningFormat( "Tried to choose option in {0} with null chooser. Ignoring event.", name );
            return;
        }
        m_optionList[a_index].OnChosen.Invoke( a_chooser.gameObject );
    }

    public void Play() {
        if ( m_voiceClip == null ) return;
        m_voiceSource = SoundManager.instance.PlaySound( m_voiceClip );
    }

    public void ResetSpeaker(GameObject a_speaker ) {
        var speaker = a_speaker.GetComponent<Speaker>();
        if( speaker == null ) {
            Debug.LogErrorFormat( "Tried to reset dialogue in {1} from {0} but it's not a Speaker.", name, a_speaker );
            return;
        }
        speaker.ResetDialogue();
    }

    public void SpeakBy(GameObject a_speaker ) {
        var speaker = a_speaker.GetComponent<Speaker>();
        if( speaker == null ) {
            Debug.LogErrorFormat( "Tried to speak dialogue {0} by {1} but it's not a Speaker.", name, a_speaker );
            return;
        }
        speaker.Speak( this );
    }

    public void Stop() {
        if ( m_voiceSource != null ) SoundManager.instance.StopSound( m_voiceSource );
    }
}
