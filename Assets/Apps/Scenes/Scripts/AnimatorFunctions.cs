using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorFunctions : MonoBehaviour
{
    void GroundAttackDamageOn()
    {
        FindObjectOfType<PlayerMelee>().groundAttackOn = true;
    }

    void GroundAttackDamagedOff()
    {
        FindObjectOfType<PlayerMelee>().groundAttackOn = false;
        Debug.Log("DamageOFF");
    }
}
