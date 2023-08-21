using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool isAndroidSetup;

    [Header("SaveData")]
    [SerializeField]
    private SaveData saveDataScriptableObject;

    [Header("Game Manager")]
    [SerializeField]
    private float gameTime;
    private float timer;
    [SerializeField]
    private int playerPoints;
    [SerializeField]
    private int AIPoints;

    [Header("Random Backboard Bonus")]
    [SerializeField]
    private GameObject backBoardBonusUI4;
    [SerializeField]
    private GameObject backBoardBonusUI5;
    [SerializeField]
    private int backBoardBonusTurnDurationMax;
    [SerializeField, Range(0, 100)]
    private int percentageToActiveBackboardBonus;
    [SerializeField, Range(0, 100)]
    private int percentageToActiveBackboardBonus5;

    [Header("Fire Bonus")]
    [SerializeField]
    private Slider fireBonusSlider;
    [SerializeField]
    private Image fireImageBackground;
    [SerializeField]
    private GameObject fireOnBallParticles;
    [SerializeField]
    private Color fireImageBackgroundColorWhenActive;

    [Header("Reference")]
    [SerializeField]
    private PlayerBase player;
    [SerializeField]
    private PlayerBase AI;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI playerPointsText;
    [SerializeField]
    private TextMeshProUGUI aiPointsText;
    [SerializeField]
    private Image timerImagePlayer;
    [SerializeField]
    private Image timerImageAI;
    [SerializeField]
    private GameObject endUI;
    [SerializeField]
    private TextMeshProUGUI endUIText;
    [SerializeField]
    private TextMeshProUGUI rewardText;
    [SerializeField]
    private GameObject newRecordText;
    [SerializeField]
    private string winText;
    [SerializeField]
    private string loseText;
    [SerializeField]
    private string drawText;
    [SerializeField]
    private string rewardText1;
    [SerializeField]
    private string rewardText2;

    [Header("Throwing Ball Slider")]
    [SerializeField]
    private RectTransform perfectShotIndicator;
    [SerializeField]
    private RectTransform backboardShotIndicator;
    [Range(0, 700)]
    public float sliderValuePerfectShot;
    [Range(0, 700)]
    public float sliderValueBackboardShot;
    [SerializeField]
    private int nextPointsToIncreaseShotValuesOnSlider;
    [SerializeField]
    private int[] pointsToIncreaseShotValuesOnSlider;

    [Header("Values to get points on Throwing Ball Slider, (from 0 to 1)")]
    public float valueTo3PointsMin;  // P
    public float valueTo3PointsMax;  // P + x
    public float valueTo2PointsMin;  // P - y
    public float valueTo2PointsMax;  // P + x + y
    public float valueToBackboardAndPointsMin;  // > P + x + y && < B + z
    public float valueToBackboardAndPointsMax;  // B + z
    public float valueToHitBasketAndGoOut; // < P - y


    internal bool isFireBonusActive;
    internal bool isBackBoardBonusActive;
    internal int pointsToGiveOnBackboardBonus;
    internal bool isInGame;

    private PlayerBase winner;
    private int backBoardBonusTurnDuration;
    private int levelOnShotSlider = 0;

    void Start()
    {
        isInGame = true;
        backBoardBonusTurnDuration = backBoardBonusTurnDurationMax;
        timer = gameTime;
        nextPointsToIncreaseShotValuesOnSlider = pointsToIncreaseShotValuesOnSlider[levelOnShotSlider];
        SetPositionOfShotPointsOnSlider();
    }

    void Update()
    {
        if(isInGame)
        {
            timer -= Time.deltaTime;
            timerImagePlayer.fillAmount = timer / gameTime;
            timerImageAI.fillAmount = timer / gameTime;
            if (timer <= 0)
            {
                EndMatch();
            }
        }
    }

    public void SetPositionOfShotPointsOnSlider()
    {
        perfectShotIndicator.localPosition = new Vector3(perfectShotIndicator.localPosition.x, valueTo3PointsMin * 800 / 10, perfectShotIndicator.localPosition.z);
        backboardShotIndicator.localPosition = new Vector3(backboardShotIndicator.localPosition.x, valueToBackboardAndPointsMin * 800 / 10, backboardShotIndicator.localPosition.z);

        perfectShotIndicator.sizeDelta = new Vector2(perfectShotIndicator.sizeDelta.x, (((valueTo3PointsMax - valueTo3PointsMin) / 10) * 800));
        backboardShotIndicator.sizeDelta = new Vector2(perfectShotIndicator.sizeDelta.x, (((valueToBackboardAndPointsMax - valueToBackboardAndPointsMin) / 10) * 800));
    }

    public void AddPlayerPoints(int points, bool isBackboardShot, bool playerHasFireBonusActive)
    {
        // check points to give
        int pointsToGive = points;

        if (isBackboardShot && isBackBoardBonusActive)
        {
            pointsToGive += pointsToGiveOnBackboardBonus;
        }
        if (playerHasFireBonusActive)
        {
            pointsToGive *= 2;
        }

        playerPoints += pointsToGive;
        playerPointsText.text = playerPoints.ToString();
        if(!isFireBonusActive)
        {
            fireBonusSlider.value += points;

            if (fireBonusSlider.value >= 9)
            {
                fireBonusSlider.value = 9;
                StartCoroutine(ActiveFireBonus());
            }
        }

        if(playerPoints >= nextPointsToIncreaseShotValuesOnSlider)
        {
            IncreaseShotValuesOnSlider();
        }
    }

    public void AddAIPoints(int points, bool isBackboardShot, bool hasFireBonusActive)
    {
        // check points to give
        int pointsToGive = points;

        if (isBackboardShot && isBackBoardBonusActive)
        {
            pointsToGive += pointsToGiveOnBackboardBonus;
        }
        if (hasFireBonusActive)
        {
            pointsToGive *= 2;
        }

        AIPoints += pointsToGive;
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
            int rnd1 = Random.Range(0, 100);
            Debug.Log("rand back 1 = " + rnd1);
            if(rnd1 <= percentageToActiveBackboardBonus)
            {
                int rnd2 = Random.Range(0, 100);
                Debug.Log("rand back 2 = " + rnd2);
                if (rnd2 <= percentageToActiveBackboardBonus5)
                {
                    isBackBoardBonusActive = true;
                    backBoardBonusUI5.SetActive(true);
                    pointsToGiveOnBackboardBonus = 5;
                }
                else
                {
                    isBackBoardBonusActive = true;
                    backBoardBonusUI4.SetActive(true);
                    pointsToGiveOnBackboardBonus = 4;
                }
            }
            //if (rnd >= percentageToActiveBackboardBonus && rnd < 13)
            //{
            //    isBackBoardBonusActive = true;
            //    backBoardBonusUI4.SetActive(true);
            //    pointsToGiveOnBackboardBonus = 4;
            //}
            //else if(rnd >= 13)
            //{
            //    isBackBoardBonusActive = true;
            //    backBoardBonusUI5.SetActive(true);
            //    pointsToGiveOnBackboardBonus = 5;
            //}
        }
    }

    private IEnumerator ActiveFireBonus()
    {
        isFireBonusActive = true;
        fireImageBackground.color = fireImageBackgroundColorWhenActive;
        fireOnBallParticles.SetActive(true);

        while (fireBonusSlider.value > 0)
        {
            fireBonusSlider.value -= Time.deltaTime;
            yield return null;
        }
        
        while(player.ignoreInputs)
        {
            yield return null;
        }

        DisableFireBonus();
    }

    public void DisableFireBonus()
    {
        fireBonusSlider.value = 0;
        isFireBonusActive = false;
        fireImageBackground.color = Color.white;
        fireOnBallParticles.SetActive(false);
    }

    private void EndMatch()
    {
        isInGame = false;
        CheckWinner();
        CheckRecordScore();
        CheckReward();
    }

    private void CheckWinner()
    {
        endUI.SetActive(true);
        if (playerPoints != AIPoints)
        {
            endUIText.text = playerPoints > AIPoints ? winText : loseText;
            winner = playerPoints > AIPoints ? player : AI;
        }
        else
        {
            endUIText.text = drawText;
            winner = null;
        }
    }

    private void CheckReward()
    {
        int goldToGive = 0;
        if(winner == player)
        {
            goldToGive = playerPoints * 2;
        }
        else if (winner == AI)
        {
            goldToGive = Mathf.FloorToInt(playerPoints / 2);
        }
        else // if (winner == null)
        {
            goldToGive = playerPoints;
        }

        rewardText.text = rewardText1 + " " + goldToGive.ToString() + " " + rewardText2;
        saveDataScriptableObject.AddGold(goldToGive);
    }

    private void CheckRecordScore()
    {
        if(playerPoints > saveDataScriptableObject.maxScore)
        {
            saveDataScriptableObject.SetNewMaxScore(playerPoints);
            newRecordText.SetActive(true);
        }
    }

    private void IncreaseShotValuesOnSlider()
    {
        if (levelOnShotSlider >= pointsToIncreaseShotValuesOnSlider.Length - 1)
            return;

        levelOnShotSlider++;
        nextPointsToIncreaseShotValuesOnSlider = pointsToIncreaseShotValuesOnSlider[levelOnShotSlider];

        sliderValuePerfectShot += 50;
        sliderValueBackboardShot += 50;
        SetPositionOfShotPointsOnSlider();
    }
}
