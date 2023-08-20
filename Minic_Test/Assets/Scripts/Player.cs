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

    [Header("mobile inputs components")]
    Touch touch;
    Vector3 inputInitPosition;
    Vector2 maxPosY;

    private float swipingTimer;

    [Header("PC inputs components")]
    Vector3 mouseStartPosition;
    Vector3 mouseEndPosition;
    private bool mouseMovementStarted;

    //
    float pixelInitPerc;
    float pixelMaxPerc;
    float pixelMaxY;

    void Start()
    {

    }

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
        myCamera.GetComponent<Animator>().SetTrigger("Throw");

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
        touch.phase = TouchPhase.Ended;
        swipingTimer = 0;
        inputInitPosition = Vector3.zero;
        throwingTimer = 0;
        throwingPowerSlider.value = 0;
        if(makePoints)
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

        if (throwPower < basketboard)
        {
            // NO, no points
            Debug.Log("GO OUT");
            gameManager.DisableFireBonus();
        }
        else if (throwPower >= basketboard && throwPower < twoPointsLess)
        {
            // basket board, no points
            Debug.Log("HIT BASKET");
            gameManager.DisableFireBonus();
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(0, 0, hasFireBonus);
        }
        else if (throwPower >= twoPointsLess && throwPower < perfectShotValueMin)
        {
            // enter in basket --> 2 points
            Debug.Log("ENTER 2 POINTS LESS");
            gameManager.AddPlayerPoints(2, false, hasFireBonus);
            makePoints = true;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
        }
        else if (throwPower >= perfectShotValueMin && throwPower <= perfectShotValueMax)
        {
            // enter in basket --> 3 points
            Debug.Log("ENTER 3 POINTS");
            gameManager.AddPlayerPoints(3, false, hasFireBonus);
            makePoints = true;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 3, hasFireBonus);
        }
        else if (throwPower > perfectShotValueMax && throwPower <= twoPointsMore)
        {
            // enter in basket --> 2 points
            Debug.Log("ENTER 2 POINTS MORE");
            gameManager.AddPlayerPoints(2, false, hasFireBonus);
            makePoints = true;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(1, 2, hasFireBonus);
        }
        else if (throwPower > twoPointsMore && throwPower < backboardShotValue)
        {
            // hit backboard and go out --> NO points
            Debug.Log("HIT BACKBOARD AND GO OUT LESS");
            gameManager.DisableFireBonus();
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
        }
        else if (throwPower >= backboardShotValue && throwPower <= backboardMore)
        {
            // hit backboard and enter in basket --> 2 points
            Debug.Log("HIT BACKBOARD AND ENTER 2 POINTS");
            gameManager.AddPlayerPoints(2, true, hasFireBonus);
            makePoints = true;
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(3, 2, hasFireBonus);
        }
        else
        {
            // hit backboard and go out
            Debug.Log("HIT BACKBOARD AND GO OUT MORE");
            gameManager.DisableFireBonus();
            ball.GetComponent<Ball>().SetAudioClipToPlayAndParticlesToUse(2, 0, hasFireBonus);
        }

        gameManager.DoRandomBackboardBonus();
    }

    private IEnumerator MovePlayerToNextPosition()
    {
        currentPlayerPosition++;
        if(currentPlayerPosition >= playerPositions.Length)
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
        //transform.position = playerPositions[currentPlayerPosition].position;
        //transform.eulerAngles = playerPositions[currentPlayerPosition].eulerAngles;
        ignoreInputs = false;
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
                //Debug.Log(inputInitPosition.y);
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
