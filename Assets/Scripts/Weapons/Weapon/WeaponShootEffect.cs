using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponShootEffect : MonoBehaviour
{
    private ParticleSystem shootEffectParticleSystem;

    private void Awake()
    {
        shootEffectParticleSystem = GetComponent<ParticleSystem>();
    }

    public void SetShootEffect(WeaponShootEffectSO shootEffect, float aimAngle)
    {
        SetShootEffectColorGradient(shootEffect.colorGradient);
        
        SetShootEffectParticlesStartingValues(shootEffect.duration, shootEffect.startParticleSize, shootEffect.startParticleSpeed,
            shootEffect.startLifeTime, shootEffect.effectGravity, shootEffect.maxParticleNumber);
        
        SetShootEffectParticleEmission(shootEffect.emissionRate, shootEffect.burstParticleNumber);
        
        SetEmmitterRotation(aimAngle);
        
        SetShootEffectParticleSprite(shootEffect.sprite);

        SetShootEffectVelocityOverLifeTime(shootEffect.velocityOverLifeTimeMin, shootEffect.velocityOverLifeTimeMax);
    }

    private void SetShootEffectColorGradient(Gradient colorGradient)
    {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = shootEffectParticleSystem.colorOverLifetime;
        colorOverLifetimeModule.color = colorGradient;
    }

    private void SetShootEffectParticlesStartingValues(float duration, float startParticleSize, float startParticleSpeed, float startLifeTime, float effectGravity, int maxParticleNumber)
    {
        ParticleSystem.MainModule mainModule= shootEffectParticleSystem.main;

        mainModule.duration = duration;
        mainModule.startSize = startParticleSize;
        mainModule.startSpeed = startParticleSpeed;
        mainModule.startLifetime = startLifeTime;
        mainModule.gravityModifier = effectGravity;
        mainModule.maxParticles = maxParticleNumber;
    }

    private void SetShootEffectParticleEmission(int emissionRate, int burstParticleNumber)
    {
        ParticleSystem.EmissionModule emissionModule = shootEffectParticleSystem.emission;
        ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, burstParticleNumber);
        emissionModule.rateOverTime = emissionRate;
    }

    private void SetEmmitterRotation(float aimAngle)
    {
        transform.eulerAngles = new Vector3(0f, 0f, aimAngle);
    }

    private void SetShootEffectParticleSprite(Sprite sprite)
    {
        ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = shootEffectParticleSystem.textureSheetAnimation;
        textureSheetAnimationModule.SetSprite(0, sprite);
    }

    private void SetShootEffectVelocityOverLifeTime(Vector3 velocityOverLifeTimeMin, Vector3 velocityOverLifeTimeMax)
    {
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = shootEffectParticleSystem.velocityOverLifetime;
       
        ParticleSystem.MinMaxCurve minMaxCurveX= new ParticleSystem.MinMaxCurve();
        minMaxCurveX.mode = ParticleSystemCurveMode.TwoCurves;
        minMaxCurveX.constantMin = velocityOverLifeTimeMin.x;
        minMaxCurveX.constantMax = velocityOverLifeTimeMax.x;
        velocityOverLifetimeModule.x = minMaxCurveX;

        ParticleSystem.MinMaxCurve minMaxCurveY= new ParticleSystem.MinMaxCurve();
        minMaxCurveY.mode = ParticleSystemCurveMode.TwoCurves;
        minMaxCurveY.constantMin = velocityOverLifeTimeMin.y;
        minMaxCurveY.constantMax = velocityOverLifeTimeMax.y;
        velocityOverLifetimeModule.y = minMaxCurveY;

        ParticleSystem.MinMaxCurve minMaxCurveZ= new ParticleSystem.MinMaxCurve();
        minMaxCurveZ.mode= ParticleSystemCurveMode.TwoCurves;
        minMaxCurveZ.constantMin= velocityOverLifeTimeMin.z;
        minMaxCurveZ.constantMax= velocityOverLifeTimeMax.z;
        velocityOverLifetimeModule.z = minMaxCurveZ;


    }
}
