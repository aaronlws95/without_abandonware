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
        INIT,
        ACTIVE,
        DEAD
    }

    // Constants
    int WALL_LAYER = 6;
    int WAYPOINTS_LAYER = 7;

    [Header("General")]
    Grid grid;
    public ParticleSystem ps;
    public Sprite[] sprites;
    public MoveState moveState;
    public PlayerState playerState;
    public float stateChangeCooldown = 0f;
    public float reverseCooldown = 0f;
    float wallCheckRayCastLength = 0.51f;
    float velocityThreshold = 0.1f;
    float stateChangeCount = 0f;
    float reverseCount = 0f;

    [Header("Wall Run")]
    public float defaultWallRunSpeed = 10f;
    public float minWallRunSpeed = 10f;
    public float maxWallRunSpeed = 15f;
    public float curWallRunSpeed = 10f;
    float waypointDistThreshold = 0.2f;
    float wallRunRayCastLength = 0.6f;
    bool isClockwise = false;
    bool startWallRun = false;
    Waypoints curWaypoints;
    Vector3 curWaypointPos;
    Vector3 nextWaypointPos;

    [Header("Gravity")]
    public float gravitySpeed = 10f;
    public float gravityMaxSpeed = 15f;
    float gravityRayCastLength = 0.6f;
    float gravityDownSlopeRayCastLength = 1.0f;
    float gravityCannotStopRayCastLength = 1.2f;
    int gravitySign = 1; // down
    bool canStop = false;
    float activeGravityRayCastLength;

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
        playerState = PlayerState.INIT;
        ChangeMoveState(moveState);
        grid = GameObject.Find("Grid").GetComponent<Grid>();
        sm = SoundManager.instance;
    }

    void Update()
    {
        if (playerState == PlayerState.INIT)
        {
            if (Input.anyKey && !Input.GetKey(KeyCode.Escape))
            {
                playerState = PlayerState.ACTIVE;
            }
        }
        else if (playerState == PlayerState.ACTIVE)
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
    }

    void ReverseMoveState()
    {
        switch (moveState)
        {
            case (MoveState.WALLRUN):
                sm.PlaySound("Reverse");
                ps.Play();
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
        if (playerState == PlayerState.ACTIVE)
        {
            hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckRayCastLength, 1 << WALL_LAYER);
            hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckRayCastLength, 1 << WALL_LAYER);
            hitUp = Physics2D.Raycast(transform.position, Vector2.up, wallCheckRayCastLength, 1 << WALL_LAYER);
            hitDown = Physics2D.Raycast(transform.position, Vector2.down, wallCheckRayCastLength, 1 << WALL_LAYER);  

            if (hitLeft)
            {
                transform.position = new Vector3(transform.position.x + Time.fixedDeltaTime, transform.position.y, transform.position.z);
                ps.transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z);

            }

            if (hitRight)
            {
                transform.position = new Vector3(transform.position.x - Time.fixedDeltaTime, transform.position.y, transform.position.z);
                ps.transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z);
            }

            if (hitDown)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + Time.fixedDeltaTime, transform.position.z);
                ps.transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
            }

            if (hitUp)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - Time.fixedDeltaTime, transform.position.z);
                ps.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
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
                ps.Play();
                isClockwise = false;
                startWallRun = false;
                break;
            case (MoveState.GRAVITY):
                sm.PlaySound("Gravity");
                gravitySign = 1;
                canStop = false;
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
        RaycastHit2D hitWaypointCenter = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, 1 << WAYPOINTS_LAYER);
        RaycastHit2D hitWaypoint = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, 0f, 1 << WAYPOINTS_LAYER);
        RaycastHit2D hitWall = Physics2D.BoxCast(col.bounds.center, col.bounds.size, 0f, Vector2.down, 0f, 1 << WALL_LAYER);

        RaycastHit2D wallRunHitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallRunRayCastLength, 1 << WALL_LAYER);
        RaycastHit2D wallRunHitRight = Physics2D.Raycast(transform.position, Vector2.right, wallRunRayCastLength, 1 << WALL_LAYER);
        RaycastHit2D wallRunHitUp = Physics2D.Raycast(transform.position, Vector2.up, wallRunRayCastLength, 1 << WALL_LAYER);
        RaycastHit2D wallRunHitDown = Physics2D.Raycast(transform.position, Vector2.down, wallRunRayCastLength, 1 << WALL_LAYER);  

        // If not running then latch on
        if ((hitWaypoint || hitWaypointCenter) && !startWallRun && (hitWall || (wallRunHitLeft || wallRunHitRight || wallRunHitUp || wallRunHitDown)))
        {
            // isClockwise = false as default
            if (rb.velocity.magnitude > velocityThreshold)
            {
                if (wallRunHitUp)
                {
                    if (rb.velocity.x > velocityThreshold || wallRunHitLeft)
                    {
                        isClockwise = true;
                    }
                }

                else if (wallRunHitDown)
                {
                    if (rb.velocity.x < -velocityThreshold || wallRunHitRight)
                    {
                        isClockwise = true;
                    }
                }

                else if (wallRunHitLeft)
                {
                    if (rb.velocity.y > velocityThreshold || wallRunHitDown)
                    {
                        isClockwise = true;
                    }
                }

                else if (wallRunHitRight)
                {
                    if (rb.velocity.y < -velocityThreshold || wallRunHitUp)
                    {
                        isClockwise = true;
                    }
                }
            }

            WaypointCollider wpcol;
            if (hitWaypointCenter)
            {
                wpcol = hitWaypointCenter.transform.gameObject.GetComponent<WaypointCollider>();
                curWaypoints = hitWaypointCenter.transform.parent.GetComponent<Waypoints>();
            }
            else 
            {
                wpcol = hitWaypoint.transform.gameObject.GetComponent<WaypointCollider>();
                curWaypoints = hitWaypoint.transform.parent.GetComponent<Waypoints>();                
            }


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
        if (!canStop)
        {
            activeGravityRayCastLength = gravityCannotStopRayCastLength;
        }
        else 
        {
            activeGravityRayCastLength = gravityRayCastLength;
        }
        RaycastHit2D gravityHitDown = Physics2D.Raycast(transform.position, gravitySign * Vector2.down, activeGravityRayCastLength, 1 << WALL_LAYER);

        // Handle hitting walls other than the "ground"
        if ((hitLeft && rb.velocity.x < -velocityThreshold) || (hitRight && rb.velocity.x > velocityThreshold))
        {
            if (gravityHitDown)
            {
                canStop = true;
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }            
        }

        if ((hitUp && gravitySign == 1) || (hitDown && gravitySign == -1))
        {
            if ((rb.velocity.y > velocityThreshold && gravitySign == 1) || (rb.velocity.y < -velocityThreshold && gravitySign == -1))
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
        }

        // Landed
        if (gravityHitDown && canStop)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
        // Falling
        else if (!gravityHitDown)
        {
            float gravityVelocity = rb.velocity.y - gravitySign * gravitySpeed * Time.fixedDeltaTime;
            float sign = Mathf.Sign(gravityVelocity);
            gravityVelocity = Mathf.Min(Mathf.Abs(gravityVelocity), gravityMaxSpeed);

            rb.velocity = new Vector2(rb.velocity.x, sign * gravityVelocity);
            canStop = true;
        }
        // Deal with moving down slopes
        else if ((gravitySign == 1 && rb.velocity.y < -velocityThreshold) || (gravitySign == -1 && rb.velocity.y > velocityThreshold))
        {
            RaycastHit2D gravityHitDownSlope = Physics2D.Raycast(transform.position, rb.velocity.normalized, gravityDownSlopeRayCastLength, 1 << WALL_LAYER);
            if (gravityHitDownSlope)
            {
                canStop = true;
            }
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
        Gizmos.DrawLine(pos, pos + Vector2.down * wallCheckRayCastLength);
        Gizmos.DrawLine(pos, pos + Vector2.left * wallCheckRayCastLength);
        Gizmos.DrawLine(pos, pos + Vector2.right * wallCheckRayCastLength);
        Gizmos.DrawLine(pos, pos + Vector2.up * wallCheckRayCastLength);

        Gizmos.color = Color.red;
        if (moveState == MoveState.WALLRUN)
        {
            Gizmos.DrawLine(pos, pos + Vector2.down * gravityRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.left * gravityRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.right * gravityRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.up * gravityRayCastLength);
        }
        else if (moveState == MoveState.GRAVITY)
        {
            Gizmos.DrawLine(pos, pos + gravitySign * Vector2.down * activeGravityRayCastLength);
            Gizmos.DrawLine(pos, pos + rb.velocity.normalized * gravityDownSlopeRayCastLength);
        }
        else if (moveState == MoveState.BOUNCE)
        {
            Gizmos.DrawLine(pos, pos + rb.velocity.normalized * bounceRayCastLength);
        }

    }
}
