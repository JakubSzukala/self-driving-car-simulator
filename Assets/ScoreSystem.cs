using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public float Score
    { get; private set; }

    [SerializeField] private TMP_Text UIScore;

    void Start()
    {
        Score = 0f;
        UIScoreUpdate();
    }

    public void IncreaseScore(float increment)
    {
        Score += increment;
        UIScoreUpdate();
    }

    public void DecreaseScore(float decrement)
    {
        Score -= decrement;
        UIScoreUpdate();
    }

    public void ResetScore() => Score = 0f;

    private void UIScoreUpdate()
    {
        UIScore.text = $"Score: {Score}";
    }
}
