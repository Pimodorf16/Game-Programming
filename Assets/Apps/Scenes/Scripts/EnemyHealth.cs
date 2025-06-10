using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private Enemy enemy;
    private EnemyMovement enemyMovement;

    public float maxHealth = 3f;
    [SerializeField]private float currentHealth;
    public float damageCooldown = 0.5f;

    public bool isTakingDamage = false;
    private bool isAlive = true;

    public bool GetAliveStatus()
    {
        return isAlive;
    }

    public float GetCurrentHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<Enemy>();
        enemyMovement = GetComponent<EnemyMovement>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, float knockbackForce, Vector2 knockbackDirection, Transform attacker)
    {
        if (isTakingDamage || enemy.currentState == Enemy.EnemyState.Dead)
        {
            return;
        }

        isTakingDamage = true;
        StartCoroutine(DamageCooldown());

        currentHealth -= damage;
        enemyMovement.ApplyKnockback(knockbackForce, knockbackDirection);
        enemy.WasAttacked(attacker);

        if (currentHealth <= 0)
        {
            EnemyDie();
        }
        else
        {
            enemy.ChangeState(Enemy.EnemyState.Hurt);
        }
    }

    void EnemyDie()
    {
        isAlive = false;
        enemy.ChangeState(Enemy.EnemyState.Dead);
    }

    IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        isTakingDamage = false;
    }
}
