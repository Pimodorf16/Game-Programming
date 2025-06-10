using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PostChaseBehaviour
{
    ReturnToOrigin, PatrolFromCurrentLocation
}

public class Enemy : MonoBehaviour
{
    public enum EnemyType {Sword, Archer, Hybrid}
    public EnemyType enemyType;

    public enum EnemyState
    {
        Idle, Patrol, Searching, Returning, Follow, Engage, RushIn, Flee, Hurt, Attack, Dead
    }

    [Header("State Machine")]
    public EnemyState currentState;

    [Header("Behaviour Options")]
    public bool isStationary = false;
    public PostChaseBehaviour postChaseAction;

    [Header("Decisions")]
    private float timeSinceLastDecision = 0f;
    public float decisionCooldown = 0.5f;

    [Header("Combat")]
    public float hurtDuration = 0.4f;
    public float fleeHealthPercentage = 0.25f;
    [SerializeField] float playerDistance;
    [SerializeField] private Transform fleeFrom;

    private EnemyHealth enemyHealth;
    private EnemyMovement enemyMovement;
    private EnemyCombat enemyCombat;
    private AnimatorFunctions animatorFunctions;

    private Vector3 originPosition;
    private Vector3 lastKnownPlayerPosition;

    [SerializeField] private Animator animator;
    
    private PlayerMelee playerMelee;
    private bool playerWasAttackingLastFrame = false;

    public KillCount killCount;

    // Start is called before the first frame update
    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemyMovement = GetComponent<EnemyMovement>();
        enemyCombat = GetComponent<EnemyCombat>();
        animatorFunctions = GetComponent<AnimatorFunctions>();

        originPosition = transform.position;

        if (isStationary)
        {
            currentState = EnemyState.Idle;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
    }

    public void WasAttacked(Transform attacker)
    {
        if(!enemyMovement.CanSeePlayer() && enemyHealth.GetCurrentHealthPercentage() <= fleeHealthPercentage)
        {
            ChangeState(EnemyState.Flee, attacker);
        }
        else if (!enemyMovement.CanSeePlayer() && enemyHealth.GetCurrentHealthPercentage() > fleeHealthPercentage)
        {
            ChangeState(EnemyState.Follow, attacker);
        }
    }   

    void DecideNextAction(Transform player)
    {
        if (enemyHealth.GetCurrentHealthPercentage() <= fleeHealthPercentage)
        {
            ChangeState(EnemyState.Flee, player);
        }
        else if (enemyCombat.CanAttack() && Vector2.Distance(transform.position, player.position) < enemyCombat.attackRange)
        {
            if(Random.value > 0.3f)
            {
                ChangeState(EnemyState.Attack);
            }
        }
        else if(enemyCombat.CanAttack() && Vector2.Distance(transform.position, player.position) > enemyCombat.attackRange)
        {
            if(Random.value > 0.8f)
            {
                ChangeState(EnemyState.RushIn);
            }
        }
    }

