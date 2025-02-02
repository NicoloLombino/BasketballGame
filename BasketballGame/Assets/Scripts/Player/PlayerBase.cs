using System.Collections;
using UnityEngine;

public abstract class PlayerBase : MonoBehaviour
{
    private const float FIRE_BONUS_INCREMENT_2_POINTS_SHOT = 2.5f;
    private const float FIRE_BONUS_INCREMENT_3_POINTS_SHOT = 4.2f;

    protected AudioSource audioSource;

    [Header("reference")]
    [SerializeField]
    protected GameManager gameManager;
    [SerializeField]
    protected SaveData saveDataScriptableObject;

    [Header("Ball components")]
    [SerializeField]
    protected Ball ball;
    [SerializeField]
    protected Transform dribblePosition;
    [SerializeField]
    protected float dribbleHeight;
    [SerializeField]
    protected float throwingDuration;

    [Header("Throwing components")]
    [SerializeField]
    protected Transform throwStartPosition;
    [SerializeField]
    protected Transform throwEndPosition;
    [SerializeField]
    protected PlayerPosition[] playerPositions;

    [Header("Fire bonus components")]
    [SerializeField]
    protected GameObject fireOnBallParticles;
    [SerializeField]
    protected GameObject fireBonusUI;
    [SerializeField]
    private AudioClip fireBonusEndClip;
    protected bool isFireBonusActive;
    protected float fireBonusValue;

    [Header("Lucky Ball components")]
    protected bool isLuckyBallActive;

    protected Transform throwEndPositionChild;

    protected int currentPlayerPosition = 0;
    protected float throwingTimer;
    protected bool isThrowingBall;

    protected bool doBackboardShot;
    protected int pointsEarnedBase;
    protected bool makePoints;

    internal bool ignoreInputs;

