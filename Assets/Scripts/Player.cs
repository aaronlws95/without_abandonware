using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum MoveState
    {
        WALLRUN,
        GRAVITY
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

    // Constants
    int WALL_LAYER = 1 << 6;
    float PLAYER_SIZE = 1f;

    public Sprite[] sprites;
    public float minSpeed = 5f;
    public float maxSpeed = 15f;

    [Header("Wall Run")]
    public float wallRunSpeed = 5f;
    public float rayCastLength = 0.8f;
    public MoveState state;
    public WallRunMoveDir wallRunMoveDir;
    public WallRunDirection wallRunDirection;
    bool waitForNextCheck = false; // wait for next check when moving on rectangular blocks
    bool startWallRun = false;


    [Header("Gravity")]
    public float gravitySpeed = 10f;

    // Components
    Rigidbody2D rb;
    SpriteRenderer sr;

    // Debugging
    // Vector3 colContactPoint;

    private KeyCode[] keyCodes = new KeyCode[] { KeyCode.Q, KeyCode.W };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ChangeState(state);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            wallRunDirection = GetOppositeRunDir(wallRunDirection);
            wallRunMoveDir = GetOppositeMoveDir(wallRunMoveDir);
        }

        // Debug.Log(state);
        for (int i = 0; i < keyCodes.Length; ++i)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                ChangeState((MoveState)i);
            }
        }
    }

    void FixedUpdate()
    {
        // WALL RUN
        // Debug.Log(rb.velocity);
        switch (state)
        {
            case (MoveState.WALLRUN):
                UpdateWallRun();
                break;
            case (MoveState.GRAVITY):
                UpdateGravity();
                break;
        }
    }

    void ChangeState(MoveState _state)
    {
        switch (_state)
        {
            case (MoveState.WALLRUN):
                state = MoveState.WALLRUN;
                startWallRun = false;
                sr.sprite = sprites[0];
                break;
            case (MoveState.GRAVITY):
                state = MoveState.GRAVITY;
                sr.sprite = sprites[1];
                break;
        }
    }

    ////////// WALL RUN //////////
    void UpdateWallRun()
    {
        // Entering wall run
        if (!startWallRun)
        {
            RaycastHit2D checkUp = Physics2D.Raycast(transform.position, Vector2.up, rayCastLength, WALL_LAYER);
            RaycastHit2D checkDown = Physics2D.Raycast(transform.position, Vector2.down, rayCastLength, WALL_LAYER);
            RaycastHit2D checkLeft = Physics2D.Raycast(transform.position, Vector2.left, rayCastLength, WALL_LAYER);
            RaycastHit2D checkRight = Physics2D.Raycast(transform.position, Vector2.right, rayCastLength, WALL_LAYER);

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
        RaycastHit2D checkForward = Physics2D.Raycast(transform.position, forwardVector, rayCastLength, WALL_LAYER);

        WallRunMoveDir nextMoveDir = GetNextMoveDir(dir, moveDir);
        WallRunMoveDir downMoveDir = GetOppositeMoveDir(nextMoveDir);
        Vector2 downwardVector = GetDirectionVector(downMoveDir);
        RaycastHit2D checkDown = Physics2D.Raycast(transform.position, downwardVector, rayCastLength, WALL_LAYER);

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
            if (Vector2.Distance(checkForward.point, transform.position) < PLAYER_SIZE / 2)
            {
                int signModifier = dir == WallRunDirection.CCW ? 1 : -1;
                rb.velocity = -signModifier * Vector2.Perpendicular(checkForward.normal).normalized * wallRunSpeed;
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
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - gravitySpeed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (state == MoveState.GRAVITY)
        {
            colContactPoint = col.contacts[0].point;
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * rayCastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayCastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * rayCastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * rayCastLength);
    }
}
