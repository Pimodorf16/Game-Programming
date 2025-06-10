using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public PlayerHealth playerHealth;
    public AnimatorFunctions animatorFunctions;
    public Animator playerAnimator;
    public InputActionReference attack;
    
    public Transform groundAttackOrigin;
    public float attackRadius = 1f;
    public LayerMask enemyMask;

    public int attackDamage = 1;
    public float attackKnockbackForce = 2f;

    public float cooldownTime = .34f;
    private float cooldownTimer = 0f;
    private bool inCooldown = false;

    public bool groundAttackOn = false;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        attack.action.started += Attack;
    }

    private void OnDisable()
    {
        attack.action.started -= Attack;
    }

    void Attack(InputAction.CallbackContext obj)
    {
        if (playerMovement.onAir == false && inCooldown == false && playerMovement.groundAttacking == false && playerMovement.isKnockedBack == false && playerHealth.isTakingDamage == false && playerHealth.isAlive == true)
        {
            GroundAttack();
        }
    }

    private void Update()
    {
        if(cooldownTimer <= 0)
        {
            inCooldown = false;
        }
        else
        {
            cooldownTimer -= Time.deltaTime;
            inCooldown = true;
        }

        if(groundAttackOn == true)
        {
            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(groundAttackOrigin.position, attackRadius, enemyMask);
            foreach (var enemy in enemiesInRange)
            {
                Vector2 direction = (enemy.transform.position - transform.position).normalized;
                direction.y += Random.Range(0.3f, 0.9f);
                
                enemy.GetComponent<EnemyHealth>().TakeDamage(attackDamage, attackKnockbackForce, direction, transform);
            }
            Debug.Log("Attacking");
        }
    }

    void GroundAttack()
    {
        playerMovement.groundAttacking = true;
        
        playerAnimator.SetTrigger("Attack");

        StartCoroutine(GroundAttackWait());
    }

    IEnumerator GroundAttackWait()
    {
        yield return new WaitForSeconds(0.4f);
        
        playerMovement.groundAttacking = false;
        Debug.Log("notGroundAttacking2");
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundAttackOrigin.position, attackRadius);
    }
}
