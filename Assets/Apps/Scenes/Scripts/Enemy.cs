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
        Idle, Patrol, Searching, Returning, Follow, Engage, Flee, Attack, Dead
    }

    [Header("State Machine")]
    public EnemyState currentState;

    [Header("Behaviour Options")]
    public bool isStationary = false;
    public PostChaseBehaviour postChaseAction;

    [Header("Decisions")]
    private float timeSinceLastDecision = 0f;
    public float decisionCooldown = 1.0f;

    public float fleeHealthPercentage = 0.25f;

    private EnemyHealth enemyHealth;
    private EnemyMovement enemyMovement;
    private EnemyCombat enemyCombat;
    private AnimatorFunctions animatorFunctions;

    private Vector3 originPosition;
    private Vector3 lastKnownPlayerPosition;

    [SerializeField] private Animator animator;
    
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
        if (!enemyMovement.CanSeePlayer())
        {
            ChangeState(EnemyState.Follow, attacker);
        }
    }

    public void PlayHurtAnimation()
    {
        animator.SetTrigger("Hurt");
    }

    void DecideNextAction(Transform player)
    {
        if(enemyCombat.CanAttack() && Vector2.Distance(transform.position, player.position) < enemyCombat.attackRange)
        {
            if(Random.value > 0.3f)
            {
                ChangeState(EnemyState.Attack);
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
                    ChangeState(EnemyState.Flee);
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
                if (enemyHealth.GetCurrentHealthPercentage() <= fleeHealthPercentage)
                {
                    ChangeState(EnemyState.Flee);
                    break;
                }

                if (!canSeePlayer || Vector2.Distance(transform.position, playerTarget.position) > enemyCombat.disengageDistance)
                {
                    ChangeState(EnemyState.Follow);
                    break;
                }

                float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
                if(distanceToPlayer > enemyCombat.optimalDistance + 1f)
                {
                    enemyMovement.MoveTowards(playerTarget, enemyMovement.moveSpeed);
                }else if(distanceToPlayer < enemyCombat.optimalDistance - 1f)
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
                break;
            case EnemyState.Flee:
                enemyMovement.RunAwayFrom(playerTarget, enemyMovement.followSpeed);
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
                enemyMovement.StartFollowing(target);
                break;
            case EnemyState.Engage:
                break;
            case EnemyState.Flee:
                break;
            case EnemyState.Attack:
                animator.SetTrigger("Attack");
                enemyCombat.GroundAttack();
                ChangeState(EnemyState.Engage);
                break;
            case EnemyState.Dead:
                animator.SetBool("Dead", true);
                PlayHurtAnimation();
                this.enabled = false;
                break;
            default:
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if(currentState != EnemyState.Follow && currentState != EnemyState.Attack && currentState != EnemyState.Dead)
            {
                ChangeState(EnemyState.Follow, collision.transform);
            }
        }
    }
}
