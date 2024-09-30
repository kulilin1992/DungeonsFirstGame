using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    private TextMeshProUGUI scoreText;

    private void Awake()
    {
        scoreText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnScoreChanged += UpdateScoreText;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnScoreChanged -= UpdateScoreText;
    }

    private void UpdateScoreText(ScoreChangedArgs scoreChangedArgs)
    {
        //Debug.Log("scoreText" + scoreText);
        scoreText.text = "分数: " + scoreChangedArgs.score.ToString("###,###0") + 
        "\nMULTIPLIER: x" + scoreChangedArgs.multiplier;
    }
}
