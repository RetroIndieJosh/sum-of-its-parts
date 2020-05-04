using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionMapper : MonoBehaviour {
    // TODO make these SpriteRenderer
    [SerializeField]
    private Sprite m_regionSprite = null;

    [SerializeField]
    private Sprite m_exitSprite = null;

    [SerializeField]
    private string m_mapLayerName = "";

    [SerializeField]
    private int m_mapSortingOrder = 0;

    private GameObject m_map = null;
    private float m_scale = 1.0f;
    private Rect m_allRegionsRect = new Rect();

    private void Start() {
        foreach ( var region in FindObjectsOfType<Room>() ) {
            m_allRegionsRect.xMin = Mathf.Min( region.Rect.xMin, m_allRegionsRect.xMin );
            m_allRegionsRect.xMax = Mathf.Max( region.Rect.xMax, m_allRegionsRect.xMax );

            m_allRegionsRect.yMin = Mathf.Min( region.Rect.yMin, m_allRegionsRect.yMin );
            m_allRegionsRect.yMax = Mathf.Max( region.Rect.yMax, m_allRegionsRect.yMax );
        }

        var scaleX = CameraManager.instance.Rectangle.width / m_allRegionsRect.width;
        var scaleY = CameraManager.instance.Rectangle.height / m_allRegionsRect.height;
        m_scale = Mathf.Min( scaleX, scaleY );
        m_map = new GameObject() { name = "Map" };
        m_map.transform.parent = Camera.main.transform;

        Vector3 pos = -m_allRegionsRect.center * m_scale;
        pos.z = 5.0f;
        m_map.transform.localPosition = pos;

        m_map.SetActive( false );
    }

    public void ToggleMap() {
        m_map.SetActive( !m_map.activeSelf );
        if ( !m_map.activeSelf ) return;

        foreach ( var region in FindObjectsOfType<Room>() ) {
            var mapRep = GetMapRepresentation( region );

            mapRep.transform.parent = m_map.transform;
            mapRep.transform.localPosition = region.Rect.center * m_scale;
        }

        foreach( var sr in m_map.GetComponentsInChildren<SpriteRenderer>()) {
            sr.sortingLayerName = m_mapLayerName;
            sr.sortingOrder += m_mapSortingOrder;
        }

        Debug.Log( "Map rect: " + m_allRegionsRect );
    }

    public GameObject GetMapRepresentation(Room a_region ) {
        var mapRepresentation = new GameObject() {
            name = a_region.name + "(Map)"
        };

        var sr = mapRepresentation.AddComponent<SpriteRenderer>();
        sr.sprite = m_regionSprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = a_region.Rect.size * m_scale;

        var exitPrefab = new GameObject();
        exitPrefab.AddComponent<SpriteRenderer>().sprite = m_exitSprite;

        var exitList = a_region.DoorList;

        foreach ( var exit in exitList ) {
            /*
            if ( !exit.enabled ) continue;

            var pos = exit.Door.transform.position;
            var exitRep = Instantiate( exitPrefab, pos * m_scale, Quaternion.identity );
            exitRep.name = name + " Exit " + exit.Direction + " (Representation)";

            var exitRenderer = exitRep.GetComponent<SpriteRenderer>();
            exitRenderer.size = a_region.Rect.size * m_scale / 2.0f;

            if ( exit.Door != null && exit.Door.IsOpen )
                exitRenderer.color = Color.green;
            else exitRenderer.color = Color.white;

            exitRep.transform.parent = mapRepresentation.transform;
            */
        }

        Destroy( exitPrefab );

        var brightness = a_region == RoomManager.instance.CurRegion ? 1.0f : a_region.Discovered ? 0.5f : 0.1f;

        var r = sr.color.r * brightness;
        var g = sr.color.g * brightness;
        var b = sr.color.b * brightness;
        sr.color = new Color( r, g, b, 1.0f );

        return mapRepresentation;
    }
}
