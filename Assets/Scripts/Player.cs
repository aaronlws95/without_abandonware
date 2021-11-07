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

    public enum WallRunMoveDir
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    public enum WallRunDirection
    {
        CW,
        CCW
    }

    public enum PlayerState
    {
        ACTIVE,
        DEAD
    }

    // Constants
    int WALL_LAYER = 6;
    float PLAYER_SIZE = 1f;
    // float PLAYER_DIAGONAL = 1.41f;

    public Sprite[] sprites;
    public float minSpeed = 5f;
    public float maxSpeed = 15f;
    public MoveState moveState;
    public PlayerState playerState;

    [Header("Wall Run")]
    public float wallRunSpeed = 5f;
    public float wallRunRayCastLength = 0.6f;
    public WallRunMoveDir wallRunMoveDir;
    public WallRunDirection wallRunDirection;
    bool waitForNextCheck = false; // wait for next check when moving on rectangular blocks
    bool startWallRun = false;


    [Header("Gravity")]
    public float gravitySpeed = 10f;
    // public float gravityRayCastLength = 0.01f; // for dying
    public float gravityRayCastLength = 0.1f;
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

        // Debug.Log(rb.velocity);
        // Debug.Log(moveState);
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
                wallRunDirection = GetOppositeRunDir(wallRunDirection);
                wallRunMoveDir = GetOppositeMoveDir(wallRunMoveDir);
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

        switch (_state)
        {
            case (MoveState.WALLRUN):
                moveState = _state;
                startWallRun = false;
                sr.sprite = sprites[0];
                break;
            case (MoveState.GRAVITY):
                moveState = _state;
                isGrounded = false;
                sr.sprite = sprites[1];
                break;
            case (MoveState.BOUNCE):
                moveState = _state;
                sr.sprite = sprites[2];
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
        // Entering wall run
        if (!startWallRun)
        {
            RaycastHit2D checkUp = Physics2D.Raycast(transform.position, Vector2.up, wallRunRayCastLength, 1 << WALL_LAYER);
            RaycastHit2D checkDown = Physics2D.Raycast(transform.position, Vector2.down, wallRunRayCastLength, 1 << WALL_LAYER);
            RaycastHit2D checkLeft = Physics2D.Raycast(transform.position, Vector2.left, wallRunRayCastLength, 1 << WALL_LAYER);
            RaycastHit2D checkRight = Physics2D.Raycast(transform.position, Vector2.right, wallRunRayCastLength, 1 << WALL_LAYER);

            // On wall
            if (checkUp || checkDown || checkLeft || checkRight)
            {
                startWallRun = true;
                wallRunDirection = WallRunDirection.CCW;

                if (checkUp)
                {
                    wallRunMoveDir = WallRunMoveDir.LEFT;

                    if (rb.velocity.x > 0.1f)
                    {
                        wallRunDirection = GetOppositeRunDir(wallRunDirection);
                        wallRunMoveDir = GetOppositeMoveDir(wallRunMoveDir);
                    }
                }

                else if (checkDown)
                {
                    wallRunMoveDir = WallRunMoveDir.RIGHT;
                    if (rb.velocity.x < -0.1f)
                    {
                        wallRunDirection = GetOppositeRunDir(wallRunDirection);
                        wallRunMoveDir = GetOppositeMoveDir(wallRunMoveDir);
                    }
                }

                else if (checkLeft)
                {
                    wallRunMoveDir = WallRunMoveDir.DOWN;

                    if (rb.velocity.y > 0.1f)
                    {
                        wallRunDirection = GetOppositeRunDir(wallRunDirection);
                        wallRunMoveDir = GetOppositeMoveDir(wallRunMoveDir);
                    }
                }

                else if (checkRight)
                {
                    wallRunMoveDir = WallRunMoveDir.UP;

                    if (rb.velocity.y < -0.1f)
                    {
                        wallRunDirection = GetOppositeRunDir(wallRunDirection);
                        wallRunMoveDir = GetOppositeMoveDir(wallRunMoveDir);
                    }
                }

                wallRunSpeed = Mathf.Max(Mathf.Abs(rb.velocity.magnitude), minSpeed);
                wallRunSpeed = Mathf.Min(wallRunSpeed, maxSpeed);
            }
        }

        if (startWallRun)
        {
            HandleWallRun(wallRunDirection, wallRunMoveDir);
        }
    }
    void HandleWallRun(WallRunDirection dir, WallRunMoveDir moveDir)
    {
        Vector2 forwardVector = GetDirectionVector(moveDir);
        RaycastHit2D checkForward = Physics2D.Raycast(transform.position, forwardVector, wallRunRayCastLength, 1 << WALL_LAYER);

        WallRunMoveDir nextMoveDir = GetNextMoveDir(dir, moveDir);
        WallRunMoveDir downMoveDir = GetOppositeMoveDir(nextMoveDir);
        Vector2 downwardVector = GetDirectionVector(downMoveDir);
        RaycastHit2D checkDown = Physics2D.Raycast(transform.position, downwardVector, wallRunRayCastLength, 1 << WALL_LAYER);

        float downOffsetX = transform.position.x;
        float downOffsetY = transform.position.y;

        switch (downMoveDir)
        {
            case (WallRunMoveDir.RIGHT):
                downOffsetX = checkDown.point.x - PLAYER_SIZE / 2;
                break;
            case (WallRunMoveDir.UP):
                downOffsetY = checkDown.point.y - PLAYER_SIZE / 2;
                break;
            case (WallRunMoveDir.LEFT):
                downOffsetX = checkDown.point.x + PLAYER_SIZE / 2;
                break;
            case (WallRunMoveDir.DOWN):
                downOffsetY = checkDown.point.y + PLAYER_SIZE / 2;
                break;
        }

        // Moving up
        if (checkDown && checkForward)
        {
            // make sure close enough to wall before moving tangentially
            if (Vector2.Distance(checkForward.point, transform.position) < (PLAYER_SIZE / 2 + 0.1f))
            {
                int signModifier = dir == WallRunDirection.CCW ? 1 : -1;
                rb.velocity = -signModifier * Vector2.Perpendicular(checkForward.normal).normalized * wallRunSpeed;
            }
            else 
            {
                rb.velocity = wallRunSpeed * forwardVector;
                transform.position = new Vector3(downOffsetX, downOffsetY, transform.position.z);                
            }
        }
        // Moving forward
        else if (checkDown && !checkForward)
        {
            rb.velocity = wallRunSpeed * forwardVector;
            transform.position = new Vector3(downOffsetX, downOffsetY, transform.position.z);
        }
        // Not touching anything
        else if (!checkDown && !checkForward && !waitForNextCheck)
        {
            WallRunMoveDir oppNextMoveDir = GetOppositeMoveDir(nextMoveDir);
            rb.velocity = wallRunSpeed * GetDirectionVector(oppNextMoveDir);
            wallRunMoveDir = oppNextMoveDir;
            waitForNextCheck = true;
        }
        // Changing directions
        else if (!checkDown && checkForward)
        {
            wallRunMoveDir = nextMoveDir;
        }

        if (checkDown || checkForward)
        {
            waitForNextCheck = false;
        }
    }
    WallRunMoveDir GetOppositeMoveDir(WallRunMoveDir moveDir)
    {
        switch (moveDir)
        {
            case (WallRunMoveDir.RIGHT):
                return WallRunMoveDir.LEFT;
            case (WallRunMoveDir.UP):
                return WallRunMoveDir.DOWN;
            case (WallRunMoveDir.LEFT):
                return WallRunMoveDir.RIGHT;
            case (WallRunMoveDir.DOWN):
                return WallRunMoveDir.UP;
        }

        return WallRunMoveDir.UP;
    }
    Vector2 GetDirectionVector(WallRunMoveDir moveDir)
    {
        switch (moveDir)
        {
            case (WallRunMoveDir.RIGHT):
                return Vector2.right;
            case (WallRunMoveDir.UP):
                return Vector2.up;
            case (WallRunMoveDir.LEFT):
                return Vector2.left;
            case (WallRunMoveDir.DOWN):
                return Vector2.down;
        }

        return Vector2.up;
    }
    WallRunMoveDir GetNextMoveDir(WallRunDirection dir, WallRunMoveDir moveDir)
    {
        if (dir == WallRunDirection.CCW)
        {
            switch (moveDir)
            {
                case (WallRunMoveDir.RIGHT):
                    return WallRunMoveDir.UP;
                case (WallRunMoveDir.UP):
                    return WallRunMoveDir.LEFT;
                case (WallRunMoveDir.LEFT):
                    return WallRunMoveDir.DOWN;
                case (WallRunMoveDir.DOWN):
                    return WallRunMoveDir.RIGHT;
            }
        }
        else if (dir == WallRunDirection.CW)
        {
            switch (moveDir)
            {
                case (WallRunMoveDir.RIGHT):
                    return WallRunMoveDir.DOWN;
                case (WallRunMoveDir.UP):
                    return WallRunMoveDir.RIGHT;
                case (WallRunMoveDir.LEFT):
                    return WallRunMoveDir.UP;
                case (WallRunMoveDir.DOWN):
                    return WallRunMoveDir.LEFT;
            }
        }

        return WallRunMoveDir.UP;
    }
    WallRunDirection GetOppositeRunDir(WallRunDirection dir)
    {
        if (dir == WallRunDirection.CW)
        {
            return WallRunDirection.CCW;
        }
        else
        {
            return WallRunDirection.CW;
        }
    }

    ////////// GRAVITY //////////
    void UpdateGravity()
    {
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

    void OnCollisionStay2D(Collision2D col)
    {
        if (moveState == MoveState.WALLRUN)
        {
            colContactPoint = col.contacts[0].point;
            Vector2 dir = new Vector2(transform.position.x, transform.position.y) - col.contacts[0].point;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.01f, 1 << WALL_LAYER);
            if (hit)
            {
                transform.position = new Vector3(transform.position.x + col.contacts[0].normal.x, transform.position.y + col.contacts[0].normal.y, transform.position.z);
            }
        }
        else if (moveState == MoveState.GRAVITY)
        {
            if (col.gameObject.layer == WALL_LAYER)
            {
                // colContactPoint = col.contacts[0].point;
                // Vector2 dir = new Vector2(transform.position.x, transform.position.y) - col.contacts[0].point;
                // RaycastHit2D hit = Physics2D.Raycast(col.contacts[0].point + dir * PLAYER_DIAGONAL, -dir.normalized, gravityRayCastLength, 1 << WALL_LAYER);
                // if (hit)
                // {
                //     ChangePlayerState(PlayerState.DEAD);
                // }

                RaycastHit2D hit = Physics2D.Raycast(transform.position, gravitySign*Vector2.down, gravityRayCastLength, 1 << WALL_LAYER);
                if (hit)
                {
                    transform.position = hit.point + gravitySign*Vector2.up*PLAYER_SIZE/2;
                    rb.velocity = Vector2.zero;
                    isGrounded = true;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (moveState == MoveState.WALLRUN)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * wallRunRayCastLength);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * wallRunRayCastLength);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallRunRayCastLength);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallRunRayCastLength);
        }
        else if (moveState == MoveState.GRAVITY)
        {
            Gizmos.color = Color.blue;
            Vector2 pos = new Vector2(transform.position.x, transform.position.y);
            Vector2 dir = pos - colContactPoint;
            // Gizmos.DrawLine(pos, colContactPoint);
            // Gizmos.DrawLine(colContactPoint + dir * PLAYER_DIAGONAL, colContactPoint + dir * PLAYER_DIAGONAL - dir.normalized * 1f);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + gravitySign*Vector2.down*gravityRayCastLength);
        }
        else if (moveState == MoveState.BOUNCE)
        {
            Gizmos.color = Color.green;
            Vector2 pos = new Vector2(transform.position.x, transform.position.y);
            Gizmos.DrawLine(pos, pos + rb.velocity.normalized*bounceRayCastLength);
        }

    }
}
