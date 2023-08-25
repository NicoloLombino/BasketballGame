using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
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
    protected Transform[] playerPositions;

    [Header("Fire bonus components")]
    [SerializeField]
    protected GameObject fireOnBallParticles;
    [SerializeField]
    protected GameObject fireBonusUI;
    [SerializeField]
    private AudioClip fireBonusEndClip;
    protected bool isFireBonusActive;
    protected float fireBonusValue;

    [SerializeField]
    protected Transform throwEndPosition;
    protected Transform throwEndPositionChild;

    protected int currentPlayerPosition = 0;
    protected float throwingTimer;
    protected bool isThrowingBall;

    protected bool doBackboardShot;
    protected int pointsEarned;
    protected bool makePoints;

    internal bool ignoreInputs;

    private float fireValueDecreasingDivisor;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    protected virtual void Start()
    {
        currentPlayerPosition = Random.Range(0, playerPositions.Length);
        transform.position = playerPositions[currentPlayerPosition].position;
        transform.eulerAngles = playerPositions[currentPlayerPosition].eulerAngles;
        fireValueDecreasingDivisor = gameManager.fireBonusDecreasingSpeedDivisor;
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
        pointsEarned = pointsToGive;
        Debug.Log("POINTS " + pointsEarned);
        doBackboardShot = isBackboardShot;
    }

    private IEnumerator MovingPlayerToNextPosition(int direction)
    {
        currentPlayerPosition += direction;
        if (currentPlayerPosition >= playerPositions.Length)
        {
            currentPlayerPosition = 0;
        }
        else if(currentPlayerPosition < 0)
        {
            currentPlayerPosition = playerPositions.Length - 1;
        }

        float movingTimer = 0;
        float movingPercent = 0;
        Vector3 startPosition = transform.position;
        Vector3 startRotation = transform.eulerAngles;
        while (movingPercent < 1)
        {
            movingTimer += Time.deltaTime;
            movingPercent = movingTimer / 0.2f;
            transform.position = Vector3.Lerp(startPosition, playerPositions[currentPlayerPosition].position, movingPercent);
            transform.eulerAngles = Vector3.Lerp(startRotation, playerPositions[currentPlayerPosition].eulerAngles, movingPercent);
            yield return null;
        }

        transform.LookAt(throwEndPosition, Vector3.up);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        ignoreInputs = false;
    }

    protected void CheckThrowingResult(float throwPower, bool hasFireBonus)
    {
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
        CheckFireBonus(points);
    }

    protected virtual void ResetShot(int directionOfMovement)
    {
        ball.transform.position = dribblePosition.position + Vector3.up * 0.7f;
        pointsEarned = 0;
        throwingTimer = 0;
        ball.hasMakeSound = false;
        doBackboardShot = false;

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
            fireBonusValue += points == 2 ? 2.5f : 4f;

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
}
