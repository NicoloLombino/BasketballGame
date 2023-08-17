using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Player stats")]
    [Range(0,2)]
    public int shootingPosition;
    [Header("Ball components")]
    [SerializeField]
    private Transform ball;
    [SerializeField]
    private Transform dribblePosition;
    [SerializeField]
    private float dribbleHeight;
    [SerializeField]
    private float throwingDuration;

    [Header("Throwing components")]
    [SerializeField]
    private Transform throwStartPosition; 
    [SerializeField]
    private Transform throwEndPosition;
    [SerializeField]
    private float maxSwipingTimer;

    [Header("UI components")]
    [SerializeField]
    private Slider throwingPowerSlider;

    [Header("mobile inputs components")]
    Touch touch;
    Vector3 inputInitPosition;
    Vector2 maxPosY;

    [Header("reference")]
    [SerializeField]
    private InGameUI inGameUI;


    private float throwingTimer;
    private Vector3 throwEndPositionWithRandomError;
    protected bool isThrowingBall;

    private bool ignoreInputs;

    private float swipingTimer;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isThrowingBall && !ignoreInputs)
        {
            ball.position = dribblePosition.position + Vector3.up * 0.5f + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * dribbleHeight));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(ThrowingBall(throwingPowerSlider.value));
        } 

        // mobile input

        if (Input.touchCount > 0 && !ignoreInputs)
        {
            touch = Input.GetTouch(0);
            if (inputInitPosition == Vector3.zero)
            {           
                inputInitPosition = touch.position;
                Debug.Log(inputInitPosition.y);
                maxPosY = touch.position;
            }

            swipingTimer += Time.deltaTime;
            if (touch.phase == TouchPhase.Moved)
            {
                if (touch.position.y > maxPosY.y)
                {
                    throwingPowerSlider.value = (touch.position.y - inputInitPosition.y)/ 700;
                    maxPosY = touch.position;
                }
            }
            if (touch.phase == TouchPhase.Ended || swipingTimer >= maxSwipingTimer)
            {
                Debug.Log("init "+ inputInitPosition.y);
                Debug.Log("end " + touch.position.y);
                StartCoroutine(ThrowingBall(throwingPowerSlider.value));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="throwPower"></param> is the value of the slider when player throw the ball
    /// <returns></returns>
    private IEnumerator ThrowingBall(float throwPower)
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

        CheckThrowingResult(throwPower);

        float throwingPercent = 0;
        while (throwingPercent < 1)
        {
            throwingTimer += Time.deltaTime;
            throwingPercent = throwingTimer / throwingDuration;
            ball.position = Vector3.Lerp(throwStartPosition.position, throwEndPositionWithRandomError, throwingPercent)
                + Vector3.up * 5 * Mathf.Sin(throwingPercent * 3.14f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(2);
        ResetShot();
    }

    private void ResetShot()
    {
        ball.position = dribblePosition.position;
        isThrowingBall = false;
        ignoreInputs = false;
        touch.phase = TouchPhase.Ended;
        swipingTimer = 0;
        inputInitPosition = Vector3.zero;
        throwingTimer = 0;
    }

    private void CheckThrowingResult(float throwPower)
    {
        float perfectShotValue = inGameUI.GetSliderValuePerfectShot(); // 3 points
        float backboardShotValue = inGameUI.GetSliderValueBackboardShot();
        float basketboard = perfectShotValue - 0.2f; // hit the basket
        float twoPointsLess = perfectShotValue - 0.1f; // 2 points
        float twoPointsMore = perfectShotValue + 0.1f; // 2 points
        float backboardLess = twoPointsMore + 0.1f; // hit the backboard left or right
        float backboardMore = backboardShotValue + 0.1f; // hit the backboard up

        if (throwPower < basketboard)
        {
            // NO, no points
            Debug.Log("GO OUT");
        }
        else if (throwPower >= basketboard && throwPower < twoPointsLess)
        {
            // basket board, no points
            Debug.Log("HIT BASKET");
        }
        else if (throwPower >= twoPointsLess && throwPower < perfectShotValue)
        {
            // enter in basket --> 2 points
            Debug.Log("ENTER 2 POINTS LESS");
            inGameUI.AddPlayerPoints(2, false);
        }
        else if (throwPower >= perfectShotValue && throwPower < twoPointsMore)
        {
            // enter in basket --> 3 points
            Debug.Log("ENTER 3 POINTS");
            inGameUI.AddPlayerPoints(3, false);
        }
        else if (throwPower >= twoPointsMore && throwPower < backboardLess)
        {
            // enter in basket --> 2 points
            Debug.Log("ENTER 2 POINTS MORE");
            inGameUI.AddPlayerPoints(2, false);
        }
        else if (throwPower >= backboardLess && throwPower < backboardShotValue)
        {
            // hit backboard and go out --> NO points
            Debug.Log("HIT BACKBOARD AND GO OUT LESS");
        }
        else if (throwPower >= backboardShotValue && throwPower < backboardMore)
        {
            // hit backboard and enter in basket --> 2 points
            Debug.Log("HIT BACKBOARD AND ENTER 2 POINTS");
            inGameUI.AddPlayerPoints(2, true);
        }
        else
        {
            // hit backboard and go out
            Debug.Log("HIT BACKBOARD AND GO OUT MORE");
        }

        inGameUI.DoRandomBackboardBonus();
    }

    private void MovePlayerToNextPosition()
    {

    }
}
