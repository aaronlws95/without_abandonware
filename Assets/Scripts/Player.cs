using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Enums
    public enum MoveState
    {
        WALLRUN,
        GRAVITY,
        BOUNCE
    }

    public enum PlayerState
    {
        ACTIVE,
        DEAD
    }

    // Constants
    int WALL_LAYER = 6;
    int WAYPOINTS_LAYER = 7;

    [Header("General")]
    Grid grid;
    public Sprite[] sprites;
    public MoveState moveState;
    public PlayerState playerState;
    public float stateChangeCooldown = 0f;
    public float reverseCooldown = 0f;
    public float wallCheckRayCastLength = 0.5f;
    float stateChangeCount = 0f;
    float reverseCount = 0f;

    [Header("Wall Run")]
    public float defaultWallRunSpeed = 10f;
    public float minWallRunSpeed = 10f;
    public float maxWallRunSpeed = 15f;
    public float curWallRunSpeed = 10f;
    public float wallRunRayCastLength = 0.55f;
    public float waypointDistThreshold = 0.2f;
    public bool isClockwise = false;
    bool startWallRun = false;
    Waypoints curWaypoints;
    Vector3 curWaypointPos;
    Vector3 nextWaypointPos;

    [Header("Gravity")]
    public float gravitySpeed = 10f;
    public float gravityMaxSpeed = 15f;
    public float gravityRayCastLength = 0.6f;
    public bool isReverse = false;
    int gravitySign = 1; // down

    [Header("Bounce")]
    public float bounceRayCastLength = 0.5f;

    // Components
    Rigidbody2D rb;
    SpriteRenderer sr;
    BoxCollider2D col;
    SoundManager sm;

    private KeyCode[] keyCodes = new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E };

    RaycastHit2D hitLeft;
    RaycastHit2D hitRight;
    RaycastHit2D hitUp;
    RaycastHit2D hitDown;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        playerState = PlayerState.ACTIVE;
        ChangeMoveState(moveState);
        grid = GameObject.Find("Grid").GetComponent<Grid>();
        sm = SoundManager.instance;
    }

    void Update()
    {
        // Reverse
        if (reverseCount < reverseCooldown)
        {
            reverseCount += Time.deltaTime;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R))
            {
                ReverseMoveState();
                reverseCount = 0f;
            }
        }

        if (stateChangeCount < stateChangeCooldown)
        {
            stateChangeCount += Time.deltaTime;
        }
        else
        {
            for (int i = 0; i < keyCodes.Length; ++i)
            {
                if (Input.GetKey(keyCodes[i]))
                {
                    ChangeMoveState((MoveState)i);
                    stateChangeCount = 0f;
                    break;
                }
            }
        }

    }

    void ReverseMoveState()
    {
        switch (moveState)
        {
            case (MoveState.WALLRUN):
                sm.PlaySound("Reverse");
                isClockwise = !isClockwise;
                curWaypoints.SetClockwise(isClockwise);
                Vector3 tmp = curWaypointPos;
                curWaypointPos = nextWaypointPos;
                nextWaypointPos = tmp;
                break;
            case (MoveState.GRAVITY):
                sm.PlaySound("Reverse");
                gravitySign *= -1;
                break;
            case (MoveState.BOUNCE):
                rb.velocity = -rb.velocity;
                break;

        }
    }

    void FixedUpdate()
    {
        hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckRayCastLength, 1 << WALL_LAYER);
        hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckRayCastLength, 1 << WALL_LAYER);
        hitUp = Physics2D.Raycast(transform.position, Vector2.up, wallCheckRayCastLength, 1 << WALL_LAYER);
        hitDown = Physics2D.Raycast(transform.position, Vector2.down, wallCheckRayCastLength, 1 << WALL_LAYER);  

        if (hitLeft)
        {
            transform.position = new Vector3(transform.position.x + Time.fixedDeltaTime, transform.position.y, transform.position.z);
        }

        if (hitRight)
        {
            transform.position = new Vector3(transform.position.x - Time.fixedDeltaTime, transform.position.y, transform.position.z);
        }

        if (hitDown)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + Time.fixedDeltaTime, transform.position.z);
        }

        if (hitUp)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - Time.fixedDeltaTime, transform.position.z);
        }

        switch (moveState)
        {
            case (MoveState.WALLRUN):
                UpdateWallRun();
                break;
            case (MoveState.GRAVITY):
                UpdateGravity();
                break;
            case (MoveState.BOUNCE):
                UpdateBounce();
                break;
        }
    }

    void ChangeMoveState(MoveState _state)
    {
        if (moveState == _state)
        {
            return;
        }

        moveState = _state;
        sr.sprite = sprites[(int)moveState];

        switch (_state)
        {
            case (MoveState.WALLRUN):
                sm.PlaySound("WallRun");
                isClockwise = false;
                startWallRun = false;
                break;
            case (MoveState.GRAVITY):
                sm.PlaySound("Gravity");
                gravitySign = 1;
                break;
            case (MoveState.BOUNCE):
                sm.PlaySound("Bounce");
                break;
        }
    }

    public void ChangePlayerState(PlayerState _state)
    {
        switch (_state)
        {
            case (PlayerState.DEAD):
                playerState = _state;
                rb.bodyType = RigidbodyType2D.Static;
                break;
        }
    }
    ////////// WALL RUN //////////
    void UpdateWallRun()
    {
        // Check if the center is within the waypoint collider
        // RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, wallRunRayCastLength, 1 << WAYPOINTS_LAYER);
        RaycastHit2D hitWaypoint = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, 0f, 1 << WAYPOINTS_LAYER);
        RaycastHit2D hitWall = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, 0f, 1 << WALL_LAYER);

        RaycastHit2D wallRunHitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallRunRayCastLength, 1 << WALL_LAYER);
        RaycastHit2D wallRunHitRight = Physics2D.Raycast(transform.position, Vector2.right, wallRunRayCastLength, 1 << WALL_LAYER);
        RaycastHit2D wallRunHitUp = Physics2D.Raycast(transform.position, Vector2.up, wallRunRayCastLength, 1 << WALL_LAYER);
        RaycastHit2D wallRunHitDown = Physics2D.Raycast(transform.position, Vector2.down, wallRunRayCastLength, 1 << WALL_LAYER);  

        // If not running then latch on
        if (hitWaypoint && !startWallRun && (hitWall || (wallRunHitLeft || wallRunHitRight || wallRunHitUp || wallRunHitDown)))
        {
            // isClockwise = false as default
            if (wallRunHitUp)
            {
                if (rb.velocity.x > 0.1f || wallRunHitLeft)
                {
                    isClockwise = true;
                }
            }

            else if (wallRunHitDown)
            {
                if (rb.velocity.x < -0.1f || wallRunHitRight)
                {
                    isClockwise = true;
                }
            }

            else if (wallRunHitLeft)
            {
                if (rb.velocity.y > 0.1f || wallRunHitDown)
                {
                    isClockwise = true;
                }
            }

            else if (wallRunHitRight)
            {
                if (rb.velocity.y < -0.1f || wallRunHitUp)
                {
                    isClockwise = true;
                }
            }

            WaypointCollider wpcol = hitWaypoint.transform.gameObject.GetComponent<WaypointCollider>();
            curWaypoints = hitWaypoint.transform.parent.GetComponent<Waypoints>();

            if (isClockwise)
            {
                curWaypoints.SetCurrentPath(wpcol.nextIndex, isClockwise);
                curWaypointPos = curWaypoints.GetWaypointPositionAt(wpcol.nextIndex);
            }
            else
            {
                curWaypoints.SetCurrentPath(wpcol.index, isClockwise);
                curWaypointPos = curWaypoints.GetWaypointPositionAt(wpcol.index);
            }

            nextWaypointPos = curWaypoints.GenerateNextWaypointPosition();

            // Snap to grid
            Vector3Int cellPosition = grid.WorldToCell(transform.position);
            transform.position = grid.GetCellCenterWorld(cellPosition);

            if (rb.velocity.magnitude < 0.1f)
            {
                curWallRunSpeed = Mathf.Max(Mathf.Abs(rb.velocity.magnitude), defaultWallRunSpeed);
            }
            else
            {
                // Transfer speed from previous state
                curWallRunSpeed = Mathf.Max(Mathf.Abs(rb.velocity.magnitude), minWallRunSpeed);
                curWallRunSpeed = Mathf.Min(curWallRunSpeed, maxWallRunSpeed);
            }

            startWallRun = true;
        }

        // While running 
        if (startWallRun)
        {
            float dist = Vector2.Distance(transform.position, nextWaypointPos);
            if (dist < waypointDistThreshold)
            {
                curWaypointPos = nextWaypointPos;
                nextWaypointPos = curWaypoints.GenerateNextWaypointPosition();
                transform.position = curWaypointPos;
            }

            Vector2 dir = (nextWaypointPos - transform.position).normalized;
            rb.velocity = dir * curWallRunSpeed;
        }
    }

    ////////// GRAVITY //////////
    void UpdateGravity()
    {
        // Handle hitting walls other than the "ground"
        if (hitLeft || hitRight)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        if ((hitUp && gravitySign == 1) || (hitDown && gravitySign == -1))
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        // Landed
        RaycastHit2D gravityHitDown = Physics2D.Raycast(transform.position, gravitySign * Vector2.down, gravityRayCastLength, 1 << WALL_LAYER);
        if (gravityHitDown)
        {
            rb.velocity = Vector2.zero;
        }
        // Falling
        else
        {
            float gravityVelocity = rb.velocity.y - gravitySign * gravitySpeed * Time.fixedDeltaTime;
            float sign = Mathf.Sign(gravityVelocity);
            gravityVelocity = Mathf.Min(Mathf.Abs(gravityVelocity), gravityMaxSpeed);

            rb.velocity = new Vector2(rb.velocity.x, sign * gravityVelocity);
        }
    }

    ////////// BOUNCE //////////
    void UpdateBounce()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rb.velocity.normalized, bounceRayCastLength, 1 << WALL_LAYER);
        if (hit)
        {
            Vector2 reflectDir = Vector2.Reflect(rb.velocity.normalized, hit.normal).normalized;
            rb.velocity = reflectDir * rb.velocity.magnitude;
        }
    }

    void OnDrawGizmos()
    {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + gravitySign * Vector2.down * wallCheckRayCastLength);
        Gizmos.DrawLine(pos, pos + Vector2.left * wallCheckRayCastLength);
        Gizmos.DrawLine(pos, pos + Vector2.right * wallCheckRayCastLength);
        Gizmos.DrawLine(pos, pos + gravitySign * Vector2.up * wallCheckRayCastLength);

        if (moveState == MoveState.WALLRUN)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + Vector2.down * wallRunRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.left * wallRunRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.right * wallRunRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.up * wallRunRayCastLength);
        }
        else if (moveState == MoveState.GRAVITY)
        {
            Gizmos.color = Color.red;

        }
        else if (moveState == MoveState.BOUNCE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + rb.velocity.normalized * bounceRayCastLength);
        }

    }
}
