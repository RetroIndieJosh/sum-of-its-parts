using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PartEvent : UnityEvent<Part> { }

public class EldeeAttacher : MonoBehaviour {
    public PartEvent OnAttach = new PartEvent();
    public PartEvent OnDetach = new PartEvent();

    public int PartCount {  get { return m_colliderDict.Count; } }

    private Collider2D m_originalCollider = null;
    private Palette m_palette = null;

    public bool OriginalColliderIsTrigger {  set { m_originalCollider.isTrigger = value; } }
    public bool IsTrigger { set { foreach ( var collider in m_colliderDict.Values ) collider.isTrigger = value; } }

    public void DestroyColliders() {
        foreach ( var collider in m_colliderDict.Values ) Destroy( collider );
    }

    private void Awake() {
        m_originalCollider = GetComponent<Collider2D>();
        m_palette = GetComponentInChildren<Palette>();
    }

    Dictionary<Part, Collider2D> m_colliderDict = new Dictionary<Part, Collider2D>();

    private void Update() {
        foreach ( var part in m_colliderDict.Keys ) {
            var palette = part.GetComponentInChildren<Palette>();
            palette.CloneColors( m_palette );
        }
    }

    public void AttachPart(Part a_part ) {
        a_part.transform.localScale = Vector2.one;

        Collider2D collider = null;

        {
            var poly = a_part.GetComponent<PolygonCollider2D>();
            if ( poly != null ) {
                collider = gameObject.AddComponent<PolygonCollider2D>();
                collider.offset = (Vector2)a_part.transform.localPosition + poly.offset;
                ((PolygonCollider2D)collider).points = (Vector2[])poly.points.Clone();
            }
        }

        
        {
            var box = a_part.GetComponent<BoxCollider2D>();
            if ( box != null ) {
                collider = gameObject.AddComponent<BoxCollider2D>();
                collider.offset = (Vector2)a_part.transform.localPosition + box.offset;
                ((BoxCollider2D)collider).size = box.size;
            }
        }

        {
            var circle = a_part.GetComponent<CircleCollider2D>();
            if ( circle != null ) {
                collider = gameObject.AddComponent<CircleCollider2D>();
                collider.offset = (Vector2)a_part.transform.localPosition + circle.offset;
                ((CircleCollider2D)collider).radius = circle.radius;
            }
        }

        m_colliderDict[a_part] = collider;
        OnAttach.Invoke( a_part );
    }

    public void RemovePart(Part a_part ) {
        if( !m_colliderDict.ContainsKey(a_part)) {
            Debug.LogWarningFormat( "Tried to remove part {0} from {1} but it's not in the dictionary.", a_part, 
                name );
            return;
        }

        Destroy( m_colliderDict[a_part] );
        m_colliderDict.Remove( a_part );
        OnDetach.Invoke( a_part );
    }
}
