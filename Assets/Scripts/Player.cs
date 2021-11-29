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
        DEAD,
        WIN,
    }

    // Constants
    int WALL_LAYER = 6;
    int WAYPOINTS_LAYER = 7;

    [Header("General")]
    Grid grid;
    public ParticleSystem wallRunPS;
    public ParticleSystem gravityPS;
    public ParticleSystem bouncePS;
    public Sprite[] sprites;
    public MoveState moveState;
    public PlayerState playerState;
    public float stateChangeCooldown = 0f;
    public float reverseCooldown = 0f;
    float wallCheckRayCastLength = 0.51f;
    float velocityThreshold = 0.1f;
    float stateChangeCount = 0f;
    float reverseCount = 0f;
    public bool reverseDisabled = false;

    [Header("Wall Run")]
    public float defaultWallRunSpeed = 10f;
    public float minWallRunSpeed = 10f;
    public float maxWallRunSpeed = 15f;
    public float curWallRunSpeed = 10f;
    public float wallRunVelocityDampening = 1f;
    float waypointDistThreshold = 0.2f;
    float wallRunRayCastLength = 0.6f;
    bool isClockwise = false;
    bool startWallRun = false;
    Waypoints curWaypoints;
    Vector3 curWaypointPos;
    Vector3 nextWaypointPos;

    [Header("Gravity")]
    public bool gravityDisabled = false;
    public float gravitySpeed = 10f;
    public float gravityMaxSpeed = 15f;
    float gravityRayCastLength = 0.53f;
    float gravityDownSlopeRayCastLength = 1.0f;
    float gravityCannotStopRayCastLength = 1.2f;
    int gravitySign = 1; // down
    bool canStop = false;
    float activeGravityRayCastLength;

    [Header("Bounce")]
    public bool bounceDisabled = false;
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

    bool isReversing = false;

    public bool isReverseCooldown = false;
    public GameObject arrow;

    bool startReverse = false;

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
        arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(rb.velocity.normalized.y, rb.velocity.normalized.x) * Mathf.Rad2Deg - 90f));

         if (Input.GetKey(KeyCode.Y))
        {
            isReverseCooldown = !isReverseCooldown;
        }

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
            if (reverseCount < reverseCooldown && moveState == MoveState.BOUNCE && isReverseCooldown && startReverse)
            {
                reverseCount += Time.deltaTime;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R) && !reverseDisabled)
                {
                    isReversing = true;
                    reverseCount = 0f;
                    startReverse = true;
                }
                else 
                {
                    startReverse = false;
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
                wallRunPS.Play();
                isClockwise = !isClockwise;
                Vector3 tmp = curWaypointPos;
                curWaypointPos = nextWaypointPos;
                curWaypoints.SetCurrentPath(curWaypoints.GetNextIndex(curWaypoints.getCurrentIndex(), isClockwise), isClockwise);
                nextWaypointPos = tmp;
                break;
            // case (MoveState.GRAVITY):
            //     gravitySign *= -1;
            //     break;
            case (MoveState.BOUNCE):
                sm.PlaySound("Reverse");
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
                if (moveState != MoveState.BOUNCE)
                {
                    transform.position = new Vector3(transform.position.x + Time.fixedDeltaTime, transform.position.y, transform.position.z);
                }
                wallRunPS.transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z);

            }

            if (hitRight)
            {
                if (moveState != MoveState.BOUNCE)
                {
                    transform.position = new Vector3(transform.position.x - Time.fixedDeltaTime, transform.position.y, transform.position.z);
                }
                wallRunPS.transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z);
            }

            if (hitDown)
            {
                if (moveState != MoveState.BOUNCE)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y + Time.fixedDeltaTime, transform.position.z);
                }
                wallRunPS.transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
            }

            if (hitUp)
            {
                if (moveState != MoveState.BOUNCE)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y - Time.fixedDeltaTime, transform.position.z);
                }
                wallRunPS.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            }

            if (isReversing)
            {
                ReverseMoveState();
                isReversing = false;
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


        switch (_state)
        {
            case (MoveState.WALLRUN):
                moveState = _state;
                sr.sprite = sprites[(int)moveState];            
                sm.PlaySound("WallRun");
                wallRunPS.Play();
                isClockwise = false;
                startWallRun = false;
                break;
            case (MoveState.GRAVITY):
                if (!gravityDisabled)
                {
                    moveState = _state;
                    sr.sprite = sprites[(int)moveState];                       
                    sm.PlaySound("Gravity");
                    gravityPS.Play();
                    gravitySign = 1;
                    canStop = false;
                    activeGravityRayCastLength = gravityRayCastLength;
                }
                break;
            case (MoveState.BOUNCE):
                if (!bounceDisabled)
                {
                    moveState = _state;
                    sr.sprite = sprites[(int)moveState];               
                    sm.PlaySound("Bounce");
                }
                break;
        }
    }

    public void ChangePlayerState(PlayerState _state)
    {
        if (playerState == _state)
        {
            return;
        }

        playerState = _state;
        switch (_state)
        {
            case (PlayerState.DEAD):
                rb.bodyType = RigidbodyType2D.Static;
                break;
            case (PlayerState.WIN):
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
            // if (rb.velocity.magnitude > velocityThreshold)
            // {
            //     if (wallRunHitUp)
            //     {
            //         if (rb.velocity.x > velocityThreshold || wallRunHitLeft)
            //         {
            //             isClockwise = true;
            //         }
            //     }

            //     else if (wallRunHitDown)
            //     {
            //         if (rb.velocity.x < -velocityThreshold || wallRunHitRight)
            //         {
            //             isClockwise = true;
            //         }
            //     }

            //     else if (wallRunHitLeft)
            //     {
            //         if (rb.velocity.y > velocityThreshold || wallRunHitDown)
            //         {
            //             isClockwise = true;
            //         }
            //     }

            //     else if (wallRunHitRight)
            //     {
            //         if (rb.velocity.y < -velocityThreshold || wallRunHitUp)
            //         {
            //             isClockwise = true;
            //         }
            //     }
            // }

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

            // get nearest waypoint index

            int nearestWaypointIndex = wpcol.index;

            var curDist = Vector2.Distance(transform.position, wpcol.curWaypointPos);
            var nextDist = Vector2.Distance(transform.position, wpcol.nextWaypointPos);
            var prevDist = Vector2.Distance(transform.position, wpcol.prevWaypointPos);

            RaycastHit2D curHit = Physics2D.Raycast(transform.position, wpcol.curWaypointPos - transform.position, curDist, 1 << WALL_LAYER);
            RaycastHit2D nextHit = Physics2D.Raycast(transform.position, wpcol.nextWaypointPos - transform.position, nextDist, 1 << WALL_LAYER);
            RaycastHit2D prevHit = Physics2D.Raycast(transform.position, wpcol.prevWaypointPos - transform.position, prevDist, 1 << WALL_LAYER);

            if (curDist < nextDist && curDist < prevDist && !curHit)
            {
                nearestWaypointIndex = wpcol.index;
            }
            else if (nextDist < curDist && nextDist < prevDist && !nextHit)
            {
                nearestWaypointIndex = wpcol.nextIndex;
            }
            else if (prevDist < curDist && prevDist < nextDist && !prevHit)
            {
                nearestWaypointIndex = wpcol.prevIndex;
            }

            // Debug.Log(nearestWaypointIndex);

            int nextIndex = curWaypoints.GetNextIndex(nearestWaypointIndex, false);
            int prevIndex = curWaypoints.GetNextIndex(nearestWaypointIndex, true);

            var thisNextWaypointPos = curWaypoints.GetWaypointPositionAt(nextIndex);
            var prevWaypointPos = curWaypoints.GetWaypointPositionAt(prevIndex);
            var curCurWaypointPos = curWaypoints.GetWaypointPositionAt(nearestWaypointIndex);

            Vector3 dirNext = (thisNextWaypointPos - curCurWaypointPos).normalized;
            Vector3 dirPrev = (prevWaypointPos - curCurWaypointPos).normalized;

            if (Vector2.Dot(dirNext, rb.velocity.normalized) > Vector2.Dot(dirPrev, rb.velocity.normalized)) 
            {
                isClockwise = false;
            }
            else 
            {
                isClockwise = true;
            }

            // Debug.Log(isClockwise);

            // near enough to current waypoint
            if(Vector2.Distance(transform.position, curCurWaypointPos) < waypointDistThreshold)
            {
                curWaypoints.SetCurrentPath(nearestWaypointIndex, isClockwise);
                curWaypointPos = curWaypoints.GetWaypointPositionAt(nearestWaypointIndex);
            }
            else
            {
                if (isClockwise)
                {
                    RaycastHit2D checkHitPlayer = Physics2D.Raycast(curCurWaypointPos, prevWaypointPos - curCurWaypointPos, Vector2.Distance(curCurWaypointPos, prevWaypointPos), 1 << 8);
                    RaycastHit2D checkHitWall = Physics2D.Raycast(curCurWaypointPos, prevWaypointPos - curCurWaypointPos, Vector2.Distance(curCurWaypointPos, prevWaypointPos), 1 << 6);
                    int setIndex = 0;
                    if (checkHitPlayer && !checkHitWall)
                    {
                        setIndex = nearestWaypointIndex;
                    }
                    else 
                    {
                        setIndex = nextIndex;
                    }
                    curWaypoints.SetCurrentPath(setIndex, isClockwise);
                    curWaypointPos = curWaypoints.GetWaypointPositionAt(setIndex);
                }
                else
                {
                    RaycastHit2D checkHitPlayer = Physics2D.Raycast(curCurWaypointPos, thisNextWaypointPos - curCurWaypointPos, Vector2.Distance(curCurWaypointPos, thisNextWaypointPos), 1 << 8);
                    RaycastHit2D checkHitWall = Physics2D.Raycast(curCurWaypointPos, thisNextWaypointPos - curCurWaypointPos, Vector2.Distance(curCurWaypointPos, thisNextWaypointPos), 1 << 6);
                    int setIndex = 0;
                    if (checkHitPlayer && !checkHitWall)
                    {
                        setIndex = nearestWaypointIndex;
                    }
                    else 
                    {
                        setIndex = prevIndex;
                    }
                    curWaypoints.SetCurrentPath(setIndex, isClockwise);
                    curWaypointPos = curWaypoints.GetWaypointPositionAt(setIndex);
                }                
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
                curWallRunSpeed = Mathf.Max(Mathf.Abs(rb.velocity.magnitude) * wallRunVelocityDampening, minWallRunSpeed);
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

            // Debug.Log(nextWaypointPos);
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
            if ((hitLeft && rb.velocity.x < -velocityThreshold) || (hitRight && rb.velocity.x > velocityThreshold))
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            if (rb.velocity.magnitude < velocityThreshold)
            {
                sm.PlaySound("GravityStop");
            }
            canStop = false;
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

        if (rb.velocity.magnitude > velocityThreshold)
        {
            Vector2 velocityDir = rb.velocity.normalized;
            gravityPS.transform.position = new Vector3(transform.position.x - velocityDir.x * 0.5f, transform.position.y - velocityDir.y * 0.5f, transform.position.z);
            gravityPS.Play();
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
            bouncePS.transform.position = new Vector3(hit.point.x, hit.point.y, transform.position.z);
            bouncePS.Play();
            sm.PlaySound("BounceCollide");
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

    public Vector2 GetVelocity()
    {
        return rb.velocity;
    }
}
