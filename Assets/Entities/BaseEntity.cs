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

    // Stores the current sprite that represents the entity within the game; can be initialized within the editor.
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

    // Stores the game's main tilemap so it can be referenced by the entity when collision with the world is checked.
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
