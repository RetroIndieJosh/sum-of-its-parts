using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    static public SoundManager instance = null;

    [System.Serializable]
    class Sound
    {
        public string name = "unnamed";
        public AudioClip clip = null;
    }

    [SerializeField]
    private List<Sound> m_soundList = new List<Sound>();

    [SerializeField]
    private float m_soundVolume = 0.7f;

    Dictionary<string, AudioClip> m_soundDict = new Dictionary<string, AudioClip>();
    List<AudioSource> m_audioSourceList = new List<AudioSource>();

    public AudioSource PlaySound(AudioClip a_clip ) {
        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = a_clip;
        audioSource.volume = m_soundVolume;
        audioSource.Play();
        m_audioSourceList.Add( audioSource );

        StartCoroutine( DestroySoundWhenFinished( audioSource ) );
        return audioSource;
    }

    public AudioSource PlaySound( AudioClip a_clip, Vector2 a_position ) {
        var go = new GameObject();
        go.transform.parent = transform;

        var audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = a_clip;
        audioSource.volume = m_soundVolume;
        audioSource.Play();

        go.transform.position = a_position;
        Destroy( go, a_clip.length + 0.1f );

        return audioSource;
    }

    public AudioSource PlaySound(string a_soundName) {
        if( !m_soundDict.ContainsKey(a_soundName)) {
            Debug.LogErrorFormat( "Invalid sound name '{0}'.", a_soundName );
            return null;
        }

        return PlaySound( m_soundDict[a_soundName] );
    }

    public void PlaySoundNoReturn(AudioClip a_clip ) {
        PlaySound( a_clip );
    }

    public void PlaySoundNoReturn( string a_soundName ) {
        PlaySound( a_soundName );
    }

    public void StopSound(AudioSource a_source) {
        if ( !m_audioSourceList.Contains( a_source ) ) return;

        a_source.Stop();
        m_audioSourceList.Remove( a_source );
    }

    private void Awake() {
        if( instance != null ) {
            Debug.LogErrorFormat( "{0} has a duplicate sound manager. Destroying the sound manager.", gameObject );
            Destroy( this );
            return;
        }
        instance = this;

        foreach( var sound in m_soundList) {
            if( sound.clip == null ) {
                Debug.LogWarningFormat( "Sound for '{0}' not set. Ignoring.", sound.name );
                continue;
            }

            if ( m_soundDict.ContainsKey( sound.name ) ) {
                Debug.LogErrorFormat( "Duplicate sound name '{0}'. Ignoring.", sound.name );
                continue;
            }

            m_soundDict.Add( sound.name, sound.clip );
        }
    }

    private IEnumerator DestroySoundWhenFinished(AudioSource a_audioSource) {
        var timeElapsed = 0.0f;
        while( timeElapsed < a_audioSource.clip.length ) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        StopSound( a_audioSource );
        Destroy( a_audioSource );
    }
}
