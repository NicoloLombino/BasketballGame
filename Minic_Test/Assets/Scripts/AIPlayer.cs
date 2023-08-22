using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : PlayerBase
{
    [SerializeField]
    private SaveData saveDataScriptableObject;

    [Header("AI components")]
    [SerializeField]
    private float timerToThrow;
    [SerializeField, Range(0,10)]
    private int accuracy;

    [Header("Fire bonus components")]
    [SerializeField]
    private GameObject fireOnBallParticles;
    [SerializeField]
    private GameObject fireBonusUI;
    private bool isFireBonusActive;
    private float fireBonusValue;

    private float timer;

    protected override void Start()
    {
        base.Start();
        timerToThrow += (10 / saveDataScriptableObject.AILevel) / 3;
    }

    protected override void Update()
    {
        base.Update();

        if(gameManager.isInGame && !ignoreInputs)
        {
            timer += Time.deltaTime;
            if(timer >= timerToThrow)
            {
                CheckBestThrow();
                timer = 0;
            }
        }
    }

    private void AIThrowingBall(float sliderValue)
    {
        int rndValueOnThrow = Random.Range(accuracy, 11);
        float rndError = ((10f - rndValueOnThrow) / 10f) * (1 - saveDataScriptableObject.AILevel / 10f);
        int rndAim = Random.Range(0, 11);
        Debug.Log("rndAim = " + rndAim);
        float aimSign = 1;
        float totalAim = ((float)(accuracy + saveDataScriptableObject.AILevel) / 2);
        if (rndAim <= totalAim)
        {
            aimSign = 1;
        }
        else
        {
            aimSign = -1;
        }
        float rndErrorOnThrow = rndError * aimSign; 
        float finalThrowValueOnSlider = sliderValue + rndErrorOnThrow;
        Debug.Log("with RND= " + rndValueOnThrow + " AND ERROR= " + rndErrorOnThrow + " FINAL= " + finalThrowValueOnSlider);
        StartCoroutine(ThrowingBall(finalThrowValueOnSlider, isFireBonusActive)); 
    }

    private void CheckBestThrow()
    {
        ignoreInputs = true;
        float throwValue = 0;
        if(gameManager.isBackBoardBonusActive)
        {
            // aim to backboard
            throwValue = (gameManager.valueToBackboardAndPointsMin / 10 );
        }
        else
        {
            // aim to basket to get 3 points
            throwValue = (gameManager.valueTo3PointsMin + gameManager.valueTo3PointsMax) / 20;
        }

        AIThrowingBall(throwValue);
    }

    private IEnumerator ThrowingBall(float throwPower, bool hasFireBonus)
    {
        ignoreInputs = true;
        float preparingTimer = 0;
        float preparingPercent = 0;
        Vector3 ballPosition = ball.transform.position;

        while (preparingPercent < 1)
        {
            preparingTimer += Time.deltaTime;
            preparingPercent = preparingTimer / 0.7f;
            ball.transform.position = Vector3.Lerp(ballPosition, throwStartPosition.position, preparingPercent);
            yield return null;
        }

        CheckThrowingResult(throwPower, hasFireBonus);

        // the sound of throwing ball
        audioSource.Play();

        float throwingPercent = 0;
        while (throwingPercent < 1)
        {
            throwingTimer += Time.deltaTime;
            throwingPercent = throwingTimer / throwingDuration;
            ball.transform.position = Vector3.Lerp(throwStartPosition.position, throwEndPosition.position, throwingPercent)
                + Vector3.up * 5 * Mathf.Sin(throwingPercent * 3.14f);
            yield return null;
        }

        throwingTimer = 0;
        float throwingPercent2 = 0;
        while (throwingPercent2 < 1)
        {
            throwingTimer += Time.deltaTime;
            throwingPercent2 = throwingTimer / (throwingDuration / 2);
            ball.transform.position = Vector3.Lerp(throwEndPosition.position, throwEndPositionChild.position, throwingPercent2)
                + transform.up * 0.75f * Mathf.Sin(throwingPercent2 * 3.14f);
            yield return null;
        }

        if (makePoints)
        {
            gameManager.AddAIPoints(pointsEarned, doBackboardShot, hasFireBonus);
        }
        gameManager.DoRandomBackboardBonus();

        yield return new WaitForSecondsRealtime(0.1f);
        ResetShot();
    }

    private void ResetShot()
    {
        ball.transform.position = dribblePosition.position + Vector3.up * 0.7f;
        isThrowingBall = false;
        throwingTimer = 0;
        ball.hasMakeSound = false;
        if (makePoints)
        {
            MovePlayerToNextPosition(-1);
            makePoints = false;
        }
        else
        {
            ignoreInputs = false;
        }
    }

    private void CheckThrowingResult(float throwPower, bool hasFireBonus)
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
            Debug.Log("AI --> GO OUT");
            DisableFireBonus();
            ball.hasMakeSound = true;
            ballThrowingAnimationIndex = 0;
        }
        else if (throwPower >= basketboard && throwPower < twoPointsLess)
        {
            // basket board, no points
            Debug.Log("AI --> HIT BASKET");
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(0, 0, hasFireBonus);
            DisableFireBonus();
            ballThrowingAnimationIndex = 1;
        }
        else if (throwPower >= twoPointsLess && throwPower < perfectShotValueMin)
        {
            // enter in basket --> 2 points
            Debug.Log("AI --> ENTER 2 POINTS LESS");
            points = 2;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
            makePoints = true;
            ballThrowingAnimationIndex = 2;
        }
        else if (throwPower >= perfectShotValueMin && throwPower <= perfectShotValueMax)
        {
            // enter in basket --> 3 points
            Debug.Log("AI --> ENTER 3 POINTS");
            points = 3;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 3, hasFireBonus);
            makePoints = true;
            ballThrowingAnimationIndex = 3;
        }
        else if (throwPower > perfectShotValueMax && throwPower <= twoPointsMore)
        {
            // enter in basket --> 2 points
            Debug.Log("AI --> ENTER 2 POINTS MORE");
            points = 2;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
            makePoints = true;
            ballThrowingAnimationIndex = 2;
        }
        else if (throwPower > twoPointsMore && throwPower < backboardShotValue)
        {
            // hit backboard and go out --> NO points
            Debug.Log("AI --> HIT BACKBOARD AND GO OUT LESS");
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
            DisableFireBonus();
            ballThrowingAnimationIndex = 4;
        }
        else if (throwPower >= backboardShotValue && throwPower <= backboardMore)
        {
            // hit backboard and enter in basket --> 2 points
            Debug.Log("AI --> HIT BACKBOARD AND ENTER 2 POINTS");
            points = 2;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(3, 2, hasFireBonus);
            isBackboardShot = true;
            makePoints = true;
            ballThrowingAnimationIndex = 5;
        }
        else
        {
            // hit backboard and go out
            Debug.Log("AI --> HIT BACKBOARD AND GO OUT MORE");
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
            DisableFireBonus();
            ballThrowingAnimationIndex = 6;
        }

        SetBallAnimationPositionsOnThrowing(
            ball.ballThrowingPositions[currentPlayerPosition].ballPositions[ballThrowingAnimationIndex],
            ball.ballThrowingPositions[currentPlayerPosition].ballPositions[ballThrowingAnimationIndex].GetChild(0));

        //gameManager.AddAIPoints(points, isBackboardShot, hasFireBonus);
        SetThrowValues(points, isBackboardShot);
        CheckFireBonusOnAI(points);
    }

    private void CheckFireBonusOnAI(int points)
    {
        if (!isFireBonusActive)
        {
            fireBonusValue += points;

            if (fireBonusValue >= 9)
            {
                fireBonusValue = 9;
                StartCoroutine(ActiveFireBonus());
            }
        }
    }

    private IEnumerator ActiveFireBonus()
    {
        isFireBonusActive = true;
        fireOnBallParticles.SetActive(true);
        fireBonusUI.SetActive(true);

        while (fireBonusValue > 0)
        {
            fireBonusValue -= Time.deltaTime;
            yield return null;
        }

        while(ignoreInputs)
        {
            yield return null;
        }

        DisableFireBonus();
    }

    public void DisableFireBonus()
    {
        fireBonusValue = 0;
        isFireBonusActive = false;
        fireOnBallParticles.SetActive(false);
        fireBonusUI.SetActive(false);
    }
}
