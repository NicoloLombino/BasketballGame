using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUI : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField]
    private float gameTime;
    private float timer;
    [SerializeField]
    private int playerPoints;
    [SerializeField]
    private int AIPoints;


    [SerializeField]
    private TextMeshProUGUI playerPointsText;
    [SerializeField]
    private TextMeshProUGUI aiPointsText;
    [SerializeField]
    private Image timerImage;

    [SerializeField]
    private RectTransform perfectShotIndicator;
    [SerializeField]
    private RectTransform backboardShotIndicator;
    [Range(0, 700)]
    public float sliderValuePerfectShot;
    [Range(0, 700)]
    public float sliderValueBackboardShot;

    [Header("Random Backboard Bonus")]
    internal bool isBackBoardBonusActive;
    internal int pointsToGiveOnBackboardBonus;
    [SerializeField]
    private GameObject backBoardBonusUI4;
    [SerializeField]
    private GameObject backBoardBonusUI5;
    [SerializeField]
    private int backBoardBonusTurnDurationMax;
    [SerializeField, Range(0, 10)]
    private float percentageToActiveBackboardBonus;

    [Header("Fire Bonus")]
    internal bool isFireBonusActive;
    [SerializeField]
    private Slider fireBonusSlider;
    [SerializeField]
    private Image fireImageBackground;

    private int backBoardBonusTurnDuration;

    void Start()
    {
        backBoardBonusTurnDuration = backBoardBonusTurnDurationMax;
        timer = gameTime;
        SetPositionOfShotPointsOnSlider();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        timerImage.fillAmount = timer / gameTime;
    }

    public void SetPositionOfShotPointsOnSlider()
    {
        perfectShotIndicator.localPosition = new Vector3(perfectShotIndicator.localPosition.x, sliderValuePerfectShot, perfectShotIndicator.localPosition.z);
        backboardShotIndicator.localPosition = new Vector3(backboardShotIndicator.localPosition.x, sliderValueBackboardShot, backboardShotIndicator.localPosition.z);
    }

    public float GetSliderValuePerfectShot()
    {
        return sliderValuePerfectShot / 700;
    }
    public float GetSliderValueBackboardShot()
    {
        return sliderValueBackboardShot / 700;
    }

    public void AddPlayerPoints(int points, bool isBackboardShot)
    {
        // check points to give
        int pointsToGive = points;
        if (isFireBonusActive)
        {
            pointsToGive *= 2;
        }
        if (isBackboardShot && isBackBoardBonusActive)
        {
            pointsToGive += pointsToGiveOnBackboardBonus;
        }

        playerPoints += pointsToGive;
        playerPointsText.text = playerPoints.ToString();
        if(!isFireBonusActive)
        {
            fireBonusSlider.value += points;

            if (fireBonusSlider.value >= 10)
            {
                fireBonusSlider.value = 10;
                StartCoroutine(ActiveFireBonus());
            }
        }


    }
    public void AddAIPoints(int points)
    {
        AIPoints += points;
        aiPointsText.text = AIPoints.ToString();
    }

    public void DoRandomBackboardBonus()
    {
        if (isBackBoardBonusActive)
        {
            backBoardBonusTurnDuration--;
            if(backBoardBonusTurnDuration <= 0)
            {
                isBackBoardBonusActive = false;
                backBoardBonusTurnDuration = backBoardBonusTurnDurationMax;
                backBoardBonusUI4.SetActive(false);
                backBoardBonusUI5.SetActive(false);
                pointsToGiveOnBackboardBonus = 0;
            }
        }
        else
        {
            int rnd = Random.Range(0, 11);
            if (rnd >= percentageToActiveBackboardBonus && rnd < 10)
            {
                isBackBoardBonusActive = true;
                backBoardBonusUI4.SetActive(true);
                pointsToGiveOnBackboardBonus = 4;
            }
            else if(rnd >= 10)
            {
                isBackBoardBonusActive = true;
                backBoardBonusUI5.SetActive(true);
                pointsToGiveOnBackboardBonus = 5;
            }

        }
    }

    private IEnumerator ActiveFireBonus()
    {
        isFireBonusActive = true;
        fireImageBackground.enabled = true;

        while(fireBonusSlider.value > 0)
        {
            fireBonusSlider.value -= Time.deltaTime;
            yield return null;
        }

        isFireBonusActive = false;
        fireImageBackground.enabled = false;
    }
}
