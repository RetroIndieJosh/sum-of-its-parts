using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour {
    [SerializeField]
    private string m_sceneName = "";

    [SerializeField]
    private float m_delay = 0.0f;

    [SerializeField]
    private bool m_executeOnStart = false;

    public void ChangeScene() { StartCoroutine( ChangeAfterSeconds() ); } 

    public void ChangeScene(float a_delay ) {
        m_delay = a_delay;
        ChangeScene();
    }

    public void ChangeScene(string a_sceneName ) {
        m_sceneName = a_sceneName;
        ChangeScene();
    }

    private void Start() {
        if ( m_executeOnStart ) ChangeScene();
    }

    private IEnumerator ChangeAfterSeconds() {
        yield return new WaitForSeconds( m_delay );
        SceneManager.LoadScene( m_sceneName );
    }
}
