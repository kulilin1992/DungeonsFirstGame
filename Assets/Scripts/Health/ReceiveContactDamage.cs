using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveContactDamage : MonoBehaviour
{
    [SerializeField] private int contactDamageAmount;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void TakeConTactDamage(int damageAmount = 0)
    {
        if (contactDamageAmount > 0)
            damageAmount = contactDamageAmount;

        health.TakeDamage(damageAmount);
        
    }

}
