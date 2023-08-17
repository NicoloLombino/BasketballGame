using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private SaveData saveDataScriptableObject;
    [SerializeField]
    private Slider AISlider;
    [SerializeField]
    private TextMeshProUGUI goldText;
    [SerializeField]
    private TextMeshProUGUI maxScoreText;

    void Start()
    {
        SetValuesOnMenu();
    }

    public void SaveDataInScriptableObject()
    {
        saveDataScriptableObject.SetAILevel((int)AISlider.value);
    }

    private void SetValuesOnMenu()
    {
        maxScoreText.text = saveDataScriptableObject.maxScore.ToString();
        goldText.text = saveDataScriptableObject.gold.ToString();
    }

    public void GoToGameScene()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
}
