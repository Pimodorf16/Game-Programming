using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StartingDirection
{
    Left, Right
}

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Orientation")]
    public StartingDirection startingDirection = StartingDirection.Left;
    
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float jumpForce = 8f;

    [Header("Patrol Settings")]


    [Header("Follow Settings")]
    public float followSpeed = 2f;
    public Transform followTarget;

    [Header("Raycast Detection")]
    public float wallCheckDistance = 0.5f;
    public float ledgeCheckDistance = 1f;
    public float playerSightDistance = 10f;
    public Transform castPoint;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    Rigidbody2D rb;
    bool isFacingRight = true;
    bool isPatrolling = false;
    bool isMovingToPosition = false;

    bool isTouchingWall;
    bool isNearLedge;
    bool canSeePlayer;

    Vector3 destination;

    public void SetFacingDirection(bool faceRight)
    {
        if(faceRight && !isFacingRight)
        {
            Flip();
        }else if(!faceRight && isFacingRight)
        {
            Flip();
        }
    }

    public bool CanSeePlayer()
    {
        return canSeePlayer;
    }

    public Transform GetPlayerTarget()
    {
        return canSeePlayer ? followTarget : null;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        followTarget = null;
        isFacingRight = true;

        if(startingDirection == StartingDirection.Left)
        {
            SetFacingDirection(false);
        }
        else
        {
            SetFacingDirection(true);
        }
    }

    private void Update()
    {
        DrawDebugRays();
    }

    private void FixedUpdate()
    {
        PerformDetections();

        if(isPatrolling)
        {
            Patrol();
        }
        else if(isMovingToPosition)
        {
            MoveTowardsDestination();
        }
        else if (followTarget != null)
        {
            Follow();
        }
    }

    void MoveTowardsDestination()
    {
        float moveDirection = (destination.x > transform.position.x) ? 1 : -1;
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        if ((moveDirection > 0 && !isFacingRight) || (moveDirection < 0 && isFacingRight))
        {
            Flip();
        }
    }

    public void StopMoving()
    {
        isPatrolling = false;
        isMovingToPosition = false;
        followTarget = null;
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    public void StartPatrolling()
    {
        isPatrolling = true;
        isMovingToPosition = false;
        followTarget = null;
    }

    public void MoveToPosition(Vector3 position)
    {
        isMovingToPosition = true;
        isPatrolling = false;
        followTarget = null;
        destination = position;
    }

    public void StartFollowing(Transform playerTarget)
    {
        isMovingToPosition = false;
        isPatrolling = false;
        followTarget = playerTarget;
    }

    void PerformDetections()
    {
        //Get Enemy Facing Direction
        Vector2 castDirection = isFacingRight ? Vector2.right : Vector2.left;

        //Detect Wall
        RaycastHit2D wallHit = Physics2D.Raycast(castPoint.position, castDirection, wallCheckDistance, groundLayer);
        isTouchingWall = wallHit.collider != null;

        //Detect Ledge
        Vector2 ledgeRayOrigin = castPoint.position + (Vector3)(castDirection * wallCheckDistance);
        RaycastHit2D ledgeHit = Physics2D.Raycast(ledgeRayOrigin, Vector2.down, ledgeCheckDistance, groundLayer);
        isNearLedge = ledgeHit.collider == null;

        //Detect Player
        RaycastHit2D playerHit = Physics2D.Raycast(castPoint.position, castDirection, playerSightDistance, playerLayer);
        canSeePlayer = playerHit.collider != null;

        if (canSeePlayer)
        {
            followTarget = playerHit.transform;
        }
        else
        {
            followTarget = null;
        }
    }

    void Patrol()
    {
        if(isTouchingWall || isNearLedge)
        {
            Flip();
        }

        float moveDirection = isFacingRight ? 1 : -1;
        rb.velocity = new Vector2(moveSpeed * moveDirection, rb.velocity.y);
    }

    void Follow()
    {
        if(followTarget != null)
        {
            if (isTouchingWall)
            {
                if(followTarget.position.y > transform.position.y)
                {
                    Jump();
                }
            }

            float moveDirection = (followTarget.position.x > transform.position.x) ? 1 : -1;
            rb.velocity = new Vector2(moveDirection * followSpeed, rb.velocity.y);

            if((moveDirection > 0 && !isFacingRight) || (moveDirection < 0 && isFacingRight))
            {
                Flip();
            }
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void Jump()
    {
        rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
    }

    void DrawDebugRays()
    {
        Vector2 castDirection = isFacingRight ? Vector2.right : Vector2.left;

        Color wallRayColor = isTouchingWall ? Color.red : Color.green;
        Color ledgeRayColor = isNearLedge ? Color.red : Color.green;
        Color playerRayColor = canSeePlayer ? Color.magenta : Color.cyan;

        Debug.DrawRay(castPoint.position, castDirection * wallCheckDistance, wallRayColor);

        Vector2 ledgeRayOrigin = castPoint.position + (Vector3)(castDirection * wallCheckDistance);
        Debug.DrawRay(ledgeRayOrigin, Vector2.down * ledgeCheckDistance, ledgeRayColor);

        Debug.DrawRay(castPoint.position, castDirection * playerSightDistance, playerRayColor);
    }
}
