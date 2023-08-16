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
    private bool isThrowingBall;

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
                    throwingPowerSlider.value = (touch.position.y - inputInitPosition.y)/ 1000;
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

        float throwingPercent = 0;
        while (throwingPercent < 1)
        {
            throwingTimer += Time.deltaTime;
            throwingPercent = throwingTimer / throwingDuration;
            ball.position = Vector3.Lerp(throwStartPosition.position, throwEndPositionWithRandomError, throwingPercent)
                + Vector3.up * 5 * Mathf.Sin(throwingPercent * 3.14f);
            yield return null;
        }
    }
}
