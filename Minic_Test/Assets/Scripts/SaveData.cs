using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SaveData : ScriptableObject
{
    [Header("Platform")]
    public bool isMobileGame;

    [Header("Data")]
    public int gold;
    public int maxScore;
    public int AILevel;
    public int playerMaterialIndex;
    public int ballMaterialIndex;

    [Header("Player Materials")]
    public Material playerChosenMaterial;
    public Material[] playerMaterials;

    [Header("Ball Materials")]
    public Material ballChosenMaterial;
    public Material[] ballMaterials;   

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
        ballChosenMaterial = ballMaterials[materialIndex];
    }

    public void SetPlayerMaterial(int materialIndex)
    {
        playerMaterialIndex = materialIndex;
        PlayerPrefs.SetInt("PlayerMaterialIndex", materialIndex);
        playerChosenMaterial = playerMaterials[materialIndex];
    }

    public void LoadAllSavedValues()
    {
        playerMaterialIndex = PlayerPrefs.GetInt("PlayerMaterialIndex");
        ballMaterialIndex = PlayerPrefs.GetInt("BallMaterialIndex");
        ballChosenMaterial = ballMaterials[ballMaterialIndex];
        playerChosenMaterial = playerMaterials[playerMaterialIndex];
        gold = PlayerPrefs.GetInt("Gold");
        maxScore = PlayerPrefs.GetInt("MaxScore");
        AILevel = PlayerPrefs.GetInt("AILevel");

    }
}


