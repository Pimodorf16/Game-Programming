using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Engage")]
    public float engageDistance = 7f;
    public float optimalDistance = 5f;
    public float disengageDistance = 10f;

    [Header("Attack")]
    public Transform groundAttackOrigin;
    public bool canAttack = true;
    public float attackRadius = 1f;
    public float attackRange = 1f;
    public float attackRate = 1f;
    public int attackDamage = 1;
    public float attackKnockbackForce = 2f;
    public LayerMask playerMask;

    public bool isRanged = false;
    public GameObject arrowPrefab;
    public Transform firePoint;

    public bool CanAttack()
    {
        return canAttack;
    }

    public void GroundAttack()
    {
        Collider2D[] playerInRange = Physics2D.OverlapCircleAll(groundAttackOrigin.position, attackRadius, playerMask);
        foreach (var player in playerInRange)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            direction.y += Random.Range(0.3f, 0.9f);

            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage, attackKnockbackForce, direction);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundAttackOrigin.position, attackRadius);
    }
}
