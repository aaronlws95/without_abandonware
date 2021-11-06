using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    int WALL_LAYER = 1 << 6;
    float PLAYER_HEIGHT = 1f;
    float PLAYER_WIDTH = 1f;

    public float wallRunSpeed = 5f;
    public float rayCastLength = 0.8f;

    public enum MoveState
    {
        WALLRUN
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

    // States
    public MoveState state;
    public WallRunMoveDir wallRunMoveDir;
    public WallRunDirection wallRunDirection;

    RaycastHit2D checkUp;
    RaycastHit2D checkDown;
    RaycastHit2D checkLeft;
    RaycastHit2D checkRight;

    bool waitForNextCheck = false;

    // Components
    Rigidbody2D rb;

    // Debugging
    // Vector3 colContactPoint;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (wallRunDirection == WallRunDirection.CCW)
            {
                wallRunDirection = WallRunDirection.CW;
                wallRunMoveDir = GetOppositeMoveDir(wallRunMoveDir);
            }
            else
            {
                wallRunDirection = WallRunDirection.CCW;
                wallRunMoveDir = GetOppositeMoveDir(wallRunMoveDir);
            }
        }

        // WALL RUN
        // Debug.Log(rb.velocity);
        if (state == MoveState.WALLRUN)
        {
            checkUp = Physics2D.Raycast(transform.position, Vector2.up, rayCastLength, WALL_LAYER);
            checkDown = Physics2D.Raycast(transform.position, Vector2.down, rayCastLength, WALL_LAYER);
            checkLeft = Physics2D.Raycast(transform.position, Vector2.left, rayCastLength, WALL_LAYER);
            checkRight = Physics2D.Raycast(transform.position, Vector2.right, rayCastLength, WALL_LAYER);

            // Debug.Log(wallRunDirection + " " + wallRunMoveDir);
            if (wallRunDirection == WallRunDirection.CCW)
            {
                switch (wallRunMoveDir)
                {
                    case (WallRunMoveDir.RIGHT):
                        HandleWallRun(checkDown, checkRight, transform.position.x, checkDown.point.y + PLAYER_HEIGHT / 2, Vector2.right, WallRunMoveDir.UP);
                        break;
                    case (WallRunMoveDir.UP):
                        HandleWallRun(checkRight, checkUp, checkRight.point.x - PLAYER_WIDTH / 2, transform.position.y, Vector2.up, WallRunMoveDir.LEFT);
                        break;
                    case (WallRunMoveDir.LEFT):
                        HandleWallRun(checkUp, checkLeft, transform.position.x, checkUp.point.y - PLAYER_HEIGHT / 2, Vector2.left, WallRunMoveDir.DOWN);
                        break;
                    case (WallRunMoveDir.DOWN):
                        HandleWallRun(checkLeft, checkDown, checkLeft.point.x + PLAYER_WIDTH / 2, transform.position.y, Vector2.down, WallRunMoveDir.RIGHT);
                        break;
                }
            }
            else if (wallRunDirection == WallRunDirection.CW)
            {
                switch (wallRunMoveDir)
                {
                    case (WallRunMoveDir.RIGHT):
                        HandleWallRun(checkUp, checkRight, transform.position.x, checkUp.point.y - PLAYER_HEIGHT / 2, Vector2.right, WallRunMoveDir.DOWN);
                        break;
                    case (WallRunMoveDir.UP):
                        HandleWallRun(checkLeft, checkUp, checkLeft.point.x + PLAYER_WIDTH / 2, transform.position.y, Vector2.up, WallRunMoveDir.RIGHT);
                        break;
                    case (WallRunMoveDir.LEFT):
                        HandleWallRun(checkDown, checkLeft, transform.position.x, checkDown.point.y + PLAYER_HEIGHT / 2, Vector2.left, WallRunMoveDir.UP);
                        break;
                    case (WallRunMoveDir.DOWN):
                        HandleWallRun(checkRight, checkDown, checkRight.point.x - PLAYER_WIDTH / 2, transform.position.y, Vector2.down, WallRunMoveDir.LEFT);
                        break;
                }
            }                
            
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

        return WallRunMoveDir.LEFT;
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

        return Vector2.left;     
    }

    void HandleWallRun(RaycastHit2D checkFloor, RaycastHit2D checkAdj, float downOffsetX, float downOffsetY, Vector2 moveDir, WallRunMoveDir nextDir)
    {
        // Moving up
        if (checkFloor && checkAdj)
        {
            if (wallRunDirection == WallRunDirection.CCW)
            {
                rb.velocity = -Vector2.Perpendicular(checkAdj.normal).normalized * wallRunSpeed;
            }
            else 
            {
                rb.velocity = Vector2.Perpendicular(checkAdj.normal).normalized * wallRunSpeed;
            }
        }
        // Moving forward
        else if (checkFloor && !checkAdj)
        {
            rb.velocity = wallRunSpeed * moveDir;
            transform.position = new Vector3(downOffsetX, downOffsetY, transform.position.z);
        }
        // Not touching anything
        else if (!checkFloor && !checkAdj && !waitForNextCheck)
        {
            WallRunMoveDir oppDir = GetOppositeMoveDir(nextDir);
            rb.velocity = wallRunSpeed * GetDirectionVector(oppDir);
            wallRunMoveDir = oppDir;
            waitForNextCheck = true;
        }
        // Changing directions
        else if (!checkFloor && checkAdj)
        {
            wallRunMoveDir = nextDir;
        }

        if (checkFloor || checkAdj)
        {
            waitForNextCheck = false;
        }
    }


    // void OnCollisionStay2D(Collision2D col)
    // {
    //     if (state == MoveState.WALLRUN)
    //     {
    //         colContactPoint = col.contacts[0].point;
    //     }
    // }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * rayCastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayCastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * rayCastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * rayCastLength);

        Gizmos.color = Color.blue;
        // Gizmos.DrawLine(colContactPoint + (transform.position - colContactPoint).normalized*0.71f, colContactPoint);
        if (checkDown)
        {
            Gizmos.DrawLine(transform.position, checkDown.point);
        }

    }
}
