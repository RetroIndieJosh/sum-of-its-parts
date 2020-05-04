using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOption : MonoBehaviour {
    [SerializeField]
    private GoEvent m_onSelected = new GoEvent();

    public int Id = -1;

    public GoEvent OnSelected {  get { return m_onSelected; } }

    public void Select() {
        m_onSelected.Invoke( gameObject );
    }
}
