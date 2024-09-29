
using UnityEngine;

public class Ammo : MonoBehaviour, IFireable
{
    [SerializeField] private TrailRenderer trailRenderer;

    private float ammoRange = 0f;
    private float ammoSpeed;
    private Vector3 fireDirection;
    private float fireDirectionAngle;
    private SpriteRenderer spriteRenderer;
    private AmmoDetailsSO ammoDetails;
    private float ammoChargeTimer;
    private bool isAmmoMaterialSet = false;
    private bool overrideAmmoMovement;

    private bool isColliding = false;

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(trailRenderer), trailRenderer);
    }

#endif

    #endregion Validation

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (ammoChargeTimer > 0f)
        {
            ammoChargeTimer -= Time.deltaTime;
            return;
        }
        else if (!isAmmoMaterialSet) {
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }
        
        //calculate distance to move ammo
        Vector3 distanceToMove = fireDirection * ammoSpeed * Time.deltaTime;
        transform.position += distanceToMove;

        ammoRange -= distanceToMove.magnitude;

        if (ammoRange <= 0f)
        {
            if (ammoDetails.isPlayerAmmo) {
                StaticEventHandler.CallMultiplierChangedEvent(false);
            }
            DisableAmmo();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (isColliding) return;

        DealDamage(collision);

        DisplayAmmoHitEffect();
        DisableAmmo();
    }

    private void DealDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();

        bool enemyHit = false;
        if (health != null)
        {
            isColliding = true;
            health.TakeDamage(ammoDetails.ammoDamage);

            if (health.enemy != null) {
                enemyHit = true;
            }
        }

        if (ammoDetails.isPlayerAmmo ) {
            
            if (enemyHit) {
                StaticEventHandler.CallMultiplierChangedEvent(true);
            }
            else {
                StaticEventHandler.CallMultiplierChangedEvent(false);

            }
        }
    }

    private void DisplayAmmoHitEffect()
    {
        if (ammoDetails.ammoHitEffect != null && ammoDetails.ammoHitEffect.ammoHitEffectPrefab != null)
        {
            AmmoHitEffect ammoHitEffect = (AmmoHitEffect) PoolManager.Instance.ReuseComponent
                (ammoDetails.ammoHitEffect.ammoHitEffectPrefab, transform.position, Quaternion.identity);

            ammoHitEffect.SetHitEffect(ammoDetails.ammoHitEffect);    
            
            ammoHitEffect.gameObject.SetActive(true);
        }
    }

    private void DisableAmmo()
    {
        gameObject.SetActive(false);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void InitialiseAmmo(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectionVector, bool overrideAmmoMovement = false)
    {
        //ammo
        this.ammoDetails = ammoDetails;
        isColliding = false;
        SetFireDirection(ammoDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);
        spriteRenderer.sprite = ammoDetails.ammoIcon;

        if (ammoDetails.ammoChargeTime > 0f) {
            ammoChargeTimer = ammoDetails.ammoChargeTime;
            SetAmmoMaterial(ammoDetails.ammoChargeMaterial);
            isAmmoMaterialSet = false;
        }
        else {
            ammoChargeTimer = 0f;
            SetAmmoMaterial(ammoDetails.ammoMaterial);
            isAmmoMaterialSet = true;
        }
        ammoRange = ammoDetails.ammoRange;
        this.ammoSpeed = ammoSpeed;
        this.overrideAmmoMovement = overrideAmmoMovement;
        gameObject.SetActive(true);

        //trail
        if (ammoDetails.isAmmoTrail) {
            trailRenderer.gameObject.SetActive(true);
            trailRenderer.emitting = true;
            trailRenderer.material = ammoDetails.ammoTrailMaterial;
            trailRenderer.startWidth = ammoDetails.ammoTrailStartWidth;
            trailRenderer.endWidth = ammoDetails.ammoTrailEndWidth;
            trailRenderer.time = ammoDetails.ammoTrailTime;
        }
        else
        {
            trailRenderer.gameObject.SetActive(false);
            trailRenderer.emitting = false;
        }
    }

    private void SetFireDirection(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        float randomSpread = Random.Range(ammoDetails.ammoSpreadMin, ammoDetails.ammoSpreadMax);
        int spreadToggle = Random.Range(0, 2) * 2 - 1;
        if (weaponAimDirectionVector.magnitude < Settings.useAimAngleDistance) {
            fireDirectionAngle = aimAngle;
        }
        else
        {
            fireDirectionAngle = weaponAimAngle;
        }
        fireDirectionAngle += randomSpread * spreadToggle;
        transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);

        fireDirection = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);

    }

    private void SetAmmoMaterial(Material ammoMaterial)
    {
        spriteRenderer.material = ammoMaterial;
    }
}
