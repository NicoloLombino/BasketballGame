using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private SaveData saveDataScriptableObject;
    [SerializeField]
    private SceneLoader sceneLoader;

    [Header("UI components")]
    [SerializeField]
    private Slider AISlider;
    [SerializeField]
    private TextMeshProUGUI goldText;
    [SerializeField]
    private TextMeshProUGUI maxScoreText;
    [SerializeField]
    private RectTransform loadingImageBase;
    [SerializeField]
    private Image loadingImage;

    void Start()
    {
        saveDataScriptableObject.LoadAllSavedValues();
        AISlider.value = saveDataScriptableObject.AILevel;
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

    public void SetBallMaterial(int materialIndex)
    {
        saveDataScriptableObject.SetBallMaterial(materialIndex);
    }

    public void StartLoadingScene()
    {
        StartCoroutine(LoadingScene());
    }

    private IEnumerator LoadingScene()
    {
        while(loadingImage.fillAmount < 1)
        {
            loadingImageBase.localEulerAngles += Vector3.forward * 500 * Time.deltaTime;
            loadingImage.fillAmount += Time.deltaTime / 3;
            yield return null;
        }

        sceneLoader.GoToGameScene();
    }
}
