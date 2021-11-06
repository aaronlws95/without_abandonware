using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    int WALL_LAYER = 1 << 6;
    float RAYCAST_LENGTH = 0.6f;
    float SPRITE_HEIGHT = 1f;
    float SPRITE_WIDTH = 1f;

    public float collisionAvoidanceSpeed = 1f;
    public float wallRunSpeed = 5f;

    enum MoveState
    {
        WALLRUN
    }

    enum WallRunMoveDir
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    enum WallRunDirection
    {
        CW,
        CCW
    }

    // States
    MoveState state;
    WallRunMoveDir wallRunMoveDir;
    WallRunDirection wallRunDirection;

    RaycastHit2D checkUp;
    RaycastHit2D checkDown;
    RaycastHit2D checkLeft;
    RaycastHit2D checkRight;

    // Raycasting

    // Components
    Rigidbody2D rb;

    // Debugging
    Vector3 colContactPoint;

    void Start()
    {
        state = MoveState.WALLRUN;
        wallRunMoveDir = WallRunMoveDir.RIGHT;
        wallRunDirection = WallRunDirection.CCW;

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (wallRunDirection == WallRunDirection.CCW)
            {
                wallRunDirection = WallRunDirection.CW;
            }
            else
            {
                wallRunDirection = WallRunDirection.CCW;
            }
        }

        // Debug.Log(rb.velocity);
        if (state == MoveState.WALLRUN)
        {
            checkUp = Physics2D.Raycast(transform.position, Vector2.up, RAYCAST_LENGTH, WALL_LAYER);
            checkDown = Physics2D.Raycast(transform.position, Vector2.down, RAYCAST_LENGTH, WALL_LAYER);
            checkLeft = Physics2D.Raycast(transform.position, Vector2.left, RAYCAST_LENGTH, WALL_LAYER);
            checkRight = Physics2D.Raycast(transform.position, Vector2.right, RAYCAST_LENGTH, WALL_LAYER);

            // If on wall
            if (checkUp || checkDown || checkLeft || checkRight)
            {
                if (wallRunDirection == WallRunDirection.CCW)
                {
                    switch (wallRunMoveDir)
                    {
                        case (WallRunMoveDir.RIGHT):
                            HandleWallRun(checkDown, checkRight, transform.position.x, checkDown.point.y + SPRITE_HEIGHT / 2, Vector2.right, WallRunMoveDir.UP);
                            break;
                        case (WallRunMoveDir.UP):
                            HandleWallRun(checkRight, checkUp, checkRight.point.x - SPRITE_WIDTH / 2, transform.position.y, Vector2.up, WallRunMoveDir.LEFT);
                            break;
                        case (WallRunMoveDir.LEFT):
                            HandleWallRun(checkUp, checkLeft, transform.position.x, checkUp.point.y - SPRITE_HEIGHT / 2, Vector2.left, WallRunMoveDir.DOWN);
                            break;
                        case (WallRunMoveDir.DOWN):
                            HandleWallRun(checkLeft, checkDown, checkLeft.point.x + SPRITE_WIDTH / 2, transform.position.y, Vector2.down, WallRunMoveDir.RIGHT);
                            break;
                    }
                }
                else if (wallRunDirection == WallRunDirection.CW)
                {
                    switch (wallRunMoveDir)
                    {
                        case (WallRunMoveDir.RIGHT):
                            HandleWallRun(checkUp, checkRight, transform.position.x, checkUp.point.y - SPRITE_HEIGHT / 2, Vector2.right, WallRunMoveDir.DOWN);
                            break;
                        case (WallRunMoveDir.UP):
                            HandleWallRun(checkLeft, checkUp, checkLeft.point.x + SPRITE_WIDTH / 2, transform.position.y, Vector2.up, WallRunMoveDir.RIGHT);
                            break;
                        case (WallRunMoveDir.LEFT):
                            HandleWallRun(checkDown, checkLeft, transform.position.x, checkDown.point.y + SPRITE_HEIGHT / 2, Vector2.left, WallRunMoveDir.UP);
                            break;
                        case (WallRunMoveDir.DOWN):
                            HandleWallRun(checkRight, checkDown, checkRight.point.x - SPRITE_WIDTH / 2, transform.position.y, Vector2.down, WallRunMoveDir.LEFT);
                            break;
                    }
                }                
            }
        }
    }

    void HandleWallRun(RaycastHit2D checkFloor, RaycastHit2D checkAdj, float downOffsetX, float downOffsetY, Vector2 moveDir, WallRunMoveDir nextDir)
    {
        // Moving up
        if (checkFloor && checkAdj)
        {
            rb.velocity = -Vector2.Perpendicular(checkAdj.normal).normalized * wallRunSpeed;
        }
        // Moving forward
        else if (checkFloor)
        {
            rb.velocity = wallRunSpeed * moveDir;
            transform.position = new Vector3(downOffsetX, downOffsetY, transform.position.z);
        }
        // Changing directions
        else if (!checkFloor)
        {
            wallRunMoveDir = nextDir;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * RAYCAST_LENGTH);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * RAYCAST_LENGTH);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * RAYCAST_LENGTH);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * RAYCAST_LENGTH);

        Gizmos.color = Color.blue;
        // Gizmos.DrawLine(transform.position, colContactPoint);
        if (checkDown)
        {
            Gizmos.DrawLine(transform.position, checkDown.point);
        }

    }
}
