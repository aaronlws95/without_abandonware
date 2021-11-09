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

    [Header("Wall Run")]
    public float minWallRunSpeed = 10f;
    public float maxWallRunSpeed = 15f;    
    public float curWallRunSpeed = 10f;
    public float wallRunRayCastLength = 0.01f;
    public float wallRunRayCastGroundLength = 0.6f;
    public float waypointDistThreshold = 0.7f;
    public bool isClockwise = false;
    bool startWallRun = false;
    Waypoints curWaypoints;
    Vector3 curWaypointPos;
    Vector3 nextWaypointPos;

    [Header("Gravity")]
    public float gravitySpeed = 10f;
    public float gravityRayCastLength = 1.0f;
    public float gravityMaxSpeed = 15f;
    public float gravityFallingRayCastLength = 0.5f;
    public float gravitySideWallRayCastLength = 0.5f;
    public float gravityGroundOffset = 0.5f;
    float activeGravityRayCastLength;
    public bool isReverse = false;
    int gravitySign = 1;
    bool isLanded = false;
    bool isFalling = false;

    [Header("Bounce")]
    public float bounceRayCastLength = 0.5f;

    // Components
    Rigidbody2D rb;
    SpriteRenderer sr;
    BoxCollider2D col;
    SoundManager sm;

    private KeyCode[] keyCodes = new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E };

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
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReverseMoveState();
        }

        for (int i = 0; i < keyCodes.Length; ++i)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                ChangeMoveState((MoveState)i);
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
                isLanded = false;
                gravitySign *= -1;
                break;
            case (MoveState.BOUNCE):
                rb.velocity = -rb.velocity;
                break;

        }
    }

    void FixedUpdate()
    {
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
                isFalling = false;
                isLanded = false;
                gravitySign = 1;
                activeGravityRayCastLength = gravityRayCastLength;
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, wallRunRayCastLength, 1 << WAYPOINTS_LAYER);
        // RaycastHit2D hit = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, 0f, 1 << WAYPOINTS_LAYER);

        // Check if any part is touching a wall
        RaycastHit2D checkUp = Physics2D.Raycast(transform.position, Vector2.up, wallRunRayCastGroundLength, 1 << WALL_LAYER);
        RaycastHit2D checkDown = Physics2D.Raycast(transform.position, Vector2.down, wallRunRayCastGroundLength, 1 << WALL_LAYER);
        RaycastHit2D checkLeft = Physics2D.Raycast(transform.position, Vector2.left, wallRunRayCastGroundLength, 1 << WALL_LAYER);
        RaycastHit2D checkRight = Physics2D.Raycast(transform.position, Vector2.right, wallRunRayCastGroundLength, 1 << WALL_LAYER);

        // If not running then latch on
        if (hit && !startWallRun && (checkUp || checkDown || checkLeft || checkRight))
        {
            // isClockwise = false as default
            if (checkUp)
            {
                if (rb.velocity.x > 0.1f)
                {
                    isClockwise = true;
                }
            }

            else if (checkDown)
            {
                if (rb.velocity.x < -0.1f)
                {
                    isClockwise = true;
                }
            }

            else if (checkLeft)
            {
                if (rb.velocity.y > 0.1f)
                {
                    isClockwise = true;
                }
            }

            else if (checkRight)
            {
                if (rb.velocity.y < -0.1f)
                {
                    isClockwise = true;
                }
            }

            WaypointCollider wpcol = hit.transform.gameObject.GetComponent<WaypointCollider>();
            curWaypoints = hit.transform.parent.GetComponent<Waypoints>();

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

            // Transfer speed from previous state
            curWallRunSpeed = Mathf.Max(Mathf.Abs(rb.velocity.magnitude), minWallRunSpeed);
            curWallRunSpeed = Mathf.Min(curWallRunSpeed, maxWallRunSpeed);

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

            Vector2 dir = (nextWaypointPos - curWaypointPos).normalized;
            rb.velocity = dir * curWallRunSpeed;
        }
    }

    ////////// GRAVITY //////////
    void UpdateGravity()
    {
        // Set the falling ray cast length to be lower
        // Needs to be longer to catch slopes but shorter so it doesn't detect the ground too fast
        if (isFalling)
        {
            activeGravityRayCastLength = gravityFallingRayCastLength;
        }

        // Handle hitting walls other than the "ground"
        RaycastHit2D hitLeftWall = Physics2D.Raycast(transform.position, Vector2.left, gravitySideWallRayCastLength, 1 << WALL_LAYER);
        RaycastHit2D hitRightWall = Physics2D.Raycast(transform.position, Vector2.right, gravitySideWallRayCastLength, 1 << WALL_LAYER);
        RaycastHit2D hitUp = Physics2D.Raycast(transform.position, gravitySign * Vector2.up, gravitySideWallRayCastLength, 1 << WALL_LAYER);
        if (hitLeftWall || hitRightWall)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        if (hitUp)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, gravitySign * Vector2.down, activeGravityRayCastLength, 1 << WALL_LAYER);

        if (hitGround)
        {
            // Wait until it is falling before grounding
            // This allows the player to get a jump off instead of getting stuck
            if (isFalling)
            {
                activeGravityRayCastLength = gravityRayCastLength;
                rb.velocity = Vector2.zero;
                transform.position = new Vector3(hitGround.point.x, hitGround.point.y + gravitySign * gravityGroundOffset, transform.position.z);
                // Vector3Int cellPosition = grid.WorldToCell(transform.position);
                // transform.position = grid.GetCellCenterWorld(cellPosition);
                isLanded = true;
                isFalling = false;
            }
            else 
            {
                // Deal with moving down slopes
                if ((gravitySign == 1 && rb.velocity.y < -0.1f) || (gravitySign == -1 && rb.velocity.y > 0.1f))
                {
                    isFalling = true;
                }
            }
        }
        else
        {
            isFalling = true;
        }
        if (!isLanded && isFalling)
        {
            float gravityVelocity = rb.velocity.y - gravitySign * gravitySpeed * Time.fixedDeltaTime;
            float sign = Mathf.Sign(gravityVelocity);
            gravityVelocity = Mathf.Min(Mathf.Abs(gravityVelocity), gravityMaxSpeed);

            rb.velocity = new Vector2(rb.velocity.x, sign*gravityVelocity);
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

        if (moveState == MoveState.WALLRUN)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pos, pos + Vector2.down * wallRunRayCastLength);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + Vector2.down * wallRunRayCastGroundLength);
            Gizmos.DrawLine(pos, pos + Vector2.left * wallRunRayCastGroundLength);
            Gizmos.DrawLine(pos, pos + Vector2.right * wallRunRayCastGroundLength);
            Gizmos.DrawLine(pos, pos + Vector2.up * wallRunRayCastGroundLength);
        }
        else if (moveState == MoveState.GRAVITY)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + gravitySign * Vector2.down * activeGravityRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.left * activeGravityRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.right * activeGravityRayCastLength);
            Gizmos.DrawLine(pos, pos + gravitySign * Vector2.up * activeGravityRayCastLength);
        }
        else if (moveState == MoveState.BOUNCE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + rb.velocity.normalized * bounceRayCastLength);
        }

    }
}
