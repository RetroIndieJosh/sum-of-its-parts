using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoOptions : MonoBehaviour {
    public bool IsFullScreen {
        get { return m_isFullScreen; }
        set { m_isFullScreen = value; }
    }

    public void SetResolution(int a_resolutionId) {
        m_screenWidth = m_screenSizes[a_resolutionId][0];
        m_screenHeight = m_screenSizes[a_resolutionId][1];
    }

    private int[][] m_screenSizes = {
        new int[] { 640, 480 },
        new int[] { 1280, 720 },
        new int[] { 1920, 1080 },
    };

    private bool m_isFullScreen = false;
    private int m_screenWidth = 1280;
    private int m_screenHeight = 720;

    public void Apply() {
        Screen.SetResolution( m_screenWidth, m_screenHeight, m_isFullScreen );
    }
}
