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
    internal bool backBoardBonusActive;
    [SerializeField]
    private GameObject backBoardBonusUI;
    [SerializeField]
    private int backBoardBonusTurnDurationMax;
    [Range(0, 10)]
    private float percentageToActiveBackboardBonus;

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

    public void AddPlayerPoints(int points)
    {
        playerPoints += points;
        playerPointsText.text = playerPoints.ToString();
    }
    public void AddAIPoints(int points)
    {
        AIPoints += points;
        aiPointsText.text = AIPoints.ToString();
    }

    public void DoRandomToDoBackboardBonus()
    {
        if (backBoardBonusActive)
        {
            backBoardBonusTurnDuration--;
            if(backBoardBonusTurnDuration <= 0)
            {
                backBoardBonusActive = false;
                backBoardBonusTurnDuration = backBoardBonusTurnDurationMax;
            }
        }
        else
        {
            int rnd = Random.Range(0, 10);
            if (rnd >= percentageToActiveBackboardBonus)
            {
                backBoardBonusActive = true;
                backBoardBonusUI.SetActive(true);
            }
        }
    }
}
