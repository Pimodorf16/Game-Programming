using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerDeath;

    private PlayerMovement playerMovement;

    public float currentHealth;
    public float maxHealth;
    public float damageCooldown = 0.5f;
    public bool isTakingDamage = false;
    public bool isAlive = true;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, float knockbackForce, Vector2 knockbackDirection)
    {
        if (isTakingDamage)
        {
            return;
        }

        isTakingDamage = true;
        StartCoroutine(DamageCooldown());

        currentHealth -= damage;
        playerMovement.ApplyKnockback(knockbackForce, knockbackDirection);
        OnPlayerDamaged?.Invoke();

        if(currentHealth <= 0)
        {
            currentHealth = 0;
            isAlive = false;
        }
        else
        {
            playerMovement.animator.SetTrigger("Hurt");
        }
    }

    IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        isTakingDamage = false;
    }

    public void Died()
    {
        OnPlayerDeath?.Invoke();
        playerMovement.animator.SetBool("IsDead", true);
        playerMovement.animator.SetTrigger("Hurt");
        playerMovement.rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        playerMovement.rb.bodyType = RigidbodyType2D.Static;
        Debug.Log("Died");
        playerMovement.LoadMenu();
        Debug.Log("LoadMenu");
    }
}
