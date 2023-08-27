using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const float THROWING_BALL_SLIDER_MOBILE_HEIGHT = 800;
    private const float THROWING_BALL_SLIDER_MOBILE_POSITION_Y = 200;
    private const float THROWING_BALL_SLIDER_PC_HEIGHT = 600;
    private const float THROWING_BALL_SLIDER_PC_POSITION_Y = 150;

    private const float TIME_TO_TIMER_FLASH = 10;

    AudioSource audioSource;

    public bool isAndroidSetup;

    [Header("SaveData")]
    [SerializeField]
    private SaveData saveDataScriptableObject;

    [Header("Timer Manager")]
    [SerializeField]
    private float gameTime;
    [SerializeField]
    private float timeToGiveWhenDraw;
    private float timer;

    [Header("Random Backboard Bonus")]
    [SerializeField]
    private GameObject backBoardBonusActivedUI;
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
    public float maxFireBonusTime;
    public float fireBonusDecreasingSpeedDivisor;

    [Header("Lucky Ball Bonus")]
    [Range(0,100)]
    public float percentageToActiveLuckyBall;

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
    [SerializeField]
    private TextMeshProUGUI countdownText;

    [Header("Text Effect")]
    [SerializeField]
    private PointsEffectText pointsEffectText;
    [SerializeField]
    private GameObject backboardBonusShotPointsUI;
    [SerializeField]
    private RectTransform AIPointsUIEffect;
    [SerializeField]
    private TextMeshProUGUI AIPointsUIEffectText;
    [SerializeField]
    private GameObject drawTextPrefab;

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
    public RectTransform throwingBallSliderRect;

    [Header("Values to get points on Throwing Ball Slider, (from 0 to 10)")]
    public float valueTo3PointsMin;
    public float valueTo3PointsMax;
    public float valueTo2PointsMin;
    public float valueTo2PointsMax;
    public float valueToBackboardAndPointsMin;
    public float valueToBackboardAndPointsMax;
    public float valueToHitBasketAndGoOut;

    [Header("Clips")]
    [SerializeField]
    private AudioClip countdownStepClip;
    [SerializeField]
    private AudioClip gameStartClip;
    [SerializeField]
    private AudioClip gameEndClip;
    [SerializeField]
    private AudioClip gameDrawClip;

    internal bool isInGame;

    internal bool isBackBoardBonusActive;
    internal int pointsToGiveOnBackboardBonus;

    private int playerPoints;
    private int AIPoints;

    private PlayerBase winner;
    private int backBoardBonusTurnDuration;
    private int levelOnShotSlider = 0;
    private int nextPointsToIncreaseShotValuesOnSlider;

    private bool isGameStarted;
    private bool isTimerAnimated;

    private void Awake()
    {
        CheckGamePlatformAndEditUI();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        CheckGamePlatformAndEditUI();

        backBoardBonusTurnDuration = backBoardBonusTurnDurationMax;
        timer = gameTime;
        nextPointsToIncreaseShotValuesOnSlider = pointsToIncreaseShotValuesOnSlider[levelOnShotSlider];
        SetPositionOfShotPointsOnSlider();
        StartCoroutine(CountDown(1, gameTime));
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
            else if(timer <= TIME_TO_TIMER_FLASH && !isTimerAnimated)
            {
                // active the blinking animation of timers on UI
                timerImagePlayer.gameObject.GetComponent<Animator>().enabled = true;
                timerImageAI.gameObject.GetComponent<Animator>().enabled = true;
                isTimerAnimated = true;
            }
        }
    }

    // set the position of green and purple indicator on slider
    public void SetPositionOfShotPointsOnSlider()
    {
        perfectShotIndicator.localPosition = new Vector3(perfectShotIndicator.localPosition.x, valueTo3PointsMin * throwingBallSliderRect.rect.height / 10, perfectShotIndicator.localPosition.z);
        backboardShotIndicator.localPosition = new Vector3(backboardShotIndicator.localPosition.x, valueToBackboardAndPointsMin * throwingBallSliderRect.rect.height / 10, backboardShotIndicator.localPosition.z);

        perfectShotIndicator.sizeDelta = new Vector2(perfectShotIndicator.sizeDelta.x, (((valueTo3PointsMax - valueTo3PointsMin) / 10) * throwingBallSliderRect.rect.height));
        backboardShotIndicator.sizeDelta = new Vector2(perfectShotIndicator.sizeDelta.x, (((valueToBackboardAndPointsMax - valueToBackboardAndPointsMin) / 10) * throwingBallSliderRect.rect.height));
    }

    public void AddPlayerPoints(int points, bool isBackboardShot, bool playerHasFireBonusActive, bool playerHasLuckyBall)
    {
        if (!isInGame)
            return;

        // check points to give
        int pointsToGive = points;
        bool mustBackboardEffectSpawn = false;

        // increase the points to give according to all bonus
        pointsToGive += playerHasLuckyBall ? 2 : 0;
        if (isBackboardShot && isBackBoardBonusActive)
        {
            pointsToGive += pointsToGiveOnBackboardBonus;
            DisableBackboardBonus();
            mustBackboardEffectSpawn = true;
        }

        pointsToGive *= playerHasFireBonusActive ? 2 : 1;
        //if (playerHasFireBonusActive)
        //{
        //    pointsToGive *= 2;
        //}

        playerPoints += pointsToGive;
        playerPointsText.text = playerPoints.ToString();

        // raise the indicators on slider when the player reach a set score 
        if(playerPoints >= nextPointsToIncreaseShotValuesOnSlider)
        {
            IncreaseShotValuesOnSlider();
        }

        SpawnPointsOnUI(points, pointsToGive, mustBackboardEffectSpawn, playerHasLuckyBall);
    }

    public void AddAIPoints(int points, bool isBackboardShot, bool hasFireBonusActive, bool hasLuckyBall)
    {
        if (!isInGame)
            return;

        // check points to give
        int pointsToGive = points;
        bool mustBackboardEffectSpawn = false;

        // increase the points to give according to all bonus
        pointsToGive += hasLuckyBall ? 2 : 0;

        if (isBackboardShot && isBackBoardBonusActive)
        {
            pointsToGive += pointsToGiveOnBackboardBonus;
            DisableBackboardBonus();
            mustBackboardEffectSpawn = true;
        }
        pointsToGive *= hasFireBonusActive ? 2 : 1;
        //if (hasFireBonusActive)
        //{
        //    pointsToGive *= 2;
        //}

        AIPoints += pointsToGive;
        aiPointsText.text = AIPoints.ToString();

        StartCoroutine(SpawnPointsOnUIForAI(points, pointsToGive, mustBackboardEffectSpawn, hasLuckyBall));
    }

    public void DoRandomBackboardBonus()
    {
        if (isBackBoardBonusActive)
        {
            // the backboard bonus lasts for a defined number of throws
            backBoardBonusTurnDuration--;
            if(backBoardBonusTurnDuration <= 0)
            {
                DisableBackboardBonus();
            }
        }
        else
        {
            // check if the bonus must be activated
            int rnd1 = Random.Range(0, 101);

            if(rnd1 <= percentageToActiveBackboardBonus)
            {
                // if the bonus is activated check if it will be a +4 or +5
                int rnd2 = Random.Range(0, 101);
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
                Instantiate(backBoardBonusActivedUI, Vector3.zero, Quaternion.identity);
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

    private void SpawnPointsOnUI(int pointsBase, int totalPoints, bool isBackboardShotWithBonusActive, bool hasLuckyBall)
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

            if(hasLuckyBall)
            {
                pointsTextEffect.pointsText.color = Color.blue;
                pointsTextEffect.upText.color = Color.blue;
            }
        }
        pointsTextEffect.pointsText.text = "+ " + totalPoints + " Pts!";
    }

    private IEnumerator SpawnPointsOnUIForAI(int pointsBase, int totalPoints, bool isBackboardShotWithBonusActive, bool hasLuckyBall)
    {
        AIPointsUIEffect.gameObject.SetActive(true);
        AIPointsUIEffectText.text = "+ " + totalPoints + " Pts!";
        Vector2 uiStartPosition = AIPointsUIEffect.anchoredPosition;

        float timer = 0;

        if (pointsBase == 2)
        {
            AIPointsUIEffectText.color = new Color(1.0f, 0.64f, 0.0f);  // orange

            if (isBackboardShotWithBonusActive)
            {
                AIPointsUIEffect.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        else // pointsBase == 3 -> Perfect shot
        {
            if(hasLuckyBall)
            {
                AIPointsUIEffectText.color = Color.blue; ;
            }
            else
            {
                AIPointsUIEffectText.color = Color.green;
            }
        }

        while (timer < 1f)
        {
            timer += Time.deltaTime;
            AIPointsUIEffect.anchoredPosition += new Vector2(Mathf.Sin(Time.time * 10f) * 0.2f, 0);
            yield return null;
        }

        AIPointsUIEffect.anchoredPosition = uiStartPosition;
        AIPointsUIEffect.transform.GetChild(0).gameObject.SetActive(false);
        AIPointsUIEffect.gameObject.SetActive(false);
    }

    // check if the game end or is a draw
    private bool CheckIfGameEnd()
    {
        return playerPoints != AIPoints ? true : false;
    }

    // a manager for end match
    private void EndMatch()
    {
        isInGame = false;

        if (CheckIfGameEnd())
        {
            // game end and go to result
            CheckWinner();
            CheckRecordScore();
            CheckReward();
            audioSource.PlayOneShot(gameEndClip);
        }
        else
        {
            // draw
            Instantiate(drawTextPrefab, Vector3.zero, Quaternion.identity);
            StartCoroutine(CountDown(2, timeToGiveWhenDraw));
            gameTime = timeToGiveWhenDraw;
            audioSource.PlayOneShot(gameDrawClip);
        }
    }

    private IEnumerator CountDown(float timeToWaitBeforeStart ,float matchTimer)
    {
        yield return new WaitForSecondsRealtime(timeToWaitBeforeStart);
        countdownText.transform.parent.gameObject.SetActive(true);

        int countdownValue = 3;

        while (countdownValue > 0)
        {
            countdownText.text = countdownValue.ToString();
            audioSource.PlayOneShot(countdownStepClip);
            yield return new WaitForSecondsRealtime(1);
            countdownValue--;
        }

        countdownText.transform.parent.gameObject.SetActive(false);
        isInGame = true;
        timer = matchTimer;

        if(!isGameStarted)
        {
            audioSource.PlayOneShot(gameStartClip);
            isGameStarted = true;
        }
    }

    private void CheckWinner()
    {
        endUI.SetActive(true);
        endUIText.text = playerPoints > AIPoints ? winText : loseText;
        winner = playerPoints > AIPoints ? player : AI;
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

    // raise the indicators on slider
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

    /// <summary>
    /// edit the UI according to the platform
    /// </summary>
    private void CheckGamePlatformAndEditUI()
    {
        isAndroidSetup = saveDataScriptableObject.isMobileGame;

        if(isAndroidSetup)
        {
            throwingBallSliderRect.sizeDelta = new Vector2(throwingBallSliderRect.sizeDelta.x, THROWING_BALL_SLIDER_MOBILE_HEIGHT);
            throwingBallSliderRect.anchoredPosition = new Vector3(throwingBallSliderRect.anchoredPosition.x, THROWING_BALL_SLIDER_MOBILE_POSITION_Y);
        }
        else
        {
            throwingBallSliderRect.sizeDelta = new Vector2(throwingBallSliderRect.sizeDelta.x, THROWING_BALL_SLIDER_PC_HEIGHT);
            throwingBallSliderRect.anchoredPosition = new Vector2(throwingBallSliderRect.anchoredPosition.x, THROWING_BALL_SLIDER_PC_POSITION_Y);
        }

        SetPositionOfShotPointsOnSlider();
    }
}
