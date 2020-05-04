using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

[System.Serializable]
public class GoEvent : UnityEvent<GameObject> { }

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

[System.Serializable]
public class MonoBehaviourEvent : UnityEvent<MonoBehaviour> { }

[System.Serializable]
public class StringEvent : UnityEvent<string> { }

public static class Unicode
{
    static public string LeftArrow = '\u2190'.ToString();
    static public string UpArrow = '\u2191'.ToString();
    static public string RightArrow = '\u2192'.ToString();
    static public string DownArrow = '\u2193'.ToString();
    static public string HorizontalDoubleArrow = '\u2194'.ToString();
    static public string VerticalDoubleArrow = '\u2195'.ToString();
    static public string UpLeftArrow = '\u2196'.ToString();
    static public string UpRightArrow = '\u2197'.ToString();
    static public string DownRightArrow = '\u2198'.ToString();
    static public string DownLeftArrow = '\u2199'.ToString();
}

public class TimeStringConstructor
{
    static public bool includeMilliseconds = false;
    static public bool includeSeconds = true;
    static public bool includeMinutes = true;
    static public bool includeHours = false;

    public static string getTimeString( float a_seconds ) {
        float timeLeft = a_seconds;
        int hours = 0;
        int minutes = 0;
        int seconds = 0;
        int ms = 0;

        if ( includeHours ) {
            hours = Mathf.FloorToInt( a_seconds / 3600.0f );
            timeLeft -= 3600.0f * hours;
        }

        if ( includeMinutes ) {
            minutes = Mathf.FloorToInt( timeLeft / 60.0f );
            timeLeft -= 60.0f * minutes;
        }

        if ( includeSeconds ) {
            seconds = Mathf.FloorToInt( timeLeft );
            timeLeft -= seconds;
        }

        if ( includeMilliseconds ) {
            ms = Mathf.FloorToInt( timeLeft );
        }

        string ret = "";
        if ( includeHours )
            ret += string.Format( "{0:00}:", hours );
        if ( includeMinutes )
            ret += string.Format( "{0:00}:", minutes );
        if ( includeSeconds )
            ret += string.Format( "{0:00}", seconds );
        if ( includeMilliseconds )
            ret += string.Format( "{0:000}", ms );

        return ret;
    }
}

static public class Utility
{
    // Unity Editor extensions

    // GameObject extensions

    static public void DestroySelf(this GameObject a_obj) {
        GameObject.Destroy( a_obj );
    }

    static public void SetNonvisualComponentEnabled( this GameObject a_obj, bool a_enabled ) {
        foreach ( Transform child in a_obj.transform ) {
            child.gameObject.SetNonvisualComponentEnabled( a_enabled );
            foreach ( var comp in child.gameObject.GetComponents<MonoBehaviour>() ) {
                if ( comp.GetType() == typeof( Palette ) ) continue;
                comp.enabled = a_enabled;
            }
        }
    }

    // Layer extensions
    
    static public bool ContainsLayer( this LayerMask layermask, int layer ) {
        return layermask == ( layermask | ( 1 << layer ) );
    }

    static public int ToLayerInt( this LayerMask a_layerMask ) {
        var bitmask = a_layerMask.value;
        int result = bitmask > 0 ? 0 : 31;
        while ( bitmask > 1 ) {
            bitmask = bitmask >> 1;
            result++;
        }
        return result;
    }

    // Rect extensions

