using UnityEngine;
using UnityEngine.Tilemaps;

public enum EntityFlag {
    Grounded =      28,
    DrawSprite =    29,
    Invincible =    30,
    Destroyed =     31,
}

public class BaseEntity : MonoBehaviour {
    // Contains 32 flags for various entity states (Only the top four bits are used by the BaseEntity class).
    protected int stateFlags = 0;

    // Variables for the an entity's state machine. The variables will store the currently executed state, the one to switch to on the next
    // frame (If required), and the previously executed state.
    protected delegate void State();
    protected State curState, nextState, prevState;

    // Total damage an entity can withstand.
    protected uint hitpoints = 0;
    protected uint maxHitpoints = 0;

    // 
    [SerializeField]
    protected Sprite sprite;

    // General movement variables.
    protected float hspd = 0.0f;
    protected float maxHspd = 0.0f;
    protected float vspd = 0.0f;
    protected float maxVspd = 0.0f;
    protected float hAccel = 0.0f;
    protected float vAccel = 0.0f;

    // Fraction storage values for an entity's position to perserve pixel-perfect movement.
    protected float hspdFraction = 0.0f;
    protected float vspdFraction = 0.0f;

    // Variables that allow movement physics to be altered without overwriting the original values.
    protected float maxHspdFactor = 1.0f;
    protected float maxVspdFactor = 1.0f;
    protected float hAccelFactor = 1.0f;
    protected float vAccelFactor = 1.0f;

    // Components from the Engine that allow manipulation of their various functions through code.
    protected Transform         position;
    protected BoxCollider2D     collision;
    protected SpriteRenderer    spriteRenderer;

    // 
    protected Tilemap tilemap;

    protected void Start() {
        position = gameObject.GetComponent<Transform>();
        collision = gameObject.GetComponent<BoxCollider2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite; // Assigns the inital sprite for the entity in case it wasn't in the prefab.
        tilemap = GameObject.Find("Background").GetComponent<Tilemap>();
    }

    protected void FixedUpdate() {
        curState?.Invoke();
    }

    protected void LateUpdate() {
        if (curState != nextState) { curState = nextState; }
        if (GetFlag(EntityFlag.Destroyed) && !GetFlag(EntityFlag.Invincible)) { Destroy(gameObject); }
        spriteRenderer.enabled = GetFlag(EntityFlag.DrawSprite);
    }

    /// <summary>
    /// 
    /// </summary>
    protected delegate void CollisionFunction(float _hspd, float _vspd);
    protected void ApplyFrameMovement(CollisionFunction _collisionFunction) {
        // Step One: Add the previous frame's fractional values to the entity's current hspd and vspd values.
        hspd += hspdFraction;
        vspd += vspdFraction;

        // Step Two: Perform a calculation that will isolate the fractional value from whatever the current values for hspd and vspd are.
        hspdFraction = hspd - (Mathf.Floor(Mathf.Abs(hspd)) * Mathf.Sign(hspd));
        vspdFraction = vspd - (Mathf.Floor(Mathf.Abs(vspd)) * Mathf.Sign(vspd));

        // Step Three: Remove those fractional values from both hspd and vspd; leaving only the resulting whole numbers of each.
        hspd -= hspdFraction;
        vspd -= vspdFraction;

        // Step Four: Call the world collision function to update the position of the entity and have it collide with the world. Note that
        // if no valid collision function has been provided to this function, this line will do nothing.
        _collisionFunction?.Invoke(hspd, vspd);
    }

    /// <summary>
    /// 
    /// </summary>
    protected void ProcessGravity() {
        
	}

    /// <summary>
    /// 
    /// </summary>
    protected void ProcessWorldCollision(float _hspd, float _vspd) {
        // 
        float _signHspd = Mathf.Sign(_hspd);
        float _collisionOffset = collision.bounds.extents.x * _signHspd;
        Vector3 _targetPosition = position.position + new Vector3(_collisionOffset + _hspd - (0.5f * _signHspd), 0.0f);
        Vector3Int _collisionCell = tilemap.WorldToCell(_targetPosition);
        if (tilemap.GetTile(_collisionCell) != null) {
            // 
            Vector3 _offset = new Vector3(_collisionOffset + _signHspd - (0.5f * _signHspd), 0.0f);
            Vector3Int _curCell = tilemap.WorldToCell(position.position + _offset);
            while(tilemap.GetTile(_curCell) == null) {
                position.position += new Vector3(_signHspd, 0.0f);
                _curCell = tilemap.WorldToCell(position.position + _offset);
            }

            // 
            hspd = 0.0f;
            hspdFraction = 0.0f;
            _hspd = 0.0f;
		}
        position.position += new Vector3(_hspd, 0.0f);

        // 
        float _signVspd = Mathf.Sign(_vspd);
        _collisionOffset = collision.bounds.extents.y * _signVspd;
        _targetPosition = position.position + new Vector3(0.0f, _collisionOffset + _vspd);
        _collisionCell = tilemap.WorldToCell(_targetPosition);
        if (tilemap.GetTile(_collisionCell) != null) {
            // 
            Vector3 _offset = new Vector3(0.0f, _collisionOffset + _signVspd);
            Vector3Int _curCell = tilemap.WorldToCell(position.position + _offset);
            while(tilemap.GetTile(_curCell) == null) {
                position.position += new Vector3(0.0f, _signVspd);
                _curCell = tilemap.WorldToCell(position.position + _offset);
			}

            // 
            vspd = 0.0f;
            vspdFraction = 0.0f;
            _vspd = 0.0f;
		}
        position.position += new Vector3(0.0f, _vspd);
    }

    /// <summary>
    /// Sets up the entity to change to a new state on the next game frame. If "curState" was set to null and this function is called, it will
    /// immediately set this new state instead of waiting for Unity to call this class's "LateUpdate" function.
    /// </summary>
    /// <param name="_newState"></param>
    protected void SetNextState(State _newState) {
        prevState = curState;
        nextState = _newState;
        if (curState == null) { curState = _newState; }
    }

    /// <summary>
    /// Gets the current state of a flag (Can be either a one or a zero) found within the entity's "stateFlags" variable.
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    protected bool GetFlag(EntityFlag _index) { return (stateFlags & (1 << (int) _index)) != 0; }

    // Getters for the entity's current maximum velocity values with respect to their current factor values; allowing for any number of changes
    // to these physics variables without having to overwrite the original values.
    protected float GetMaxHspd() { return maxHspd * maxHspdFactor; }
    protected float GetMaxVspd() { return maxVspd * maxVspdFactor; }
    protected float GetHorAccel() { return hAccel * hAccelFactor; }
    protected float GetVertAccel() { return vAccel * vAccelFactor; }
}
