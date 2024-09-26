using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AmmoHitEffect : MonoBehaviour
{
    private ParticleSystem ammoHitEffectParticleSystem;

    private void Awake()
    {
        ammoHitEffectParticleSystem = GetComponent<ParticleSystem>();
    }

    public void SetHitEffect(AmmoHitEffectSO ammoHitEffect)
    {
        SetHitEffectColorGradient(ammoHitEffect.colorGradient);
        
        SetHitEffectParticlesStartingValues(ammoHitEffect.duration, ammoHitEffect.startParticleSize, ammoHitEffect.startParticleSpeed,
            ammoHitEffect.startLifeTime, ammoHitEffect.effectGravity, ammoHitEffect.maxParticleNumber);
        
        SetHitEffectParticleEmission(ammoHitEffect.emissionRate, ammoHitEffect.burstParticleNumber);
        
        SetHitEffectParticleSprite(ammoHitEffect.sprite);

        SetHitEffectVelocityOverLifeTime(ammoHitEffect.velocityOverLifeTimeMin, ammoHitEffect.velocityOverLifeTimeMax);
    }

    private void SetHitEffectColorGradient(Gradient colorGradient)
    {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = ammoHitEffectParticleSystem.colorOverLifetime;
        colorOverLifetimeModule.color = colorGradient;
    }

    private void SetHitEffectParticlesStartingValues(float duration, float startParticleSize, float startParticleSpeed, float startLifeTime, float effectGravity, int maxParticleNumber)
    {
        ParticleSystem.MainModule mainModule= ammoHitEffectParticleSystem.main;

        mainModule.duration = duration;
        mainModule.startSize = startParticleSize;
        mainModule.startSpeed = startParticleSpeed;
        mainModule.startLifetime = startLifeTime;
        mainModule.gravityModifier = effectGravity;
        mainModule.maxParticles = maxParticleNumber;
    }

    private void SetHitEffectParticleEmission(int emissionRate, int burstParticleNumber)
    {
        ParticleSystem.EmissionModule emissionModule = ammoHitEffectParticleSystem.emission;
        ParticleSystem.Burst burst = new ParticleSystem.Burst(0f, burstParticleNumber);
        emissionModule.rateOverTime = emissionRate;
    }

    private void SetHitEffectParticleSprite(Sprite sprite)
    {
        ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = ammoHitEffectParticleSystem.textureSheetAnimation;
        textureSheetAnimationModule.SetSprite(0, sprite);
    }

    private void SetHitEffectVelocityOverLifeTime(Vector3 velocityOverLifeTimeMin, Vector3 velocityOverLifeTimeMax)
    {
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = ammoHitEffectParticleSystem.velocityOverLifetime;
       
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
