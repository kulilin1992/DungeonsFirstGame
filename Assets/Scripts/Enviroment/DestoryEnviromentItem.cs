using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryEnviromentItem : MonoBehaviour
{
    [SerializeField] private int startingHealthAmount = 1;
    [SerializeField] private SoundEffectSO destorySoundEffect;

    private Animator animator;
    private BoxCollider2D boxCollider2D;
    private HealthEvent healthEvent;
    private Health health;
    private ReceiveContactDamage receiveContactDamage;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        health.SetStartingHealth(startingHealthAmount);
        receiveContactDamage = GetComponent<ReceiveContactDamage>();
    }

    private void OnEnable()
    {
        healthEvent.OnHealthEvent += OnHealthChanged;
    }
    private void OnDisable()
    {
        healthEvent.OnHealthEvent -= OnHealthChanged;
    }

    private void OnHealthChanged(HealthEvent healthEvent, HealthEventArgs args)
    {
        if (args.healthAmount <= 0f) {
            StartCoroutine(PlayAnimation());
        }
    }

    private IEnumerator PlayAnimation()
    {
        Destroy(boxCollider2D);

        if (destorySoundEffect != null) {
            SoundEffectManager.Instance.PlaySoundEffect(destorySoundEffect);
        }

        animator.SetBool(Settings.destory, true);

        while(!animator.GetCurrentAnimatorStateInfo(0).IsName(Settings.stateDestoryed)) {
            yield return null;

        }

        Destroy(animator);
        Destroy(receiveContactDamage);
        Destroy(health);
        Destroy(healthEvent);
        Destroy(this);
    }
}
