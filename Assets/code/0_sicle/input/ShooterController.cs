using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterController : MonoBehaviour
{
    [System.Serializable]
    public enum FireMode
    {
        Single,
        Rapid,
        Chain,
        Timed,
        ChargeHoldRelease,
        ChargeHoldMax,
        ChargePress
    }

    [SerializeField]
    public FireMode CurFireMode = FireMode.Single;

    [SerializeField]
    private Shooter m_shooter = null;

    private KeyCode m_fireButton = KeyCode.None;

    private void OnEnable() {
        if ( InputManager.instance == null ) return;

        InputManager.instance.GetPage().AddListenerDown( KeyCode.X, HandleFireDown );
        InputManager.instance.GetPage().AddListenerHeld( KeyCode.X, HandleFireHeld );
        InputManager.instance.GetPage().AddListenerUp( KeyCode.X, HandleFireUp );

        if ( InputManager.instance.UseGamepad ) {
            InputManager.instance.GetPage().AddListenerDown( m_fireButton, HandleFireDown );
            InputManager.instance.GetPage().AddListenerHeld( m_fireButton, HandleFireHeld );
            InputManager.instance.GetPage().AddListenerUp( m_fireButton, HandleFireUp );
        }
    }

    private void OnDisable() {
        if ( InputManager.instance == null ) return;

        InputManager.instance.GetPage().RemoveListenerDown( KeyCode.X, HandleFireDown );
        InputManager.instance.GetPage().RemoveListenerHeld( KeyCode.X, HandleFireHeld );
        InputManager.instance.GetPage().RemoveListenerUp( KeyCode.X, HandleFireUp );

        if ( InputManager.instance.UseGamepad ) {
            InputManager.instance.GetPage().RemoveListenerDown( m_fireButton, HandleFireDown );
            InputManager.instance.GetPage().RemoveListenerHeld( m_fireButton, HandleFireHeld );
            InputManager.instance.GetPage().RemoveListenerUp( m_fireButton, HandleFireUp );
        }
    }

    private void Start() {
        if ( m_shooter == null ) m_shooter = GetComponent<Shooter>();
        if ( m_shooter == null ) {
            Debug.LogErrorFormat( "Shooter Controller in {0} requires Shooter. Destroying.", name );
            Destroy( gameObject );
            return;
        }

        m_fireButton = InputManager.GetKeyCodeForGamepadInput( GamepadInput.ButtonX );
    }

    private void HandleFireDown() {
        switch ( CurFireMode ) {
            case FireMode.Chain:
            case FireMode.Rapid: m_shooter.StartFire(); break;

            case FireMode.Single: m_shooter.Fire(); break;
            case FireMode.Timed: m_shooter.FireForSec( 2.0f ); break;
            case FireMode.ChargeHoldRelease: m_shooter.ChargeReset(); break;
            case FireMode.ChargePress: m_shooter.ChargeToMax(); break;

            case FireMode.ChargeHoldMax:
                if ( !m_allowCharging ) return;
                m_shooter.ChargeReset();
                break;
        }
    }

    private void HandleFireHeld() {
        switch ( CurFireMode ) {
            case FireMode.ChargeHoldMax:
                m_shooter.Charge( Time.deltaTime );
                if ( m_shooter.ChargePercent < 1.0f ) return;
                m_shooter.Fire();
                m_hasFired = true;
                break;

            case FireMode.ChargeHoldRelease: m_shooter.Charge( Time.deltaTime ); break;
        }
    }

    private void HandleFireUp() {
        switch ( CurFireMode ) {
            case FireMode.Chain: m_shooter.FireForSec( 2.0f ); break;
            case FireMode.Rapid: m_shooter.StopFire(); break;
            case FireMode.ChargeHoldRelease: m_shooter.Fire(); break;

            case FireMode.ChargeHoldMax:
                if ( !m_hasFired ) m_shooter.Fire();
                m_hasFired = false;
                break;

        }
    }

    private bool m_allowCharging = true;
    private bool m_hasFired = false;
}
