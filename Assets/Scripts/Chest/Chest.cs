using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Chest : MonoBehaviour, IUseable
{
    [ColorUsage(false, true)]
    [SerializeField] private Color materializeColor;
    [SerializeField] private float materializeTime = 3f;
    [SerializeField] private Transform itemSoawnPoint;
    private int healthPercent;
    private WeaponDetailsSO weaponDetails;
    private int ammoPercent;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private MaterializeEffect materializeEffect;
    private bool isEnabled = false;
    private ChestState chestState = ChestState.closed;
    private GameObject chestItemGameObject;
    private ChestItem chestItem;
    private TextMeshPro messageMeshPro;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        materializeEffect = GetComponent<MaterializeEffect>();
        messageMeshPro = GetComponentInChildren<TextMeshPro>();
    }

    public void Initialize(bool shouldMaterialize, int healthPercent, WeaponDetailsSO weaponDetails, int ammoPercent)
    {
        this.healthPercent = healthPercent;
        this.weaponDetails = weaponDetails;
        this.ammoPercent = ammoPercent;
        if (shouldMaterialize)
        {
            StartCoroutine(MaterializeChest());
        }
        else
        {
            EnableChest();
        }
    }

    private IEnumerator MaterializeChest()
    {
        SpriteRenderer[] spriteRendererArray = new SpriteRenderer[] {spriteRenderer};
        yield return StartCoroutine(materializeEffect.MaterializeRoutine(GameResources.Instance.materializeShader,
            materializeColor, materializeTime, spriteRendererArray, GameResources.Instance.litDefaultMaterial));

        EnableChest();
    }

    private void EnableChest()
    {
        isEnabled = true;
    }

    public void UseItem()
    {
        Debug.Log("Use Chest"); 
        if (!isEnabled) return;
        switch (chestState)
        {
            case ChestState.closed:
                OpenChest();
                break;
            case ChestState.healthItem:
                CollectHealthItem();
                break;
            case ChestState.weaponItem:
                CollectWeaponItem();
                break;
            case ChestState.ammoItem:
                CollectAmmoItem();
                break;
            case ChestState.empty:
                return;
            default:
                return;
        }
    }

    private void OpenChest()
    {
        Debug.Log("Chest Opened");
        animator.SetBool(Settings.use, true);

        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestOpen);

        if (weaponDetails != null)
        {
            if (GameManager.Instance.GetPlayer().IsWeaponHeldByPlayer(weaponDetails)) {
                weaponDetails = null;
            }
        }
        UpdateChestState();
    }

    private void UpdateChestState()
    {

        Debug.Log("healthPercent:" + healthPercent + " weaponDetails:" + weaponDetails + " ammoPercent:" + ammoPercent);
        if (healthPercent != 0) {
            chestState = ChestState.healthItem;
            InstantiateHealthItem();
        }
        else if (weaponDetails != null)
        {
            chestState = ChestState.weaponItem;
            InstantiateWeaponItem();
        }
        else if (ammoPercent != 0)
        {
            chestState = ChestState.ammoItem;
            InstantiateAmmoItem();
        }
        else
        {
            chestState = ChestState.empty;
        }
    }

    private void InstantiateHealthItem()
    {
        InstantiateItem();
        chestItem.Initialize(GameResources.Instance.heartIcon, healthPercent.ToString() + "%", itemSoawnPoint.position, materializeColor);
    }

    private void InstantiateItem()
    {
        chestItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, this.transform);
        chestItem = chestItemGameObject.GetComponent<ChestItem>();
    }

    private void CollectHealthItem()
    {
        if (chestItem == null || !chestItem.isItemMaterialized) return;

        GameManager.Instance.GetPlayer().health.AddHealth(healthPercent);

        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);

        healthPercent = 0;

        Destroy(chestItemGameObject);

        UpdateChestState();
    }

    private void InstantiateAmmoItem()
    {
        InstantiateItem();
        chestItem.Initialize(GameResources.Instance.bulletIcon, ammoPercent.ToString() + "%", itemSoawnPoint.position, materializeColor);
    }

    private void CollectAmmoItem()
    {
        if (chestItem == null || !chestItem.isItemMaterialized) return;

        Player player = GameManager.Instance.GetPlayer();

        player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), ammoPercent);

        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);

        ammoPercent = 0;

        Destroy(chestItemGameObject);

        UpdateChestState();
    }

    private void InstantiateWeaponItem()
    {
        InstantiateItem();
        chestItem.Initialize(weaponDetails.weaponIcon, weaponDetails.weaponName, itemSoawnPoint.position, materializeColor);
    }

    private void CollectWeaponItem()
    {
        if (chestItem == null || !chestItem.isItemMaterialized) return;

        if (!GameManager.Instance.GetPlayer().IsWeaponHeldByPlayer(weaponDetails)) {
            
            GameManager.Instance.GetPlayer().AddWeaponToPlayer(weaponDetails);

            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);
        }
        else
        {
            StartCoroutine(DisplayMessage("WEAPON\nALREADY\nEQUIPPED", 5f));
        }

        weaponDetails = null;

        Destroy(chestItemGameObject);

        UpdateChestState();
    }

    private IEnumerator DisplayMessage(string messageText, float msgDisplayTime)
    {
        messageMeshPro.text = messageText;
        yield return new WaitForSeconds(msgDisplayTime);
        messageMeshPro.text = "";
    }
}
// public interface IUseable
// {
//     void UseItem();
// }