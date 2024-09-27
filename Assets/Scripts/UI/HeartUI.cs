using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HeartUI : MonoBehaviour
{
    private List<GameObject> healthHeartsList = new List<GameObject>();

    private void OnEnable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthEvent += UpdateHealthUI;
    }

    private void OnDisable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthEvent -= UpdateHealthUI;
    }

    private void UpdateHealthUI(HealthEvent healthEvent, HealthEventArgs args)
    {
        SetHealthBar(args);
    }

    private void SetHealthBar(HealthEventArgs args)
    {
        ClearHealthBar();

        int healthHearts = Mathf.CeilToInt(args.healthPercent * 100f / 20f);

        for (int i = 0; i < healthHearts; i++)
        {
            GameObject heart = Instantiate(GameResources.Instance.heartPrefab, transform);

            heart.GetComponent<RectTransform>().anchoredPosition = new Vector2(Settings.uiHeartSpacing * i, 0f);
            healthHeartsList.Add(heart);
        }
    }

    private void ClearHealthBar()
    {
        foreach (GameObject heartIcon in healthHeartsList)
        {
            Destroy(heartIcon);
        }
        healthHeartsList.Clear();
    }
}
