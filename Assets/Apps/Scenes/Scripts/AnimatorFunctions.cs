using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorFunctions : MonoBehaviour
{
    private PlayerMelee playerMelee;
    private EnemyCombat enemyCombat;

    private void Awake()
    {
        playerMelee = GetComponent<PlayerMelee>();
        enemyCombat = GetComponent<EnemyCombat>();
    }

    void PlayerGroundAttackDamageOn()
    {
        if(playerMelee != null)
        {
            playerMelee.groundAttackOn = true;
        }
    }

    void PlayerGroundAttackDamagedOff()
    {
        if (playerMelee != null)
        {
            playerMelee.groundAttackOn = false;
        }
        Debug.Log("DamageOFF");
    }


    void EnemyGroundAttack()
    {
        if (enemyCombat != null)
        {
            enemyCombat.GroundAttack();
        }
    }
}
