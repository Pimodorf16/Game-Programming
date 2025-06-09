using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private Rigidbody2D rb;
    private CharacterController2D characterController2D;
    
    public static event Action OnPlayerDamaged;
    public static event Action OnPlayerDeath;

    public float knockbackDuration = 0.5f;
    public float knockbackGravityScale = 3f;
    private float originalGravityScale;
    private bool isKnockedBack = false;

    public float currentHealth;
    public float maxHealth;
    public float damageCooldown = 0.5f;
    public bool isTakingDamage = false;
    private bool isAlive = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        characterController2D = GetComponent<CharacterController2D>();
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
        ApplyKnockback(knockbackForce, knockbackDirection);
        OnPlayerDamaged?.Invoke();

        if(currentHealth <= 0)
        {
            currentHealth = 0;
            isAlive = false;
            OnPlayerDeath?.Invoke();
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

        while (!characterController2D.m_Grounded)
        {
            yield return null;
        }

        if (isAlive == false)
        {
            Died();
        }
        else
        {
            rb.gravityScale = originalGravityScale;
            isKnockedBack = false;
        }
    }

    IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        isTakingDamage = false;
    }

    void Died()
    {

    }
}
