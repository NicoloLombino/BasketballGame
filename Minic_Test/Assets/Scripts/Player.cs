using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : PlayerBase
{
    [SerializeField]
    private GameObject myCamera;

    [Header("Throwing components")]
    [SerializeField]
    private float maxSwipingTimer;

    [Header("UI components")]
    [SerializeField]
    private Slider throwingPowerSlider;
    [SerializeField]
    private RectTransform sliderValueCursor;

    [Header("Mobile inputs components")]
    Touch touch;
    Vector3 inputInitPosition;
    Vector2 maxPosY;

    private float swipingTimer;

    [Header("PC inputs components")]
    Vector3 mouseStartPosition;
    private bool mouseMovementStarted;

    private float pixelInitPerc;
    private float pixelMaxPerc;
    private float pixelMaxY;

    private bool doBackboardShot;
    private int pointsEarned;

    protected override void Update()
    {
        base.Update();

        if(gameManager.isInGame)
        {
            if(gameManager.isAndroidSetup)
            {
                ReadAndroidInput();
            }
            else
            {
                ReadPCInput();
            }              
        }

        sliderValueCursor.localPosition = new Vector2(sliderValueCursor.localPosition.x, throwingPowerSlider.value * 800 - sliderValueCursor.sizeDelta.y/2);
    }

    // OLD SYSTEM

    //private void ReadAndroidInput()
    //{
    //    if (Input.touchCount > 0 && !ignoreInputs)
    //    {
    //        touch = Input.GetTouch(0);
    //        if (inputInitPosition == Vector3.zero)
    //        {
    //            inputInitPosition = touch.position;
    //            //Debug.Log(inputInitPosition.y);
    //            maxPosY = touch.position;
    //        }

    //        swipingTimer += Time.deltaTime;
    //        if (touch.phase == TouchPhase.Moved)
    //        {
    //            Debug.Log(touch.position.y / Screen.height);
    //            if (touch.position.y > maxPosY.y)
    //            {
    //                throwingPowerSlider.value = (touch.position.y - inputInitPosition.y) / (Screen.height / 2);
    //                maxPosY = touch.position;
    //            }
    //        }
    //        if (touch.phase == TouchPhase.Ended || swipingTimer >= maxSwipingTimer)
    //        {
    //            //Debug.Log("init "+ inputInitPosition.y);
    //            //Debug.Log("end " + touch.position.y);
    //            StartCoroutine(ThrowingBall(throwingPowerSlider.value, gameManager.isFireBonusActive));
    //        }
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="throwPower"></param> is the value of the slider when player throw the ball
    /// <returns></returns>
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

        myCamera.GetComponent<Animator>().SetTrigger("Throw");
        CheckThrowingResult(throwPower, hasFireBonus);

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

        if(makePoints)
        {
            gameManager.AddPlayerPoints(pointsEarned, doBackboardShot, hasFireBonus);
        }

        yield return new WaitForSecondsRealtime(0.1f);
        ResetShot();
    }

    private void ResetShot()
    {
        ball.transform.position = dribblePosition.position;
        isThrowingBall = false;
        touch.phase = TouchPhase.Ended;
        swipingTimer = 0;
        inputInitPosition = Vector3.zero;
        throwingTimer = 0;
        throwingPowerSlider.value = 0;
        pointsEarned = 0;
        doBackboardShot = false;
        ball.hasMakeSound = false;
        if (makePoints)
        {
            MovePlayerToNextPosition();
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
        float backboardMore = gameManager.valueToBackboardAndPointsMax / 10; // 2 points backboard

        int ballThrowingAnimationIndex;

        if (throwPower < basketboard)
        {
            // NO, no points
            Debug.Log("GO OUT");
            gameManager.DisableFireBonus();
            ballThrowingAnimationIndex = 0;

        }
        else if (throwPower >= basketboard && throwPower < twoPointsLess)
        {
            // basket board, no points
            Debug.Log("HIT BASKET");
            gameManager.DisableFireBonus();
            ball.SetAudioClipToPlayAndParticlesToUse(0, 0, hasFireBonus);
            ballThrowingAnimationIndex = 1;

        }
        else if (throwPower >= twoPointsLess && throwPower < perfectShotValueMin)
        {
            // enter in basket --> 2 points
            Debug.Log("ENTER 2 POINTS LESS");
            //gameManager.AddPlayerPoints(2, false, hasFireBonus);
            SetThrowValues(2, false);
            makePoints = true;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
            ballThrowingAnimationIndex = 2;

        }
        else if (throwPower >= perfectShotValueMin && throwPower <= perfectShotValueMax)
        {
            // enter in basket --> 3 points
            Debug.Log("ENTER 3 POINTS");
            //gameManager.AddPlayerPoints(3, false, hasFireBonus);
            SetThrowValues(3, false);
            makePoints = true;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 3, hasFireBonus);
            ballThrowingAnimationIndex = 3;

        }
        else if (throwPower > perfectShotValueMax && throwPower <= twoPointsMore)
        {
            // enter in basket --> 2 points
            Debug.Log("ENTER 2 POINTS MORE");
            //gameManager.AddPlayerPoints(2, false, hasFireBonus);
            SetThrowValues(2, false);
            makePoints = true;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
            ballThrowingAnimationIndex = 2;

        }
        else if (throwPower > twoPointsMore && throwPower < backboardShotValue)
        {
            // hit backboard and go out --> NO points
            Debug.Log("HIT BACKBOARD AND GO OUT LESS");
            gameManager.DisableFireBonus();
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
            ballThrowingAnimationIndex = 4;

        }
        else if (throwPower >= backboardShotValue && throwPower <= backboardMore)
        {
            // hit backboard and enter in basket --> 2 points
            Debug.Log("HIT BACKBOARD AND ENTER 2 POINTS");
            //gameManager.AddPlayerPoints(2, true, hasFireBonus);
            SetThrowValues(2, true);
            makePoints = true;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(3, 2, hasFireBonus);
            ballThrowingAnimationIndex = 5;

        }
        else
        {
            // hit backboard and go out
            Debug.Log("HIT BACKBOARD AND GO OUT MORE");
            gameManager.DisableFireBonus();
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
            ballThrowingAnimationIndex = 6;

        }

        SetBallAnimationPositionsOnThrowing(
            ball.ballThrowingPositions[currentPlayerPosition].ballPositions[ballThrowingAnimationIndex],
            ball.ballThrowingPositions[currentPlayerPosition].ballPositions[ballThrowingAnimationIndex].GetChild(0));

        gameManager.DoRandomBackboardBonus();
    }

    private void SetThrowValues(int pointsToGive, bool isBackboardShot)
    {
        pointsEarned = pointsToGive;
        doBackboardShot = isBackboardShot;
    }

    public void HandleEndGame()
    {
        ignoreInputs = true;
    }

    private void ReadAndroidInput()
    {
        if (Input.touchCount > 0 && !ignoreInputs)
        {
            touch = Input.GetTouch(0);
            if (inputInitPosition == Vector3.zero)
            {
                inputInitPosition = touch.position;
                maxPosY = touch.position;
                pixelInitPerc = inputInitPosition.y * 100 / Screen.height;
                pixelMaxPerc = pixelInitPerc + 50;
                pixelMaxY = pixelMaxPerc * Screen.height / 100;
            }

            swipingTimer += Time.deltaTime;

            if (touch.phase == TouchPhase.Moved && swipingTimer < maxSwipingTimer)
            {
                if (touch.position.y > maxPosY.y)
                {
                    throwingPowerSlider.value = Mathf.InverseLerp(inputInitPosition.y, pixelMaxY, touch.position.y);
                    maxPosY = touch.position;
                }
            }
            if (touch.phase == TouchPhase.Ended || swipingTimer >= maxSwipingTimer)
            {
                //Debug.Log("init "+ inputInitPosition.y);
                //Debug.Log("end " + touch.position.y);
                StartCoroutine(ThrowingBall(throwingPowerSlider.value, gameManager.isFireBonusActive));
            }
        }
    }

    private void ReadPCInput()
    {
        if (!ignoreInputs)
        {
            if (Input.GetMouseButtonDown(0) && !mouseMovementStarted)
            {
                mouseStartPosition = Input.mousePosition;
                maxPosY = mouseStartPosition;
                mouseMovementStarted = true;
                pixelInitPerc = mouseStartPosition.y * 100 / Screen.height;
                pixelMaxPerc = pixelInitPerc + 50;
                pixelMaxY = pixelMaxPerc * Screen.height / 100;
            }
            else if (Input.GetMouseButton(0) && mouseMovementStarted && swipingTimer < maxSwipingTimer)
            {
                swipingTimer += Time.deltaTime;
                if (Input.mousePosition.y > maxPosY.y)
                {
                    throwingPowerSlider.value = Mathf.InverseLerp(inputInitPosition.y, pixelMaxY, Input.mousePosition.y);
                    maxPosY = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButtonUp(0) || swipingTimer >= maxSwipingTimer)
            {
                mouseMovementStarted = false;
                StartCoroutine(ThrowingBall(throwingPowerSlider.value, gameManager.isFireBonusActive));
            }
        }
    }

}
