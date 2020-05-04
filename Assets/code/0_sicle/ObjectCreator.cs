using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCreator : MonoBehaviour
{
    public void Create( GameObject a_prefab ) {
        Instantiate( a_prefab, transform.position, transform.rotation );
    }

    public void CreateForSec( GameObject a_prefab, float a_timeSec ) {
        var go = Instantiate( a_prefab );
        Destroy( go, a_timeSec );
    }
}
