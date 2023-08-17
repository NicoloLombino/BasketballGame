using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SaveData : ScriptableObject
{
    public int gold;
    public int maxScore;
    public int AILevel;

    public void AddGold(int goldToAdd)
    {
        gold += goldToAdd;
    }

    public void SetNewMaxScore(int newMaxScore)
    {
        maxScore = newMaxScore;
    }

    public void SetAILevel(int level)
    {
        AILevel = level;
    }
}


