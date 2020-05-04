using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestroyedTrigger : MonoBehaviour {
    [SerializeField]
    private UnityEvent m_onDestroyed = new UnityEvent();
    public UnityEvent OnDestroyed {  get { return m_onDestroyed; } }

    private void OnDestroy() {
        m_onDestroyed.Invoke();
    }
}
