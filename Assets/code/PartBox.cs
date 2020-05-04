using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PartBox : MonoBehaviour {
    [SerializeField]
    Part m_part = null;

    public UnityEvent OnDeposit = new UnityEvent();
    public UnityEvent OnRetrieve = new UnityEvent();

    public bool HasPart {  get { return m_part != null; } }

    public void Deposit( Part a_part ) {
        if ( m_part != null ) return;

        m_part = a_part;
        m_part.Detach();

        m_part.transform.parent = transform;
        m_part.transform.localRotation = Quaternion.identity;
        m_part.transform.localPosition = Vector3.zero;
        m_part.transform.localScale = Vector2.one * 0.5f;

        m_part.GetComponent<Collider2D>().isTrigger = true;

        OnDeposit.Invoke();
    }

    public Part Retrieve() {
        if ( m_part == null ) return null;

        m_part.transform.parent = null;
        m_part.GetComponent<Collider2D>().isTrigger = false;

        var part = m_part;
        m_part = null;

        OnRetrieve.Invoke();

        return part;
    }
}
