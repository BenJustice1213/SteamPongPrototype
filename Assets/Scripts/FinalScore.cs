using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalScore : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;

    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            int score = ScoreManager.Instance.score;
            finalScoreText.text = $"Your Score Was: {score}";
        }
        else
        {
            finalScoreText.text = "Your Score Was: 0";
        }
    }
}