    private void Update()
    {
        bool isAlive = enemyHealth.GetAliveStatus();
        bool canSeePlayer = enemyMovement.CanSeePlayer();
        Transform playerTarget = enemyMovement.GetSeenPlayer();

        if(isAlive == false)
        {
            ChangeState(EnemyState.Dead);
            return;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                if (canSeePlayer)
                {
                    ChangeState(EnemyState.Follow);
                }
                break;
            case EnemyState.Patrol:
                if (canSeePlayer)
                {
                    ChangeState(EnemyState.Follow);
                }
                break;
            case EnemyState.Searching:
                if (canSeePlayer)
                {
                    ChangeState(EnemyState.Follow);
                }

                if (Vector2.Distance(transform.position, lastKnownPlayerPosition) < 0.5f)
                {
                    if(postChaseAction == PostChaseBehaviour.ReturnToOrigin)
                    {
                        ChangeState(EnemyState.Returning);
                    }
                    else
                    {
                        ChangeState(isStationary ? EnemyState.Idle : EnemyState.Patrol);
                    }
                }
                break;
            case EnemyState.Returning:
                if (canSeePlayer)
                {
                    ChangeState(EnemyState.Follow);
                    break;
                }

                if(Vector2.Distance(transform.position, originPosition) < 0.5f)
                {
                    ChangeState(isStationary ? EnemyState.Idle : EnemyState.Patrol);
                }
                break;
            case EnemyState.Follow:
                if(enemyHealth.GetCurrentHealthPercentage() <= fleeHealthPercentage)
                {
                    ChangeState(EnemyState.Flee, playerTarget);
                    break;
                }
                
                if (canSeePlayer)
                {
                    lastKnownPlayerPosition = enemyMovement.GetSeenPlayer().position;

                    if (Vector2.Distance(transform.position, playerTarget.position) < enemyCombat.engageDistance)
                    {
                        ChangeState(EnemyState.Engage);
                    }
                }
                else
                {
                    ChangeState(EnemyState.Searching);
                }
                break;
            case EnemyState.Engage:
                if(playerMelee == null)
                {
                    ChangeState(EnemyState.Searching);
                    break;
                }

                bool playerIsAttackingNow = playerMelee.groundAttackOn;
                
                if(playerWasAttackingLastFrame && !playerIsAttackingNow)
                {
                    if (enemyCombat.CanAttack())
                    {
                        ChangeState(EnemyState.RushIn);
                        playerWasAttackingLastFrame = playerIsAttackingNow;
                        break;
                    }
                }

                if (enemyHealth.GetCurrentHealthPercentage() <= fleeHealthPercentage)
                {
                    ChangeState(EnemyState.Flee, playerTarget);
                    break;
                }

                if (!canSeePlayer || Vector2.Distance(transform.position, playerTarget.position) > enemyCombat.disengageDistance)
                {
                    ChangeState(EnemyState.Follow);
                    break;
                }

                float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
                playerDistance = distanceToPlayer;
                if(distanceToPlayer > enemyCombat.optimalDistance + 0.1f)
                {
                    enemyMovement.MoveTowards(playerTarget, enemyMovement.moveSpeed);
                }else if(distanceToPlayer < enemyCombat.optimalDistance - 0.2f)
                {
                    enemyMovement.MoveAwayFrom(playerTarget, enemyMovement.moveSpeed);
                }
                else
                {
                    enemyMovement.StopMoving();
                    enemyMovement.FaceTarget(playerTarget);
                }

                timeSinceLastDecision += Time.deltaTime;
                if(timeSinceLastDecision > decisionCooldown)
                {
                    DecideNextAction(playerTarget);
                    timeSinceLastDecision = 0f;
                }

                playerWasAttackingLastFrame = playerIsAttackingNow;
                break;
            case EnemyState.RushIn:
                if (enemyHealth.GetCurrentHealthPercentage() <= fleeHealthPercentage)
                {
                    ChangeState(EnemyState.Flee, playerTarget);
                    break;
                }

                if (!canSeePlayer || Vector2.Distance(transform.position, playerTarget.position) > enemyCombat.disengageDistance)
                {
                    ChangeState(EnemyState.Follow);
                    break;
                }

                distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
                playerDistance = distanceToPlayer;
                if (distanceToPlayer > enemyCombat.optimalDistance - 0.8f)
                {
                    enemyMovement.MoveTowards(playerTarget, enemyMovement.followSpeed + 3);
                }
                else
                {
                    ChangeState(EnemyState.Attack);
                }
                break;
            case EnemyState.Flee:
                distanceToPlayer = Vector2.Distance(transform.position, fleeFrom.position);
                playerDistance = distanceToPlayer;
                enemyMovement.RunAwayFrom(fleeFrom, enemyMovement.followSpeed);
                if(distanceToPlayer > 15f)
                {
                    ChangeState(EnemyState.Idle);
                }
                break;
            case EnemyState.Hurt:
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Dead:
                break;
            default:
                break;
        }
    }

    public void ChangeState(EnemyState newState, Transform forcedTarget = null)
    {
        if (currentState == newState && forcedTarget == null) return;

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Idle:
                enemyMovement.StopMoving();

                if(enemyMovement.startingDirection == StartingDirection.Left)
                {
                    enemyMovement.SetFacingDirection(false);
                }
                else
                {
                    enemyMovement.SetFacingDirection(true);
                }
                break;
            case EnemyState.Patrol:
                enemyMovement.StartPatrolling();
                break;
            case EnemyState.Searching:
                enemyMovement.MoveToPosition(lastKnownPlayerPosition);
                break;
            case EnemyState.Returning:
                enemyMovement.MoveToPosition(originPosition);
                break;
            case EnemyState.Follow:
                Transform target = forcedTarget;
                if(target == null)
                {
                    target = enemyMovement.GetSeenPlayer();
                }

                if(target != null)
                {
                    playerMelee = target.GetComponent<PlayerMelee>();
                }

                enemyMovement.StartFollowing(target);
                break;
            case EnemyState.Engage:
                break;
            case EnemyState.RushIn:
                break;
            case EnemyState.Flee:
                fleeFrom = forcedTarget;
                break;
            case EnemyState.Hurt:
                StartCoroutine(HurtCoroutine());
                break;
            case EnemyState.Attack:
                enemyMovement.StopMoving();
                StartCoroutine(AttackCoroutine());
                break;
            case EnemyState.Dead:
                animator.SetBool("Dead", true);
                animator.SetTrigger("Hurt");
                killCount.AddKill();
                this.enabled = false;
                break;
            default:
                break;
        }
    }

    IEnumerator HurtCoroutine()
    {
        animator.SetTrigger("Hurt");

        yield return new WaitForSeconds(hurtDuration);

        if (enemyHealth.GetAliveStatus())
        {
            if (enemyHealth.GetCurrentHealthPercentage() <= fleeHealthPercentage)
            {
                ChangeState(EnemyState.Flee, fleeFrom);
            }
            else
            {
                ChangeState(EnemyState.Engage);
            }   
        }
    }

    IEnumerator AttackCoroutine()
    {
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(enemyCombat.attackRate);

        ChangeState(EnemyState.Engage);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if(currentState != EnemyState.Follow && currentState != EnemyState.Attack && currentState != EnemyState.Dead && currentState != EnemyState.Engage && currentState != EnemyState.Hurt && currentState != EnemyState.Flee)
            {
                ChangeState(EnemyState.Follow, collision.transform);
            }
        }
    }
}
