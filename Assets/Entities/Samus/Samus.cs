using System;
using UnityEngine;

public enum SamusInput {
    Right =         0,
    Left =          1,
    Up =            2,
    Down =          3,
    Jump =          4,
    Shoot =         5,
}

public enum SamusFlag {
    Walking =       0,
    Shooting =      1,
    AimingUp =      2,
    JumpSpin =      3,
    JumpAttack =    4,
    Submerged =     5,
}

public class Samus : BaseEntity {
    // Similar to how the BaseEntity uses a single variable for 32 possible state flags, these input-oriented variables will store the values
    // for the player's keyboard/gamepad input for the current frame; storing the previous frame's inputs in order to check for input presses
    // and releases.
    private uint inputFlags = 0;
    private uint prevInputFlags = 0;

    private new void Start() {
        base.Start(); // Calls "BaseEntity" start function to grab various components
        collision.size = new Vector2(10, 32);
        collision.enabled = true;

        stateFlags |= 1 << (int) EntityFlag.DrawSprite;

        maxHitpoints = 99;
        hitpoints = maxHitpoints;

        maxHspd = 2.1f;
        maxVspd = 8.0f;
        hAccel = 0.3f;
        vAccel = 0.25f;

        DontDestroyOnLoad(gameObject);
        SetNextState(StateIntro);
    }

    private void GetInput() {
        prevInputFlags = inputFlags;
        inputFlags =    (Convert.ToUInt32(Input.GetKey(KeyCode.RightArrow))     << (int) SamusInput.Right) |
                        (Convert.ToUInt32(Input.GetKey(KeyCode.LeftArrow))      << (int) SamusInput.Left) |
                        (Convert.ToUInt32(Input.GetKey(KeyCode.UpArrow))        << (int) SamusInput.Up) |
                        (Convert.ToUInt32(Input.GetKey(KeyCode.DownArrow))      << (int) SamusInput.Down) |
                        (Convert.ToUInt32(Input.GetKey(KeyCode.Z))              << (int) SamusInput.Shoot) |
                        (Convert.ToUInt32(Input.GetKey(KeyCode.X))              << (int) SamusInput.Jump);
    }

    private void StateIntro() {
        GetInput();
        if (IsPressed(SamusInput.Right) || IsPressed(SamusInput.Left)) {
            SetNextState(StateDefault);
        }
    }

    private void StateDefault() {
        GetInput();

        int _movement = Convert.ToInt32(IsHeldDown(SamusInput.Right)) - Convert.ToInt32(IsHeldDown(SamusInput.Left));
        if (_movement != 0) {
            float _hAccel = GetHorAccel();
            float _maxHspd = GetMaxHspd();
            hspd += _hAccel * _movement;
            if (hspd > _maxHspd || hspd < -_maxHspd) {
                hspd = _maxHspd * _movement;
            }
            stateFlags |= 1 << (int) SamusFlag.Walking;
        } else {
            float _hAccel = GetHorAccel();
            hspd -= _hAccel * Math.Sign(hspd);
            if (hspd > -_hAccel && hspd < _hAccel) {
                stateFlags &= ~(1 << (int) SamusFlag.Walking);
                hspd = 0.0f;
            }
		}

		ApplyFrameMovement(ProcessWorldCollision);
    }

    private void StateAirborune() {

    }

    private void StateMorphball() {

    }

    private bool IsPressed(SamusInput _input)   { return (inputFlags & (1 << (int) _input)) != 0 && (prevInputFlags & (1 << (int) _input)) != 0; }
    private bool IsReleased(SamusInput _input)  { return (inputFlags & (1 << (int) _input)) == 0 && (prevInputFlags & (1 << (int) _input)) == 0; }
    private bool IsHeldDown(SamusInput _input)  { return (inputFlags & (1 << (int) _input)) != 0; }
}