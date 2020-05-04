using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour {
    public void LogFloat( float a_value ) {
        LogString( a_value.ToString() );
    }

    public void LogInt( int a_value ) {
        LogString( a_value.ToString() );
    }

    public void LogString( string a_value ) {
        Debug.Log( a_value );
    }
    
    public void LogObjectName( GameObject a_obj ) {
        LogString( a_obj.name );
    }
}
