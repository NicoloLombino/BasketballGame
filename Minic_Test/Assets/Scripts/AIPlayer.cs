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
    private bool isFireBonusActive;
    private float fireBonusValue;

    private float timer;

    void Start()
    {
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
        float rndAim = Random.Range(0f, 11f);
        Debug.Log("rndAim = " + rndAim);
        float aimSign = 0;
        float totalAim = ((float)(accuracy + saveDataScriptableObject.AILevel) / 2);
        if (rndAim <= totalAim)
        {
            Debug.Log("MINORE");
            aimSign = 1;
        }
        else
        {
            Debug.Log("MAGGIORE");
            aimSign = -1;
        }
        float rndErrorOnThrow = rndError * aimSign;  /*Mathf.Sign(Random.Range(-1, (saveDataScriptableObject.AILevel + 1)));*/ 
        float finalThrowValueOnSlider = sliderValue + rndErrorOnThrow;
        Debug.Log("rndError " + rndError);
        Debug.Log("aimSign " + aimSign);
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
            //throwValue = gameManager.GetSliderValueBackboardShot();
            throwValue = (gameManager.valueToBackboardAndPointsMin / 10 )/*+ gameManager.valueToBackboardAndPointsMax) / 20*/;

        }
        else
        {
            // aim to basket to get 3 points
            //throwValue = gameManager.GetSliderValuePerfectShot();
            throwValue = (gameManager.valueTo3PointsMin + gameManager.valueTo3PointsMax) / 20;
        }

        AIThrowingBall(throwValue);
    }
    private IEnumerator ThrowingBall(float throwPower, bool hasFireBonus)
    {
        ignoreInputs = true;
        float preparingTimer = 0;
        float preparingPercent = 0;
        //float preparingDuration = 0;
        Vector3 ballPosition = ball.position;
        while (preparingPercent < 1)
        {
            preparingTimer += Time.deltaTime;
            preparingPercent = preparingTimer / 0.7f;
            ball.position = Vector3.Lerp(ballPosition, throwStartPosition.position, preparingPercent);
            yield return null;
        }

        CheckThrowingResult(throwPower, hasFireBonus);

        float throwingPercent = 0;
        while (throwingPercent < 1)
        {
            throwingTimer += Time.deltaTime;
            throwingPercent = throwingTimer / throwingDuration;
            ball.position = Vector3.Lerp(throwStartPosition.position, throwEndPosition.position, throwingPercent)
                + Vector3.up * 5 * Mathf.Sin(throwingPercent * 3.14f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1);
        ResetShot();
    }

    private void ResetShot()
    {
        ball.position = dribblePosition.position;
        isThrowingBall = false;
        throwingTimer = 0;
        if (makePoints)
        {
            StartCoroutine(MovePlayerToNextPosition());
            makePoints = false;
        }
        else
        {
            ignoreInputs = false;
        }
    }

    private void CheckThrowingResult(float throwPower, bool hasFireBonus)
    {
        //float perfectShotValue = gameManager.GetSliderValuePerfectShot(); // 3 points
        //float backboardShotValue = gameManager.GetSliderValueBackboardShot();
        //float basketboard = perfectShotValue - 0.2f; // hit the basket
        //float twoPointsLess = perfectShotValue - 0.1f; // 2 points
        //float twoPointsMore = perfectShotValue + 0.1f; // 2 points
        //float backboardLess = twoPointsMore + 0.1f; // hit the backboard left or right
        //float backboardMore = backboardShotValue + 0.1f; // hit the backboard up 

        float basketboard = gameManager.valueToHitBasketAndGoOut / 10;
        float twoPointsLess = gameManager.valueTo2PointsMin / 10;
        float perfectShotValueMin = gameManager.valueTo3PointsMin / 10; // 3 points
        float perfectShotValueMax = gameManager.valueTo3PointsMax / 10; // 3 points
        //float twoPointsMore = gameManager.valueTo2PointsMax / 10;
        float twoPointsMore = gameManager.valueTo2PointsMax / 10; // hit the backboard left or right
        float backboardShotValue = gameManager.valueToBackboardAndPointsMin / 10;
        float backboardMore = gameManager.valueToBackboardAndPointsMax / 10;

        int points = 0;
        bool isBackboardShot = false;

        if (throwPower < basketboard)
        {
            // NO, no points
            Debug.Log("AI --> GO OUT");
            DisableFireBonus();
        }
        else if (throwPower >= basketboard && throwPower < twoPointsLess)
        {
            // basket board, no points
            Debug.Log("AI --> HIT BASKET");
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(0, 0, hasFireBonus);
            DisableFireBonus();
        }
        else if (throwPower >= twoPointsLess && throwPower < perfectShotValueMin)
        {
            // enter in basket --> 2 points
            Debug.Log("AI --> ENTER 2 POINTS LESS");
            points = 2;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
            makePoints = true;
        }
        else if (throwPower >= perfectShotValueMin && throwPower <= perfectShotValueMax)
        {
            // enter in basket --> 3 points
            Debug.Log("AI --> ENTER 3 POINTS");
            points = 3;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 3, hasFireBonus);
            makePoints = true;
        }
        else if (throwPower > perfectShotValueMax && throwPower <= twoPointsMore)
        {
            // enter in basket --> 2 points
            Debug.Log("AI --> ENTER 2 POINTS MORE");
            points = 2;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
            makePoints = true;
        }
        else if (throwPower > twoPointsMore && throwPower < backboardShotValue)
        {
            // hit backboard and go out --> NO points
            Debug.Log("AI --> HIT BACKBOARD AND GO OUT LESS");
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
            DisableFireBonus();
        }
        else if (throwPower >= backboardShotValue && throwPower < backboardMore)
        {
            // hit backboard and enter in basket --> 2 points
            Debug.Log("AI --> HIT BACKBOARD AND ENTER 2 POINTS");
            points = 2;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(3, 2, hasFireBonus);
            isBackboardShot = true;
            makePoints = true;
        }
        else
        {
            // hit backboard and go out
            Debug.Log("AI --> HIT BACKBOARD AND GO OUT MORE");
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
            DisableFireBonus();
        }

        gameManager.AddAIPoints(points, isBackboardShot, hasFireBonus);
        CheckFireBonusOnAI(points);
        gameManager.DoRandomBackboardBonus();
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
    }

    private IEnumerator MovePlayerToNextPosition()
    {
        currentPlayerPosition++;
        if (currentPlayerPosition >= playerPositions.Length)
        {
            currentPlayerPosition = 0;
        }

        float movingTimer = 0;
        float movingPercent = 0;
        Vector3 startPosition = transform.position;
        Vector3 startRotation = transform.eulerAngles;
        while (movingPercent < 1)
        {
            movingTimer += Time.deltaTime;
            movingPercent = movingTimer / 0.5f;
            transform.position = Vector3.Lerp(startPosition, playerPositions[currentPlayerPosition].position, movingPercent);
            transform.eulerAngles = Vector3.Lerp(startRotation, playerPositions[currentPlayerPosition].eulerAngles, movingPercent);
            yield return null;
        }
        ignoreInputs = false;
    }
}
