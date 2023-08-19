using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField, Range(0, 10)]
    private float percentageToActiveBackboardBonus;

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
    private Player player;
    [SerializeField]
    private Player AI;

    [Header("Particles")]
    [SerializeField]
    private Transform basketPosition;
    [SerializeField]
    private GameObject PointsAndFireBasketParticles3;
    [SerializeField]
    private GameObject PointsParticles3;
    [SerializeField]
    private GameObject PointsParticles2;

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

    internal bool isFireBonusActive;
    internal bool isBackBoardBonusActive;
    internal int pointsToGiveOnBackboardBonus;
    internal bool isInGame;

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


    private Player winner;
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

    // Update is called once per frame
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
        perfectShotIndicator.localPosition = new Vector3(perfectShotIndicator.localPosition.x, sliderValuePerfectShot, perfectShotIndicator.localPosition.z);
        backboardShotIndicator.localPosition = new Vector3(backboardShotIndicator.localPosition.x, sliderValueBackboardShot, backboardShotIndicator.localPosition.z);
    }

    public float GetSliderValuePerfectShot()
    {
        return sliderValuePerfectShot / 800;
    }
    public float GetSliderValueBackboardShot()
    {
        return sliderValueBackboardShot / 800;
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

        //CheckParticlesToUseOnBasket(points, isFireBonusActive);

        if(playerPoints >= nextPointsToIncreaseShotValuesOnSlider)
        {
            IncreaseShotValuesOnSlider();
        }
    }

    private void CheckParticlesToUseOnBasket(int points, bool fireActive)
    {
        GameObject particlesOnBasket = new GameObject();
        if (points == 2)
        {
            particlesOnBasket = Instantiate(PointsParticles2, basketPosition.position, Quaternion.identity);
            // rotate the particles
            particlesOnBasket.transform.eulerAngles += Vector3.right * -90;
        }
        else if(points == 3)
        {
            if(fireActive)
            {
                particlesOnBasket = Instantiate(PointsAndFireBasketParticles3, basketPosition.position, Quaternion.identity);
            }
            else
            {
                particlesOnBasket = Instantiate(PointsParticles3, basketPosition.position, Quaternion.identity);
            }
        }
        Destroy(particlesOnBasket, 1);
    }
    public void AddAIPoints(int points, bool isBackboardShot)
    {
        // check points to give
        int pointsToGive = points;

        if (isBackboardShot && isBackBoardBonusActive)
        {
            pointsToGive += pointsToGiveOnBackboardBonus;
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
            int rnd = Random.Range(0, 15);
            if (rnd >= percentageToActiveBackboardBonus && rnd < 13)
            {
                isBackBoardBonusActive = true;
                backBoardBonusUI4.SetActive(true);
                pointsToGiveOnBackboardBonus = 4;
            }
            else if(rnd >= 13)
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
        player.HandleEndGame();
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
        }
    }

    private void CheckReward()
    {
        int goldToGive = 0;
        if(winner == player)
        {
            goldToGive = playerPoints * 2;
            rewardText.text = rewardText1 + " " + goldToGive.ToString() + " " + rewardText2;
        }
        else if (winner == AI)
        {
            goldToGive = Mathf.FloorToInt(playerPoints / 2);
            rewardText.text = rewardText1 + " " + goldToGive.ToString() + " " + rewardText2;
        }
        else
        {
            goldToGive = playerPoints;
            rewardText.text = rewardText1 + " " + goldToGive.ToString() + " " + rewardText2;
        }
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
