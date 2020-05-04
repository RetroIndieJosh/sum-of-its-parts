using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Usable : MonoBehaviour {
    [SerializeField]
    private string m_useType = "";

    [SerializeField]
    private bool m_destroyOnUsed = false;

    public void Use( GameObject a_user ) {
        a_user.GetComponent<User>().Use( gameObject, m_useType );
        if ( m_destroyOnUsed ) Destroy( gameObject );
    }
}
