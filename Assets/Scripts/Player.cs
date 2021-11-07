using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
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
    float PLAYER_SIZE = 1f;

    [Header("General")]
    public Grid grid;
    public Sprite[] sprites;
    public float minSpeed = 5f;
    public float maxSpeed = 15f;
    public MoveState moveState;
    public PlayerState playerState;

    [Header("Wall Run")]
    public float wallRunSpeed = 5f;
    public float wallRunRayCastLength = 0.01f;
    public bool isClockwise = false;
    bool startWallRun = false;
    Waypoints curWaypoints;
    Vector3 curWaypointPos;
    Vector3 nextWaypointPos;

    [Header("Gravity")]
    public float gravitySpeed = 10f;
    public float gravityRayCastLength = 0.6f;
    public bool isReverse = false;
    int gravitySign = 1;
    bool isGrounded = false;

    [Header("Bounce")]
    public float bounceRayCastLength = 0.5f;

    // Components
    Rigidbody2D rb;
    SpriteRenderer sr;

    // Debugging
    Vector2 colContactPoint;
    Vector2 colContactNormal;

    private KeyCode[] keyCodes = new KeyCode[] { KeyCode.Q, KeyCode.W, KeyCode.E };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerState = PlayerState.ACTIVE;
        ChangeMoveState(moveState);
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
                isClockwise = !isClockwise;
                curWaypoints.SetClockwise(isClockwise);
                Vector3 tmp = curWaypointPos;
                curWaypointPos = nextWaypointPos;
                nextWaypointPos = tmp;
                break;
            case (MoveState.GRAVITY):
                isGrounded = false;
                gravitySign *= -1;
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
                isClockwise = false;
                startWallRun = false;
                break;
            case (MoveState.GRAVITY):
                isGrounded = false;
                break;
            case (MoveState.BOUNCE):
                break;
        }
    }

    void ChangePlayerState(PlayerState _state)
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, wallRunRayCastLength, 1 << WAYPOINTS_LAYER);
        if (hit && !startWallRun)
        {
            curWaypoints = hit.transform.parent.transform.parent.GetComponent<Waypoints>();
            curWaypointPos = curWaypoints.HandleArbitraryPosition(transform.position, isClockwise);
            nextWaypointPos = curWaypoints.GetNextWaypointPosition();
            Vector3Int cellPosition = grid.WorldToCell(transform.position);
            transform.position = grid.GetCellCenterWorld(cellPosition);
            startWallRun = true;
        }        

        if (startWallRun)
        {
            if (Vector2.Distance(transform.position, nextWaypointPos) < 0.2f)
            {
                curWaypointPos = nextWaypointPos;
                nextWaypointPos = curWaypoints.GetNextWaypointPosition();
                transform.position = curWaypointPos;
            }
            Vector2 dir = (nextWaypointPos - curWaypointPos).normalized;
            rb.velocity = dir * wallRunSpeed;           
        }
    }

    ////////// GRAVITY //////////
    void UpdateGravity()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, gravitySign*Vector2.down, gravityRayCastLength, 1 << WALL_LAYER);
        if (hitGround)
        {
            rb.velocity = Vector2.zero;
            transform.position = new Vector3(hitGround.point.x, hitGround.point.y + PLAYER_SIZE/2, transform.position.z);
            // Vector3Int cellPosition = grid.WorldToCell(transform.position);
            // transform.position = grid.GetCellCenterWorld(cellPosition);
            isGrounded = true;
        }
        else 
        {
            RaycastHit2D hitLeftWall = Physics2D.Raycast(transform.position, Vector2.left, gravityRayCastLength, 1 << WALL_LAYER);
            RaycastHit2D hitRightWall = Physics2D.Raycast(transform.position, Vector2.right, gravityRayCastLength, 1 << WALL_LAYER);
            if (hitLeftWall || hitRightWall)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }    
        }        
        if (!isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - gravitySign * gravitySpeed * Time.fixedDeltaTime);
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
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + Vector2.down*wallRunRayCastLength);
        }
        else if (moveState == MoveState.GRAVITY)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + gravitySign*Vector2.down*gravityRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.left*gravityRayCastLength);
            Gizmos.DrawLine(pos, pos + Vector2.right*gravityRayCastLength);
        }
        else if (moveState == MoveState.BOUNCE)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + rb.velocity.normalized*bounceRayCastLength);
        }

    }
}
