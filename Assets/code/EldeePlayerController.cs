using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EldeePlayerController : MonoBehaviour {
    [SerializeField]
    private bool m_isFolded = false;

    private List<Part> m_partList = new List<Part>();

    private PartBox m_targetPartBox = null;
    private float m_speedMultiplier = 1.0f;

    public void ResetPosition() {
        RoomManager.instance.CurRegion = null;
        transform.position = Vector3.zero;
        GetComponent<Facing>().CurDirection = Facing.Direction.North;
        foreach ( var palette in GetComponentsInChildren<Palette>() ) palette.ResetColors();
        m_isFolded = false;
    }

    private void Awake() {
        if ( InputManager.instance.UseGamepad ) {
            var buttonA = InputManager.GetKeyCodeForGamepadInput( GamepadInput.ButtonY );
            var buttonY = InputManager.GetKeyCodeForGamepadInput( GamepadInput.ButtonY );
 
            InputManager.instance.GetPage().AddListenerDown( buttonA, Interact );
            InputManager.instance.GetPage().AddListenerDown( buttonY, ToggleFold );
        } else {
            InputManager.instance.GetPage().AddListenerUp( KeyCode.Z, Interact );
            InputManager.instance.GetPage().AddListenerUp( KeyCode.S, ToggleFold );
            InputManager.instance.GetPage().AddListenerDown( KeyCode.LeftControl, LockAiming );
            InputManager.instance.GetPage().AddListenerUp( KeyCode.LeftControl, UnlockAiming );
        }

        GetComponent<EldeeAttacher>().OnAttach.AddListener( OnAttach );
        GetComponent<EldeeAttacher>().OnDetach.AddListener( OnDetach );

        m_speedMultiplier = GetComponent<Mover>().SpeedMultiplier;
    }

    private void OnCollisionEnter2D( Collision2D collision ) {
        m_targetPartBox = collision.gameObject.GetComponent<PartBox>();
    }

    private void OnCollisionExit2D( Collision2D collision ) {
        m_targetPartBox = null;
    }

    private void Update() {
        // TODO a more optimal way to do this whenever a part is attached rather than every frame
            // AND when folded is changed
        GetComponent<EldeeAttacher>().IsTrigger = m_isFolded;
        foreach ( var part in m_partList ) {
            part.GetComponentInChildren<ShooterController>().enabled = !m_isFolded;
            part.GetComponentInChildren<SpriteRenderer>().enabled = !m_isFolded;
        }

        GetComponent<Mover>().SpeedMultiplier = m_isFolded ? 1.0f : m_speedMultiplier;

        if ( InputManager.instance.UseGamepad ) {
            if ( InputManager.instance.GetAxis( "Triggers" ) < 0.0f ) LockAiming();
            else UnlockAiming();
        }

    }

    private void LockAiming() {
        GetComponent<Facing>().LockFacing = true;
    }

    private void UnlockAiming() {
        GetComponent<Facing>().LockFacing = false;
    }

    private void DepositPart( PartBox a_partBox ) {
        var part = GetComponentInChildren<Part>();
        if ( part == null ) return;

        part.GetComponentInChildren<SpriteRenderer>().enabled = true;

        a_partBox.Deposit( part );
    }

    private void ToggleFold() {
        if ( GetComponent<EldeeAttacher>().PartCount == 0 ) return;
        m_isFolded = !m_isFolded;
    }

    private void Interact() {
        if ( m_targetPartBox == null ) return;
        if ( m_targetPartBox.HasPart ) RetrievePart( m_targetPartBox );
        else DepositPart( m_targetPartBox );
    }

    private void OnAttach(Part a_part ) {
        m_speedMultiplier -= 0.1f;
        m_partList.Add( a_part );
    }

    private void OnDetach(Part a_part ) {
        m_speedMultiplier += 0.1f;
        m_partList.Remove( a_part );
    }

    private void RetrievePart( PartBox a_partBox ) {
        if ( a_partBox == null ) return;

        var part = a_partBox.Retrieve();
        if ( part == null ) return;

        part.AttachTo( gameObject );
    }
}
