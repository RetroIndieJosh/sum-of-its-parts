using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor( typeof( Palette ) )]
public class PaletteScriptEditor : ScriptEditor
{
    public override void OnInspectorGUI() {
        Palette palette = target as Palette;

        if( palette.Mode == PaletteMode.Custom ) {
            ShowProperty( "m_color0" );
            ShowProperty( "m_color1" );
            ShowProperty( "m_color2" );
            ShowProperty( "m_color3" );
        } else {
            HideProperty( "m_color0" );
            HideProperty( "m_color1" );
            HideProperty( "m_color2" );
            HideProperty( "m_color3" );
        }

        if ( palette.Mode == PaletteMode.Index ) ShowProperty( "PaletteIndex" );
        else HideProperty( "PaletteIndex" );

        DrawDefaultScriptEditor();

        palette.UpdateColors();
    }
}

#endif // UNITY_EDITOR

[System.Serializable]
public enum PaletteMode
{
    Default,
    Index,
    Custom
}

public class Palette : MonoBehaviour {
    public PaletteMode Mode = PaletteMode.Custom;

    [SerializeField]
    public int PaletteIndex = 0;

    [SerializeField]
    private Color m_color0 = Color.black;

    [SerializeField]
    private Color m_color1 = new Color( 0.3f, 0.3f, 0.3f, 1.0f );

    [SerializeField]
    private Color m_color2 = new Color( 0.6f, 0.6f, 0.6f, 1.0f );

    [SerializeField]
    private Color m_color3 = Color.white;

    public int Count { get { return 4; } }

    public Color this[int i] {
        get {
            switch ( i ) {
                case 0: return m_color0;
                case 1: return m_color1;
                case 2: return m_color2;
                case 3: return m_color3;
                default: return Color.magenta;
            }
        }
        set {
            switch ( i ) {
                case 0: m_color0 = value; break;
                case 1: m_color1 = value; break;
                case 2: m_color2 = value; break;
                case 3: m_color3 = value; break;
                default: break;
            }

            UpdateColors();
        }
    }

    private Texture2D m_colorSwapTex = null;
    private Color[] m_prevColors = new Color[4];

    public void CloneColors(Palette a_otherPalette ) {
        for ( int i = 0; i < 4; ++i ) this[i] = a_otherPalette[i];
    }

    public void FlashWhite( float a_interval, float a_length ) {
        if ( !isActiveAndEnabled ) return;
        StartCoroutine( FlashWhiteCoroutine( a_interval, a_length ) );
    }

    public void MonoColor(Color a_color ) {
        m_color0 = m_color1 = m_color2 = m_color3 = a_color;
    }

    public void SetWhiteForSeconds( float a_seconds ) {
        if ( !isActiveAndEnabled ) return;
        ResetColors();
        StartCoroutine( SetColorForSeconds( Color.white, a_seconds ) );
    }

    public void StoreColors() {
        for ( int i = 0; i < 4; ++i ) m_prevColors[i] = this[i];
    }

    public void ResetColors() {
        for ( int i = 0; i < 4; ++i ) this[i] = m_prevColors[i];
    }

    [ExecuteInEditMode]
    public void UpdateColors() {
        if ( m_colorSwapTex == null ) CreateTexture();

        for ( int i = 0; i < 256; ++i ) { m_colorSwapTex.SetPixel( i, 0, Color.magenta ); }

        if ( Mode != PaletteMode.Custom ) {
            var palette = PaletteManager.instance.DefaultPalette;
            if ( Mode == PaletteMode.Index ) palette = PaletteManager.instance.GetPalette( PaletteIndex );
            for ( int i = 0; i < 4; ++i ) { this[i] = palette[i]; }
        }

        // lerp between palettes
        var colorList = new Color[4];
        //var lerpAmount = m_lerpTime > 0 ? 1.0f - m_lerpTime / m_lerpTimeMax : 0.0f;
        //Debug.LogFormat( "Time: {0} | Amount: {1}", m_lerpTime, lerpAmount );

        // TODO lerp derp
        //for ( int i = 0; i < 4; ++i ) { colorList[i] = Color.Lerp( Palette[i], LerpPalette[i], lerpAmount ); } 

        for ( int i = 0; i < 4; ++i ) { colorList[i] = this[i]; }

        m_colorSwapTex.SetPixel( 0, 0, colorList[0] );
        m_colorSwapTex.SetPixel( 85, 0, colorList[1] );
        m_colorSwapTex.SetPixel( 170, 0, colorList[2] );
        m_colorSwapTex.SetPixel( 254, 0, colorList[3] );

        m_colorSwapTex.Apply();

#if UNITY_EDITOR
        var sr = GetComponent<SpriteRenderer>();
        var tempMaterial = new Material( sr.sharedMaterial );
        tempMaterial.SetTexture( "_SwapTex", m_colorSwapTex );
        sr.sharedMaterial = tempMaterial;
#else
        GetComponent<SpriteRenderer>().material.SetTexture( "_SwapTex", m_colorSwapTex );
#endif
    }

    private void Start() {
        CreateTexture();
        StoreColors();
    }

    private void CreateTexture() {
        m_colorSwapTex = new Texture2D( 256, 1, TextureFormat.RGBA32, false, false ) {
            filterMode = FilterMode.Point
        };
    }

    private IEnumerator FlashWhiteCoroutine( float a_interval, float a_length ) {
        var totalTimeElapsed = 0.0f;
        while ( totalTimeElapsed < a_length ) {
            var timeElapsed = 0.0f;
            SetWhiteForSeconds( a_interval );
            while ( timeElapsed < a_interval * 2.0f ) {
                timeElapsed += Time.deltaTime;
                totalTimeElapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator SetColorForSeconds(Color a_color, float a_seconds ) {
        StoreColors();

        MonoColor( a_color );

        float timeElapsed = 0.0f;
        while( timeElapsed < a_seconds) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        ResetColors();
    }

    private void Update() {
        // TODO if we're using the default, sync our palette to that

        // TODO handle palette lerp
        /*
        m_lerpTime -= Time.deltaTime;
        if ( m_lerpTime > 0 ) {
            if ( m_useDefault ) Debug.LogWarning( "Sprite set to use default pal, but is lerping between pals!" );
            Dirty = true;
        }
        */

        if ( GetComponent<SpriteRenderer>() == null ) {
            Destroy( this );
            return;
        }

        // TODO don't do this every frame, only when palette changed
        UpdateColors();
    }

}
