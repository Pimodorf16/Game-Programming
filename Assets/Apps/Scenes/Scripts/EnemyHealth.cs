using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private Enemy enemyController;

    Rigidbody2D rb;

    public float maxHealth = 3f;
    [SerializeField]private float currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        enemyController = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damage/*, Vector2 knockbackDirection, float knockbackForce*/)
    {
        currentHealth -= damage;

        //rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        if(currentHealth <= 0)
        {
            EnemyDie();
        }
    }

    void EnemyDie()
    {
        enemyController.currentState = Enemy.EnemyState.Dead;
        Debug.Log("enemy died");
    }
}
