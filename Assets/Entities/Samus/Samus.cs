using System;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        stateFlags |= 1 << (int) EntityFlag.DrawSprite;

        maxHitpoints = 99;
        hitpoints = maxHitpoints;

        maxHspd = 0.125f;
        maxVspd = -0.5f;
        hAccel = 0.02f;
        vAccel = 0.015f;

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
        GetInput(); // Gets input from the user for the current frame.
        if (IsPressed(SamusInput.Right) || IsPressed(SamusInput.Left)) {
            SetNextState(StateDefault);
        }
    }

    private void StateDefault() {
        GetInput(); // Gets input from the user for the current frame.

        //if (!GetFlag(EntityFlag.Grounded)) {
       //     vspd -= GetVertAccel();
        //    if (vspd < GetMaxVspd()) { vspd = GetMaxVspd(); }
       // }

        HorizontalMovement(1.0f);

        float _signHspd = Mathf.Sign(hspd);
        float _signVspd = Mathf.Sign(vspd);
        Vector3 _targetPosition = position.position + new Vector3(hspd + (collision.bounds.extents.x * _signHspd), vspd + (collision.bounds.extents.y * _signVspd));
        Vector3Int _targetCell = tilemap.WorldToCell(_targetPosition);
        if (tilemap.GetTile(_targetCell)) {
            float _unitHspd = hspd == 0.0f ? 0.0f : _signHspd / 16.0f;
            float _unitVspd = vspd == 0.0f ? 0.0f : _signVspd / 16.0f;
            Vector3 _unitVector = new Vector3(_unitHspd, _unitVspd);
            Vector3 _nextPosition = new Vector3(_unitHspd + (collision.bounds.extents.x * _signHspd), _unitVspd + (collision.bounds.extents.y * _signVspd));
            _targetCell = tilemap.WorldToCell(position.position + _nextPosition);
            while(tilemap.GetTile(_targetCell) == null) {
                position.position += _unitVector;
                _targetCell = tilemap.WorldToCell(position.position + _nextPosition);
            }
            hspd = 0.0f;
            vspd = 0.0f;
		}
        position.position += new Vector3(hspd, vspd);
    }

	private void StateAirbourne() {

    }

    private void StateMorphball() {

    }

    /// <summary>
    /// Handles horizontal movement achieved by the player pressing either the right or left movement inputs, both at the same time, (Which 
    /// causes Samus to decelerate as if no inputs are pressed) or neither of them. When she's somersaulting in middair she will be unable to
    /// decelerate, but will be able to in any other situation.
    /// </summary>
    /// <param name="_accelFactor"></param>
    private void HorizontalMovement(float _accelFactor) {
        int _movement = Convert.ToInt32(IsHeldDown(SamusInput.Right)) - Convert.ToInt32(IsHeldDown(SamusInput.Left));
        if (_movement != 0) {
            float _hAccel = GetHorAccel() * _accelFactor;
            float _maxHspd = GetMaxHspd();
            hspd += _hAccel * _movement;
            if (hspd > _maxHspd || hspd < -_maxHspd) {
                hspd = _maxHspd * _movement;
            }
            stateFlags |= 1 << (int) SamusFlag.Walking;
        } else if (!GetFlag((EntityFlag) SamusFlag.JumpSpin)) {
            float _hAccel = GetHorAccel() * _accelFactor;
            hspd -= _hAccel * Math.Sign(hspd);
            if (hspd > -_hAccel && hspd < _hAccel) {
                stateFlags &= ~(1 << (int) SamusFlag.Walking);
                hspd = 0.0f;
            }
        }
    }

    // 
    private bool IsPressed(SamusInput _input)   { return (inputFlags & (1 << (int) _input)) != 0 && (prevInputFlags & (1 << (int) _input)) == 0; }
    private bool IsReleased(SamusInput _input)  { return (inputFlags & (1 << (int) _input)) == 0 && (prevInputFlags & (1 << (int) _input)) != 0; }
    private bool IsHeldDown(SamusInput _input)  { return (inputFlags & (1 << (int) _input)) != 0; }
}