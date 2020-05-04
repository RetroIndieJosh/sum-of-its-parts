using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour {
    [System.Serializable]
    private class UseEvent
    {
        public string Name = "";
        public GoEvent Event = new GoEvent();
    }

    [SerializeField]
    private List<UseEvent> m_collectEventList = new List<UseEvent>();

    public void Use(GameObject a_obj, string a_type) {
        foreach( var collectEvent in m_collectEventList ) {
            if( collectEvent.Name == a_type ) collectEvent.Event.Invoke( a_obj );
        }
    }
}
