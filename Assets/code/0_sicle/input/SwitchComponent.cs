using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwitcherBase : MonoBehaviour {
    public virtual void DoSwitch() { }
    protected virtual void Activate() { }
}

public class SwitchComponent<T> : SwitcherBase where T : MonoBehaviour {
    /*
    [SerializeField]
    [Tooltip("Set false if you want control over when the initial activation occurs.")]
    private bool m_activateOnStart = false;

    [SerializeField]
    private int m_initialActiveId = 0;
    */

    [SerializeField]
    private KeyCode m_key = KeyCode.S;

    [SerializeField]
    private List<T> m_componentList = new List<T>();

    private int m_activeId = -1;

    public T ActiveComponent {
        get {
            if ( m_activeId < 0 ) return null;
            return m_componentList[m_activeId];
        }
    }

    public override void DoSwitch() {
        if ( ActiveComponent != null ) {
            ActiveComponent.SendMessage( "OnSwitchedFrom" + typeof( T ).ToString() );
            ActiveComponent.enabled = false;
        }

        ++m_activeId;
        if( m_activeId >= m_componentList.Count) m_activeId = 0;
        Activate();

        if ( ActiveComponent == null ) {
            Debug.LogErrorFormat( "Switched to null component in {0}. Destroying." );
            Destroy( this );
            return;
        }

        ActiveComponent.SendMessage( "OnSwitchedTo" + typeof(T).ToString() );
    }

    protected override void Activate() {
        m_componentList[m_activeId].enabled = true;
    }

    private void Start() {
        if( m_componentList.Count == 0 ) {
            Debug.LogErrorFormat( "Switcher in {0} must have at least one component defined. Destroying.", name );
            Destroy( this );
            return;
        }

        foreach ( var component in m_componentList ) {
            if ( component == null ) {
                Debug.LogErrorFormat( "Switcher in {0} cannot have null components. Destroying.", name );
                Destroy( this );
                return;
            }
        }

        InputManager.instance.GetPage().AddListenerDown( m_key, DoSwitch, true );

        foreach ( var component in m_componentList ) {
            component.SendMessage( "OnSwitchedFrom" + typeof(T).ToString() );
            component.enabled = false;
        }

        //if ( m_activateOnStart ) DoSwitch();
    }
}
