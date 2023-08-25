using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    private const float SIZE_ANCHOR_PC = 800;
    private const float SIZE_ANCHOR_MOBILE = 0;

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

    [Header("3D UI")]
    [SerializeField]
    private GameObject UI3D_imageMobile;
    [SerializeField]
    private GameObject UI3D_imagePC;

    [Header("UI")]
    [SerializeField]
    private RectTransform ballScroll;
    [SerializeField]
    private RectTransform skinScroll;

    void Start()
    {
        saveDataScriptableObject.LoadAllSavedValues();
        AISlider.value = saveDataScriptableObject.AILevel;
        SetValuesOnMenu();
        CheckPlatformToShowPlayerOnUI();
    }

    public void SaveDataInScriptableObject()
    {
        saveDataScriptableObject.SetAILevel((int)AISlider.value);
    }

    public void SetBallMaterial(int materialIndex)
    {
        saveDataScriptableObject.SetBallMaterial(materialIndex);
    }

    public void StartLoadingScene()
    {
        StartCoroutine(LoadingScene());
    }

    /// <summary>
    /// Check the platform and show the right image on UI
    /// </summary>
    private void CheckPlatformToShowPlayerOnUI()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            UI3D_imagePC.SetActive(true);
            ballScroll.offsetMin = new Vector2(SIZE_ANCHOR_PC, ballScroll.offsetMin.y);
            skinScroll.offsetMin = new Vector2(SIZE_ANCHOR_PC, ballScroll.offsetMin.y);
        }
        else if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            UI3D_imageMobile.SetActive(true);
            ballScroll.offsetMin = new Vector2(SIZE_ANCHOR_MOBILE, ballScroll.offsetMin.y);
            skinScroll.offsetMin = new Vector2(SIZE_ANCHOR_MOBILE, ballScroll.offsetMin.y);
        }
    }

    private void SetValuesOnMenu()
    {
        maxScoreText.text = saveDataScriptableObject.maxScore.ToString();
        goldText.text = saveDataScriptableObject.gold.ToString();
    }

    // a fake loading for search an opponent
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
