using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorSpriteToCamera : AnchorSpriteToRect {
    [SerializeField]
    private bool m_useForcedRatio = false;

    [SerializeField]
    private float m_forcedRatio = 4.0f / 3.0f;

    protected override void Update() {
        if ( !m_useForcedRatio ) return;

        m_anchorRect = CameraManager.instance.Rectangle;
        var prevWidth = m_anchorRect.width;
        m_anchorRect.width = m_anchorRect.height * m_forcedRatio;
        var diff = prevWidth - m_anchorRect.width;
        m_anchorRect.x += diff / 2.0f;

        base.Update();
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube( m_anchorRect.center, m_anchorRect.size );
    }
}
