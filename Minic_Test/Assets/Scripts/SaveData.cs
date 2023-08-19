using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SaveData : ScriptableObject
{
    public int gold;
    public int maxScore;
    public int AILevel;
    public int ballMaterialIndex;


    public void AddGold(int goldToAdd)
    {
        gold += goldToAdd;
        PlayerPrefs.SetInt("Gold", gold);
    }

    public void SetNewMaxScore(int newMaxScore)
    {
        maxScore = newMaxScore;
        PlayerPrefs.SetInt("MaxScore", maxScore);
    }

    public void SetAILevel(int level)
    {
        AILevel = level;
        PlayerPrefs.SetInt("AILevel", AILevel);
    }

    public void SetBallMaterial(int materialIndex)
    {
        ballMaterialIndex = materialIndex;
        PlayerPrefs.SetInt("BallMaterialIndex", materialIndex);
    }

    public void LoadAllSavedValues()
    {
        gold = PlayerPrefs.GetInt("Gold");
        maxScore = PlayerPrefs.GetInt("MaxScore");
        AILevel = PlayerPrefs.GetInt("AILevel");
        ballMaterialIndex = PlayerPrefs.GetInt("BallMaterialIndex");
    }
}


