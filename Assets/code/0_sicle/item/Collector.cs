using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour {
    [System.Serializable]
    private class CollectEvent
    {
        public string Name = "";
        public GoEvent Event = new GoEvent();
    }

    [SerializeField]
    private List<CollectEvent> m_collectEventList = new List<CollectEvent>();

    public void Collect(GameObject a_obj, string a_type) {
        foreach( var collectEvent in m_collectEventList ) {
            if( collectEvent.Name == a_type ) collectEvent.Event.Invoke( a_obj );
        }
    }
}
