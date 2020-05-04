/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePalette : MonoBehaviour {
    [SerializeField]
    private bool m_useDefault = false;

    public bool UseCustomPalette = false;
    public Palette CustomPalette = null;

    public int PaletteIndex = 0;
    public int LerpPaletteIndex = 0;

    public float LerpTime {
        private get { return m_lerpTime; }
        set {
            m_lerpTimeMax = value;
            m_lerpTime = m_lerpTimeMax;
        }
    }

    private float m_lerpTime = 0.0f;
    private float m_lerpTimeMax = 0.0f;

    public bool Dirty { private get; set; }

    public Palette Palette = null;
    public Palette LerpPalette = null;

    private SpriteRenderer m_spriteRenderer = null;

    public void SetWhiteForSeconds( float a_seconds ) {
        StartCoroutine( SetColorForSeconds( Color.white, a_seconds ) );
    }

    private IEnumerator SetColorForSeconds(Color a_color, float a_seconds ) {
        var prevUseCustom = UseCustomPalette;
        var prevCustomPalette = CustomPalette == null ? null : Instantiate( CustomPalette );

        UseCustomPalette = true;
        CustomPalette.MonoColor(Color.white);

        float timeElapsed = 0.0f;
        while( timeElapsed < a_seconds) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        UseCustomPalette = prevUseCustom;
        CustomPalette = prevCustomPalette;
        if( prevCustomPalette != null ) Destroy( prevCustomPalette );
    }

	private void Awake() {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        if( m_spriteRenderer == null) {
            Debug.LogErrorFormat( "Palette requires a sprite renderer in {0}.", gameObject );
            Destroy( this );
            return;
        }

        if ( Palette == null ) Palette = gameObject.AddComponent<Palette>();
	}

    private void Start() {
        if ( PaletteManager.instance == null ) {
            Debug.LogErrorFormat( "Sprite Palette requires Palette Manager. Destroying in {0}.", name );
            Destroy( this );
            return;
        }

        UpdatePalette();
    }

    private void Update() {
        m_lerpTime -= Time.deltaTime;
        if ( m_lerpTime > 0 ) {
            if ( m_useDefault ) Debug.LogWarning( "Sprite set to use default pal, but is lerping between pals!" );
            Dirty = true;
        }

        if ( PaletteIndex != m_prevPaletteIndex ) {
            m_prevPaletteIndex = PaletteIndex;
            Dirty = true;
        }

        if ( Dirty ) UpdatePalette();

        if ( !m_useDefault ) return;
        PaletteIndex = PaletteManager.instance.DefaultPaletteIndex;
    }

    private void UpdatePalette() {
        Dirty = false;

        var colorSwapTex = new Texture2D( 256, 1, TextureFormat.RGBA32, false, false ) {
            filterMode = FilterMode.Point
        };

        for ( int i = 0; i < 256; ++i ) { colorSwapTex.SetPixel( i, 0, Color.magenta ); }

        // lerp between palettes
        var colorList = new Color[4];
        var lerpAmount = m_lerpTime > 0 ? 1.0f - m_lerpTime / m_lerpTimeMax : 0.0f;
        //Debug.LogFormat( "Time: {0} | Amount: {1}", m_lerpTime, lerpAmount );

        for ( int i = 0; i < 4; ++i ) { colorList[i] = Color.Lerp( Palette[i], LerpPalette[i], lerpAmount ); } 

        colorSwapTex.SetPixel( 0, 0, colorList[0] );
        colorSwapTex.SetPixel( 85, 0, colorList[1] );
        colorSwapTex.SetPixel( 170, 0, colorList[2] );
        colorSwapTex.SetPixel( 254, 0, colorList[3] );

        colorSwapTex.Apply();
        GetComponent<SpriteRenderer>().material.SetTexture( "_SwapTex", colorSwapTex );
    }
}
*/