    static public Vector2 EdgeIntersectPoint(this Rect a_rect, Vector2 a_vecStart, Vector2 a_vecEnd) {
        var diff = a_vecEnd - a_vecStart;
        var direction = Facing.VectorToDirectionCardinal( diff );

        //Debug.LogFormat( "Target is {0} of start (diff {1})", direction, diff );

        var camRect = CameraManager.instance.Rectangle;
        var min = Vector2.zero;
        var max = Vector2.zero;
        switch (direction) {
            case Facing.Direction.East:
                min = new Vector2( camRect.xMax, camRect.yMin );
                max = new Vector2( camRect.xMax, camRect.yMax );
                break;
            case Facing.Direction.North:
                min = new Vector2( camRect.xMin, camRect.yMax );
                max = new Vector2( camRect.xMax, camRect.yMax );
                break;
            case Facing.Direction.South:
                min = new Vector2( camRect.xMin, camRect.yMin );
                max = new Vector2( camRect.xMax, camRect.yMin );
                break;
            case Facing.Direction.West:
                min = new Vector2( camRect.xMin, camRect.yMin );
                max = new Vector2( camRect.xMin, camRect.yMax );
                break;
        }

        var a1 = a_vecEnd.y -  a_vecStart.y;
        var b1 = a_vecStart.x - a_vecEnd.x;
        var c1 = a1 * a_vecStart.x + b1 * a_vecStart.y;

        var a2 = max.y - min.y;
        var b2 = min.x - max.x;
        var c2 = a2 * min.x + b2 * min.y;

        var det = a1 * b2 - a2 * b1;
        var x = ( b2 * c1 - b1 * c2 ) / det;
        var y = ( a1 * c2 - a2 * c1 ) / det;

        return new Vector2( x, y );
    }

    // SpriteRenderer extensions

    static public Vector2 GetActualSize(this SpriteRenderer a_renderer ) {
        var x = a_renderer.sprite.bounds.size.x; 
        var y =  a_renderer.sprite.bounds.size.y;

        x *= a_renderer.transform.localScale.x;
        y *= a_renderer.transform.localScale.y;

        if( a_renderer.drawMode != SpriteDrawMode.Simple ) {
            x *= a_renderer.size.x;
            y *= a_renderer.size.y;
        }

        var size = new Vector2( x, y );
        return size;
    }

    // string extensions

    static public Facing.Direction ToDirection( this string a_str ) {
        try {
            return (Facing.Direction)System.Enum.Parse( typeof( Facing.Direction ), a_str );
        } catch ( System.ArgumentException ) {
            Debug.LogErrorFormat( "{0} is an invalid direction. Defaulting to None.", a_str );
            return Facing.Direction.NoDirection;
        }
    }

    // Vector2 extensions

    static public Vector2 Rotate( this Vector2 a_vector, float a_angle ) {
        float sin = Mathf.Sin( a_angle * Mathf.Deg2Rad );
        float cos = Mathf.Cos( a_angle * Mathf.Deg2Rad );

        float tx = a_vector.x;
        float ty = a_vector.y;
        a_vector.x = ( cos * tx ) - ( sin * ty );
        a_vector.y = ( sin * tx ) + ( cos * ty );
        return a_vector;
    }

    static public Facing.Direction ToDirection( this Vector2 a_vector, bool a_cardinalOnly = false ) {
        if ( a_cardinalOnly ) return Facing.VectorToDirectionCardinal( a_vector );
        return Facing.VectorToDirection( a_vector );
    }

    // not extensions

    static public void GizmoArrow( Vector2 a_origin, Vector2 a_direction, float a_angle = 30.0f,
        float a_lengthMult = 0.2f ) {

        Gizmos.DrawRay( a_origin, a_direction );
        var end = a_origin + a_direction;
        var top = a_direction.Rotate( 180.0f + a_angle ) * a_lengthMult;
        var bottom = a_direction.Rotate( 180.0f - a_angle ) * a_lengthMult;
        Gizmos.DrawRay( end, top );
        Gizmos.DrawRay( end, bottom );
    }

    static public LayerMask LayerMaskFromInt(int layer ) {
        LayerMask mask = 1 << layer;
        return mask;
    }

    static public Color RandomColor( float a_alpha = 1.0f ) {
        var r = Random.Range( 0.0f, 1.0f );
        var g = Random.Range( 0.0f, 1.0f );
        var b = Random.Range( 0.0f, 1.0f );
        return new Color( r, g, b, a_alpha );
    }

    static public U RequireComponent<T, U>( T a_requirer, U a_currentComponent )
        where T : MonoBehaviour
        where U : Component {

        if ( a_currentComponent == null ) {
            var comp = a_requirer.GetComponent<U>();
            if ( comp == null ) {
                Debug.LogErrorFormat( "{0} requires {1} but none is set or in game object.", a_requirer.GetType(), 
                    a_currentComponent.GetType() );
                a_requirer.enabled = false;
            }
            return comp;
        }
        return a_currentComponent;
    }
}
