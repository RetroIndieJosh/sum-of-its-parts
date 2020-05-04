using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO fix day night palette
public class DayNightPalette : MonoBehaviour {
    [SerializeField]
    private Palette m_dayPalette = null;

    [SerializeField]
    private Palette m_nightPalette = null;

    private Palette m_Palette = null;

    int m_dayPaletteIndex = 0;
    int m_nightPaletteIndex = 0;

    public void OnSunrise() {
        Debug.Log( "Sunrise" );
        //m_Palette.PaletteIndex = m_nightPaletteIndex;
        //m_Palette.LerpPaletteIndex = m_dayPaletteIndex;
        //m_Palette.LerpTime = TimeManager.instance.SunriseLengthSec;
    }

    public void OnSunset() {
        Debug.Log( "Sunset" );
        //m_Palette.PaletteIndex = m_dayPaletteIndex;
        //m_Palette.LerpPaletteIndex = m_nightPaletteIndex;
        //m_Palette.LerpTime = TimeManager.instance.SunsetLengthSec;
    }

    private void Awake() {
        m_Palette = GetComponent<Palette>();
    }

    private void Start() {
        if ( PaletteManager.instance == null ) {
            Debug.LogErrorFormat( "Day Night Palette requires Palette Manager. Destroying in {0}.", name );
            Destroy( this );
            return;
        }

        if ( m_dayPalette == null ) Debug.LogErrorFormat( "No day palette set on {0}", gameObject );
        if ( m_nightPalette == null ) Debug.LogErrorFormat( "No night palette set on {0}", gameObject );

        m_dayPaletteIndex = PaletteManager.instance.AddPalette( m_dayPalette );
        m_nightPaletteIndex = PaletteManager.instance.AddPalette( m_nightPalette );

        m_Palette.Mode = PaletteMode.Index;
        if ( TimeManager.instance.IsDaytime ) m_Palette.PaletteIndex = m_dayPaletteIndex;
        else m_Palette.PaletteIndex = m_nightPaletteIndex;

        TimeManager.instance.OnSunrise.AddListener( OnSunrise );
        TimeManager.instance.OnSunset.AddListener( OnSunset );
    }
}
