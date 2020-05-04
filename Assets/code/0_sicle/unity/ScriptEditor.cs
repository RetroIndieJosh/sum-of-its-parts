#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScriptEditor : Editor
{
    private List<string> m_propSkipList = new List<string>();

    public ScriptEditor()  {
        m_propSkipList.Clear();
        HideProperty( "m_Script" );
    }

    protected void ShowProperty(string a_propName ) {
        if ( !m_propSkipList.Contains( a_propName ) ) return;
        m_propSkipList.Remove( a_propName );
    }

    protected void HideProperty(string a_propName ) {
        if ( m_propSkipList.Contains( a_propName ) ) return;
        m_propSkipList.Add( a_propName );
    }

    protected void DrawDefaultScriptEditor() {
        var output = "Skip properties: ";
        foreach ( var prop in m_propSkipList ) output += prop + ", ";
        //Debug.Log( output );

        var propIter = serializedObject.GetIterator();
        if ( propIter.NextVisible( true ) ) {
            do {
                var prop = serializedObject.FindProperty( propIter.name );

                bool doSkip = false;
                foreach ( var skipName in m_propSkipList ) {
                    if ( skipName == prop.name ) {
                        doSkip = true;
                        break;
                    }
                }
                if ( doSkip ) continue;

                UnityEditor.EditorGUILayout.PropertyField( prop, true );
            } while ( propIter.NextVisible( false ) );
        }

        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
    }
}

#endif // UNITY_EDITOR
