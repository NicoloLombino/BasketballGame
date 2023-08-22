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
    [Tooltip("if the backboard bonus is actived this determines if the bonus is + 4 or + 5")]
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
    [SerializeField]
    private GameObject fireBonusUI;
    public float maxFireBonusTime;
    public float fireBonusDecreasingSpeedDivisor;

    [Header("Reference")]
    [SerializeField]
    private PlayerBase player;
    [SerializeField]
    private PlayerBase AI;
    [SerializeField]
    private Transform basketTransform;

    [Header("UI components")]
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

    [Header("UI 3D")]
    [SerializeField]
    private PointsEffectText pointsEffectText;
    [SerializeField]
    private GameObject backboardBonusShotPointsUI;

    [Header("UI edit texts")]
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
    [SerializeField]
    private int[] pointsToIncreaseShotValuesOnSlider;
    [SerializeField]
    private float shotIndicatorPositionInSliderIncrement;

    [Header("Values to get points on Throwing Ball Slider, (from 0 to 10)")]
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
    private int nextPointsToIncreaseShotValuesOnSlider;

    void Start()
    {
        isInGame = true;
        backBoardBonusTurnDuration = backBoardBonusTurnDurationMax;
        timer = gameTime;
        nextPointsToIncreaseShotValuesOnSlider = pointsToIncreaseShotValuesOnSlider[levelOnShotSlider];
        fireBonusSlider.maxValue = maxFireBonusTime;
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

        if (!isFireBonusActive)
        {
            fireBonusSlider.value -= Time.deltaTime / fireBonusDecreasingSpeedDivisor;
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
            DisableBackboardBonus();
        }
        if (playerHasFireBonusActive)
        {
            pointsToGive *= 2;
        }

        playerPoints += pointsToGive;
        playerPointsText.text = playerPoints.ToString();
        if(!isFireBonusActive) 
        {
            fireBonusSlider.value += points == 2 ? 2.5f : 4f;

            if (fireBonusSlider.value >= maxFireBonusTime)
            {
                fireBonusSlider.value = maxFireBonusTime;
                StartCoroutine(ActiveFireBonus());
            }
        }

        if(playerPoints >= nextPointsToIncreaseShotValuesOnSlider)
        {
            IncreaseShotValuesOnSlider();
        }

        SpawnPointsOnUI(points, pointsToGive, isBackboardShot && isBackBoardBonusActive);
    }

    public void AddAIPoints(int points, bool isBackboardShot, bool hasFireBonusActive)
    {
        // check points to give
        int pointsToGive = points;

        if (isBackboardShot && isBackBoardBonusActive)
        {
            pointsToGive += pointsToGiveOnBackboardBonus;
            DisableBackboardBonus();
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
                DisableBackboardBonus();
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
                    backBoardBonusUI5.SetActive(true);
                    pointsToGiveOnBackboardBonus = 5;
                }
                else
                {
                    backBoardBonusUI4.SetActive(true);
                    pointsToGiveOnBackboardBonus = 4;
                }

                isBackBoardBonusActive = true;
                backBoardBonusTurnDuration = backBoardBonusTurnDurationMax;
            }
        }
    }

    private void DisableBackboardBonus()
    {
        isBackBoardBonusActive = false;
        backBoardBonusTurnDuration = backBoardBonusTurnDurationMax;
        backBoardBonusUI4.SetActive(false);
        backBoardBonusUI5.SetActive(false);
        pointsToGiveOnBackboardBonus = 0;
    }

    private IEnumerator ActiveFireBonus()
    {
        isFireBonusActive = true;
        fireImageBackground.color = fireImageBackgroundColorWhenActive;
        fireOnBallParticles.SetActive(true);
        fireBonusUI.SetActive(true);

        while (fireBonusSlider.value > 0)
        {
            fireBonusSlider.value -= Time.deltaTime;
            yield return null;
        }

        while (player.ignoreInputs)
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
        fireBonusUI.SetActive(false);
    }

    private void SpawnPointsOnUI(int pointsBase, int totalPoints, bool isBackboardShotWithBonusActive)
    {
        PointsEffectText pointsTextEffect = Instantiate(pointsEffectText, basketTransform.position + Vector3.up, Quaternion.identity);
        if (pointsBase == 2)
        {
            pointsTextEffect.pointsText.color = new Color(1.0f, 0.64f, 0.0f);  // orange


            if(isBackboardShotWithBonusActive)
            {
                GameObject backboardBonusTextEffect = Instantiate(backboardBonusShotPointsUI, basketTransform.position + Vector3.up, Quaternion.identity);
            }
        }
        else // pointsBase == 3 -> Perfect shot
        {
            pointsTextEffect.pointsText.color = Color.green;
            pointsTextEffect.upText.gameObject.SetActive(true);
        }

        pointsTextEffect.pointsText.text = "+ " + totalPoints + " Pts!";
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
        if (levelOnShotSlider < pointsToIncreaseShotValuesOnSlider.Length - 1)
        {
            levelOnShotSlider++;
            nextPointsToIncreaseShotValuesOnSlider = pointsToIncreaseShotValuesOnSlider[levelOnShotSlider];

            valueTo3PointsMin += shotIndicatorPositionInSliderIncrement;
            valueTo3PointsMax += shotIndicatorPositionInSliderIncrement;
            valueTo2PointsMin += shotIndicatorPositionInSliderIncrement;
            valueTo2PointsMax += shotIndicatorPositionInSliderIncrement;
            valueToBackboardAndPointsMin += shotIndicatorPositionInSliderIncrement;
            valueToBackboardAndPointsMax += shotIndicatorPositionInSliderIncrement;
            valueToHitBasketAndGoOut += shotIndicatorPositionInSliderIncrement;

            SetPositionOfShotPointsOnSlider();
        }
        else
        {
            return;
        }
    }
}