    private float fireValueDecreasingDivisor;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    protected virtual void Start()
    {
        SetPlayerPosition();
        fireValueDecreasingDivisor = gameManager.fireBonusDecreasingSpeedDivisor;

        ball.baseMaterial = saveDataScriptableObject.ballChosenMaterial;
    }
    protected virtual void Update()
    {
        if (!ignoreInputs)
        {
            ball.transform.position = dribblePosition.position + Vector3.up * 0.5f + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * dribbleHeight));
        }

        if (!isFireBonusActive)
        {
            fireBonusValue -= Time.deltaTime / fireValueDecreasingDivisor;
        }
    }

    protected void SetBallAnimationPositionsOnThrowing(Transform endPos1, Transform endPos2)
    {
        throwEndPosition = endPos1;
        throwEndPositionChild = endPos2;
    }

    /// <summary>
    /// to move player in one direction after a point
    /// direction = 1 or -1   --> 1 = player, -1 = AI
    /// </summary>
    /// <param name="direction"></param>
    protected void MovePlayerToNextPosition(int direction)
    {
        StartCoroutine(MovingPlayerToNextPosition(direction));
    }
    protected void SetThrowValues(int pointsToGive, bool isBackboardShot)
    {
        pointsEarnedBase = pointsToGive;
        doBackboardShot = isBackboardShot;
    }

    private IEnumerator MovingPlayerToNextPosition(int direction)
    {
        playerPositions[currentPlayerPosition].isFull = false;
        currentPlayerPosition += direction;

        if (currentPlayerPosition >= playerPositions.Length)
        {
            currentPlayerPosition = 0;
        }
        else if(currentPlayerPosition < 0)
        {
            currentPlayerPosition = playerPositions.Length - 1;
        }

        if (playerPositions[currentPlayerPosition].isFull)
        {
            currentPlayerPosition += direction;
            if (currentPlayerPosition >= playerPositions.Length)
            {
                currentPlayerPosition = 0;
            }
            else if (currentPlayerPosition < 0)
            {
                currentPlayerPosition = playerPositions.Length - 1;
            }
        }

        playerPositions[currentPlayerPosition].isFull = true;
        float movingTimer = 0;
        float movingPercent = 0;
        Vector3 startPosition = transform.position;
        Vector3 startRotation = transform.eulerAngles;
        while (movingPercent < 1)
        {
            movingTimer += Time.deltaTime;
            movingPercent = movingTimer / 0.2f;
            transform.position = Vector3.Lerp(startPosition, playerPositions[currentPlayerPosition].transform.position, movingPercent);
            transform.eulerAngles = Vector3.Lerp(startRotation, playerPositions[currentPlayerPosition].transform.eulerAngles, movingPercent);
            yield return null;
        }

        transform.LookAt(throwEndPosition, Vector3.up);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        ignoreInputs = false;
    }

    /// <summary>
    /// check the type of ball shot and set the various parameters
    /// </summary>
    /// <param name="throwPower"> the power of the shot </param>
    /// <param name="hasFireBonus"> if the player has fire bonus active </param>
    protected void CheckThrowingResult(float throwPower, bool hasFireBonus)
    {
        // get the values of the shots and divides them by 10 to convert in range 0 - 1
        float basketboard = gameManager.valueToHitBasketAndGoOut / 10; // 0 points
        float twoPointsLess = gameManager.valueTo2PointsMin / 10; // 2 points
        float perfectShotValueMin = gameManager.valueTo3PointsMin / 10; // 3 points
        float perfectShotValueMax = gameManager.valueTo3PointsMax / 10; // 3 points
        float twoPointsMore = gameManager.valueTo2PointsMax / 10; // hit the backboard left or right
        float backboardShotValue = gameManager.valueToBackboardAndPointsMin / 10; // 2 points backboard
        float backboardMore = gameManager.valueToBackboardAndPointsMax / 10; // 2 points

        int points = 0;
        bool isBackboardShot = false;
        int ballThrowingAnimationIndex;

        if (throwPower < basketboard)
        {
            // NO, no points
            DisableFireBonus();
            ball.hasMakeSound = true;
            ballThrowingAnimationIndex = 0;
        }
        else if (throwPower >= basketboard && throwPower < twoPointsLess)
        {
            // basket board, no points
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(0, 0, hasFireBonus);
            DisableFireBonus();
            ballThrowingAnimationIndex = 1;
        }
        else if (throwPower >= twoPointsLess && throwPower < perfectShotValueMin)
        {
            // enter in basket --> 2 points
            points = 2;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
            makePoints = true;
            ballThrowingAnimationIndex = 2;
        }
        else if (throwPower >= perfectShotValueMin && throwPower <= perfectShotValueMax)
        {
            // enter in basket --> 3 points
            points = 3;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 3, hasFireBonus);
            makePoints = true;
            ballThrowingAnimationIndex = 3;
        }
        else if (throwPower > perfectShotValueMax && throwPower <= twoPointsMore)
        {
            // enter in basket --> 2 points
            points = 2;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
            makePoints = true;
            ballThrowingAnimationIndex = 2;
        }
        else if (throwPower > twoPointsMore && throwPower < backboardShotValue)
        {
            // hit backboard and go out --> NO points
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
            DisableFireBonus();
            ballThrowingAnimationIndex = 4;
        }
        else if (throwPower >= backboardShotValue && throwPower <= backboardMore)
        {
            // hit backboard and enter in basket --> 2 points
            points = 2;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(3, 2, hasFireBonus);
            isBackboardShot = true;
            makePoints = true;
            ballThrowingAnimationIndex = 5;
        }
        else
        {
            // hit backboard and go out --> NO points
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
            points = 0;
            DisableFireBonus();
            ballThrowingAnimationIndex = 6;
        }

        SetBallAnimationPositionsOnThrowing(
            ball.ballThrowingPositions[currentPlayerPosition].ballPositions[ballThrowingAnimationIndex],
            ball.ballThrowingPositions[currentPlayerPosition].ballPositions[ballThrowingAnimationIndex].GetChild(0));

        SetThrowValues(points, isBackboardShot);
        //CheckFireBonus(points);
    }

    /// <summary>
    /// reset all value and prepare the player for the next shot
    /// </summary>
    /// <param name="directionOfMovement"> the direction of movement, it can be 1 or -1 </param>
    protected virtual void ResetShot(int directionOfMovement)
    {
        CheckFireBonus(pointsEarnedBase);
        ball.transform.position = dribblePosition.position + Vector3.up * 0.7f;
        pointsEarnedBase = 0;
        throwingTimer = 0;
        ball.hasMakeSound = false;
        doBackboardShot = false;
        gameManager.DoRandomBackboardBonus();
        RandomLuckyBallBonus();

        if (makePoints)
        {
            MovePlayerToNextPosition(directionOfMovement);
            makePoints = false;
        }
        else
        {
            ignoreInputs = false;
        }
    }

    protected virtual void CheckFireBonus(int points)
    {
        if (points == 0)
            return;

        if (!isFireBonusActive)
        {
            // check the value to increment on fire bonus bar
            fireBonusValue += points == 2 ? 
                FIRE_BONUS_INCREMENT_2_POINTS_SHOT : FIRE_BONUS_INCREMENT_3_POINTS_SHOT;

            if (fireBonusValue >= gameManager.maxFireBonusTime)
            {
                fireBonusValue = gameManager.maxFireBonusTime;
                StartCoroutine(StartFireBonus());
            }
        }
    }

    protected IEnumerator StartFireBonus()
    {
        isFireBonusActive = true;
        fireOnBallParticles.SetActive(true);
        fireBonusUI.SetActive(true);

        while (fireBonusValue > 0)
        {
            fireBonusValue -= Time.deltaTime;
            yield return null;
        }

        while (ignoreInputs)
        {
            yield return null;
        }

        DisableFireBonus();
    }

    protected virtual void DisableFireBonus()
    {
        if (isFireBonusActive)
        {
            audioSource.PlayOneShot(fireBonusEndClip);
        }

        fireBonusValue = 0;
        isFireBonusActive = false;
        fireOnBallParticles.SetActive(false);
        fireBonusUI.SetActive(false);
    }

    protected void RandomLuckyBallBonus()
    {
        if(isLuckyBallActive)
        {
            DisableLuckyBallBonus();
        }
        else
        {
            // check if the bonus must be activated only if backboard bonus is not active
            int rnd = Random.Range(0, 101);

            if (rnd < gameManager.percentageToActiveLuckyBall)
            {
                ActiveLuckyBall();
            }
        }
    }

    protected virtual void ActiveLuckyBall()
    {
        isLuckyBallActive = true;
        ball.HandleLuckyBallBonus(true);
    }

    protected void DisableLuckyBallBonus()
    {
        isLuckyBallActive = false;
        ball.HandleLuckyBallBonus(false);
    }

    /// <summary>
    /// Give a random position to the player from the array of positions
    /// if the selected position is occupied, assign a new one
    /// </summary>
    private void SetPlayerPosition()
    {
        currentPlayerPosition = Random.Range(0, playerPositions.Length);

        // check if the position is empty
        if (playerPositions[currentPlayerPosition].isFull)
        {
            if (currentPlayerPosition != playerPositions.Length - 1)
            {
                currentPlayerPosition += 1;
            }
            else
            {
                currentPlayerPosition -= 1;
            }
        }

        // set the position and make the player look the basket
        transform.position = playerPositions[currentPlayerPosition].transform.position;
        transform.LookAt(throwEndPosition, Vector3.up);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        playerPositions[currentPlayerPosition].isFull = true;
    }
}
