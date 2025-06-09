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
    public float moveSpeed = 10f;
    public float jumpForce = 5.5f;
    public float jumpCooldown = 1f;
    [Range(0, .3f)] [SerializeField] private float movementSmoothing = .05f;
    [SerializeField] private float moveMultiplier = 10f;
    private Vector3 velocity = Vector3.zero;

    [Header("Patrol Settings")]


    [Header("Follow Settings")]
    public float followSpeed = 20f;
    public Transform followTarget;

    [Header("Knockback")]
    public float knockbackDuration = 0.5f;
    public float knockbackGravityScale = 3f;
    private float originalGravityScale;
    private bool isKnockedBack = false;

    [Header("Raycast Detection")]
    public Vector2 playerVisionBoxSize = new Vector2(8f, 4f);
    public float wallCheckDistance = 0.5f;
    public float ledgeCheckDistance = 1f;
    public float groundCheckRadius = 1f;
    public Transform castPoint;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    EnemyHealth enemyHealth;
    
    Rigidbody2D rb;
    Animator animator;

    Transform seenPlayer;

    float lastJumpTime = -10f;

    bool isFacingRight = true;
    bool isPatrolling = false;
    bool isMovingToPosition = false;

    bool isTouchingWall;
    bool isNearLedge;
    bool canSeePlayer;
    bool isGrounded;

    Vector3 destination;

    public void SetFacingDirection(bool faceRight)
    {
        if(isFacingRight != faceRight)
        {
            Flip();
        }
    }

    public bool CanSeePlayer()
    {
        return canSeePlayer;
    }

    public Transform GetSeenPlayer()
    {
        return seenPlayer;
    }

    private void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        
        DrawDebugRays();
    }

    private void FixedUpdate()
    {
        PerformDetections();

        if (isKnockedBack)
        {
            return;
        }

        if (isPatrolling)
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

    public void ApplyKnockback(float force, Vector2 direction)
    {
        StopAllCoroutines();
        StartCoroutine(KnockbackCoroutine(force, direction));
    }

    private IEnumerator KnockbackCoroutine(float force, Vector2 direction)
    {
        originalGravityScale = rb.gravityScale;
        rb.gravityScale = knockbackGravityScale;
        isKnockedBack = true;

        rb.velocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        while (!isGrounded)
        {
            yield return null;
        }

        if(enemyHealth.GetAliveStatus() == false)
        {
            Died();
        }
        else
        {
            rb.gravityScale = originalGravityScale;
            isKnockedBack = false;
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

    public void Died()
    {
        isMovingToPosition = false;
        isPatrolling = false;
        followTarget = null;
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
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

        //Detect Ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        //Detect Player
        Vector2 boxCenter = (Vector2)castPoint.position + new Vector2(castDirection.x * playerVisionBoxSize.x / 2, 0);
        Collider2D playerCollider = Physics2D.OverlapBox(boxCenter, playerVisionBoxSize, 0f, playerLayer);
        canSeePlayer = playerCollider != null;

        if (canSeePlayer)
        {
            seenPlayer = playerCollider.transform;
        }
        else
        {
            seenPlayer = null;
        }
    }

    void Patrol()
    {
        if(isTouchingWall || isNearLedge)
        {
            Flip();
        }

        float moveDirection = isFacingRight ? 1 : -1;
        Move(moveSpeed * moveDirection * Time.fixedDeltaTime);

        //alt//rb.velocity = new Vector2(moveSpeed * moveDirection * Time.fixedDeltaTime, rb.velocity.y);
    }

    void MoveTowardsDestination()
    {
        bool canJump = Time.time >= lastJumpTime + jumpCooldown;

        if (isTouchingWall && isGrounded && canJump)
        {
            Jump();
        }

        float moveDirection = (destination.x > transform.position.x) ? 1 : -1;

        Move(moveSpeed * moveDirection * Time.fixedDeltaTime);
        //alt//rb.velocity = new Vector2(moveDirection * moveSpeed * Time.fixedDeltaTime, rb.velocity.y);

        if ((moveDirection > 0 && !isFacingRight) || (moveDirection < 0 && isFacingRight))
        {
            Flip();
        }
    }

    void Follow()
    {
        if(followTarget != null)
        {
            bool canJump = Time.time >= lastJumpTime + jumpCooldown;
            
            if (isTouchingWall && isGrounded && canJump)
            {
                Jump();
            }

            float moveDirection = (followTarget.position.x > transform.position.x) ? 1 : -1;

            Move(followSpeed * moveDirection * Time.fixedDeltaTime);
            //alt//rb.velocity = new Vector2(moveDirection * followSpeed * Time.fixedDeltaTime, rb.velocity.y);

            if((moveDirection > 0 && !isFacingRight) || (moveDirection < 0 && isFacingRight))
            {
                Flip();
            }
        }
    }

    public void MoveTowards(Transform target, float speed)
    {
        if (target != null)
        {
            bool canJump = Time.time >= lastJumpTime + jumpCooldown;

            if (isTouchingWall && isGrounded && canJump)
            {
                Jump();
            }

            float moveDirection = (target.position.x > transform.position.x) ? 1 : -1;

            Move(speed * moveDirection * Time.fixedDeltaTime);

            if ((moveDirection > 0 && !isFacingRight) || (moveDirection < 0 && isFacingRight))
            {
                Flip();
            }
        }
    }

    public void MoveAwayFrom(Transform target, float speed)
    {
        if (target != null)
        {
            bool canJump = Time.time >= lastJumpTime + jumpCooldown;

            if (isTouchingWall && isGrounded && canJump)
            {
                Jump();
            }

            float moveDirection = (target.position.x < transform.position.x) ? 1 : -1;

            Move(speed * moveDirection * Time.fixedDeltaTime);

            if ((moveDirection < 0 && !isFacingRight) || (moveDirection > 0 && isFacingRight))
            {
                Flip();
            }
        }
    }

    public void FaceTarget(Transform target)
    {
        if (target != null)
        {
            float faceDirection = (target.position.x < transform.position.x) ? 1 : -1;

            if ((faceDirection < 0 && !isFacingRight) || (faceDirection > 0 && isFacingRight))
            {
                Flip();
            }
        }
    }

    public void RunAwayFrom(Transform target, float speed)
    {
        if (target != null)
        {
            bool canJump = Time.time >= lastJumpTime + jumpCooldown;

            if (isTouchingWall && isGrounded && canJump)
            {
                Jump();
            }

            float moveDirection = (target.position.x < transform.position.x) ? 1 : -1;

            Move(speed * moveDirection * Time.fixedDeltaTime);

            if ((moveDirection > 0 && !isFacingRight) || (moveDirection < 0 && isFacingRight))
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

        lastJumpTime = Time.time;
    }

    public void Move(float move)
    {
        Vector3 targetVelocity = new Vector2(move * moveMultiplier, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref velocity, movementSmoothing);
    }

    void DrawDebugRays()
    {
        Vector2 castDirection = isFacingRight ? Vector2.right : Vector2.left;

        Color wallRayColor = isTouchingWall ? Color.red : Color.green;
        Color ledgeRayColor = isNearLedge ? Color.red : Color.green;

        Debug.DrawRay(castPoint.position, castDirection * wallCheckDistance, wallRayColor);

        Vector2 ledgeRayOrigin = castPoint.position + (Vector3)(castDirection * wallCheckDistance);
        Debug.DrawRay(ledgeRayOrigin, Vector2.down * ledgeCheckDistance, ledgeRayColor);
    }

    private void OnDrawGizmos()
    {
        if (castPoint == null) return;

        Gizmos.color = Color.cyan;
        Vector2 castDirection = isFacingRight ? Vector2.right : Vector2.left;

        Vector3 boxCenter = castPoint.position + (Vector3)(castDirection * (playerVisionBoxSize.x / 2));
        Gizmos.DrawWireCube(boxCenter, playerVisionBoxSize);

        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
