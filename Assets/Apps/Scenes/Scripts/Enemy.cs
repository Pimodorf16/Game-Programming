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
        Idle, Patrol, Searching, Returning, Follow, Attack, Dead
    }

    [Header("State Machine")]
    public EnemyState currentState;

    [Header("Behaviour Options")]
    public bool isStationary = false;
    public PostChaseBehaviour postChaseAction;

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

    private void Update()
    {
        bool canSeePlayer = enemyMovement.CanSeePlayer();
        Transform playerTarget = enemyMovement.GetSeenPlayer();

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
                if (canSeePlayer)
                {
                    lastKnownPlayerPosition = enemyMovement.GetSeenPlayer().position;
                }
                else
                {
                    ChangeState(EnemyState.Searching);
                }
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Dead:
                break;
            default:
                break;
        }
    }

    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

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
                enemyMovement.StartFollowing(enemyMovement.GetSeenPlayer());
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Dead:
                break;
            default:
                break;
        }
    }
}
