using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : PlayerBase
{
    private const float PERCENTAGE_MAX_OF_SCREEN_HEIGHT_FOR_SLIDER = 50;
    private const float PERCENTAGE_MIN_OF_POWER_SLIDER_TO_THWOW_BALL = 0.1f;

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
    private Touch touch;
    private Vector3 inputInitPosition;
    private Vector2 maxPosY;

    [Header("PC inputs components")]
    private Vector3 mouseStartPosition;
    private bool mouseMovementStarted;

    [Header("Player Fire Bonus Components")]
    [SerializeField]
    private Slider fireBonusSlider;
    [SerializeField]
    private Image fireImageBackground;
    [SerializeField]
    private Color fireImageBackgroundColorWhenActive;

    [Header("Player LuckyBall Components")]
    [SerializeField]
    private GameObject luckyBallActiveUI;

    private float swipingTimer;

    private float pixelInitPerc;
    private float pixelMaxPerc;
    private float pixelMaxY;

    protected override void Start()
    {
        base.Start();
        fireBonusSlider.maxValue = gameManager.maxFireBonusTime;
        SetMaterialsToPlayerAndBall();
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

        SetPositionOfPowerSliderIndicator();

        fireBonusSlider.value = fireBonusValue;
    }

    /// <summary>
    /// throw the ball and set his movement
    /// </summary>
    /// <param name="throwPower">is the value of the slider when player throw the ball</param>
    /// <param name="hasFireBonus">if the fire bonus is active</param>
    /// <returns></returns>
    private IEnumerator ThrowingBall(float throwPower, bool hasFireBonus)
    {
        ignoreInputs = true;

        // prepare to shoot the ball
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

        // the sound of throwing ball
        audioSource.Play();
        // start the camera animation
        myCamera.GetComponent<Animator>().SetTrigger("Throw");
        // check the shot type and his result
        CheckThrowingResult(throwPower, hasFireBonus);

        // throw the ball in the the specified position
        float throwingPercent = 0;
        while (throwingPercent < 1)
        {
            throwingTimer += Time.deltaTime;
            throwingPercent = throwingTimer / throwingDuration;
            ball.transform.position = Vector3.Lerp(throwStartPosition.position, throwEndPosition.position, throwingPercent)
                + Vector3.up * 5 * Mathf.Sin(throwingPercent * Mathf.PI);
            yield return null;
        }

        // handles the ball bounce
        throwingTimer = 0;
        float throwingPercent2 = 0;
        while (throwingPercent2 < 1)
        {
            throwingTimer += Time.deltaTime;
            throwingPercent2 = throwingTimer / (throwingDuration / 2);
            ball.transform.position = Vector3.Lerp(throwEndPosition.position, throwEndPositionChild.position, throwingPercent2)
                + transform.up * 0.75f * Mathf.Sin(throwingPercent2 * Mathf.PI);
            yield return null;
        }

        // check the score and give points to player
        if(makePoints)
        {
            gameManager.AddPlayerPoints(pointsEarnedBase, doBackboardShot, hasFireBonus, isLuckyBallActive);
        }

        ResetShot(1);
    }

    protected override void ResetShot(int directionOfMovement)
    {
        touch.phase = TouchPhase.Ended;
        swipingTimer = 0;
        inputInitPosition = Vector3.zero;
        throwingPowerSlider.value = 0;

        base.ResetShot(directionOfMovement);
    }

    protected override void ActiveLuckyBall()
    {
        base.ActiveLuckyBall();
        Instantiate(luckyBallActiveUI, Vector3.zero, Quaternion.identity);
    }

    protected override void CheckFireBonus(int points)
    {
        base.CheckFireBonus(points);

        if(isFireBonusActive)
        {
            fireImageBackground.color = fireImageBackgroundColorWhenActive;
        }
    }
    protected override void DisableFireBonus()
    {
        base.DisableFireBonus();
        fireImageBackground.color = Color.white;
    }

    private void SetPositionOfPowerSliderIndicator()
    {
        sliderValueCursor.localPosition = 
            new Vector2(sliderValueCursor.localPosition.x, throwingPowerSlider.value * 
            gameManager.throwingBallSliderRect.rect.height - sliderValueCursor.sizeDelta.y / 2);
    }

    private void SetMaterialsToPlayerAndBall()
    {
        gameObject.GetComponent<MeshRenderer>().material = saveDataScriptableObject.playerChosenMaterial;
        ball.gameObject.GetComponent<MeshRenderer>().material = saveDataScriptableObject.ballChosenMaterial;
    }

    private void ReadAndroidInput()
    {
        if (Input.touchCount > 0 && !ignoreInputs)
        {
            // check initial touch and convert accordind to screen size and
            // convert the slide distance to range 0 - 1 and set this value on slider with inverse lerp
            touch = Input.GetTouch(0);
            if (inputInitPosition == Vector3.zero)
            {
                inputInitPosition = touch.position;
                maxPosY = touch.position;
                pixelInitPerc = inputInitPosition.y * 100 / Screen.height;
                pixelMaxPerc = pixelInitPerc + PERCENTAGE_MAX_OF_SCREEN_HEIGHT_FOR_SLIDER;
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
            if (touch.phase == TouchPhase.Ended && throwingPowerSlider.value > PERCENTAGE_MIN_OF_POWER_SLIDER_TO_THWOW_BALL || swipingTimer >= maxSwipingTimer)
            {
                StartCoroutine(ThrowingBall(throwingPowerSlider.value, isFireBonusActive));
            }
        }
    }

    private void ReadPCInput()
    {
        if (!ignoreInputs)
        {
            if (Input.GetMouseButtonDown(0) && !mouseMovementStarted)
            {
                // check initial click and convert accordind to screen size and
                // convert the slide distance to range 0 - 1 and set this value on slider with inverse lerp
                mouseStartPosition = Input.mousePosition;
                maxPosY = Input.mousePosition;
                mouseMovementStarted = true;
                pixelInitPerc = mouseStartPosition.y * 100 / Screen.height;
                pixelMaxPerc = pixelInitPerc + PERCENTAGE_MAX_OF_SCREEN_HEIGHT_FOR_SLIDER;
                pixelMaxY = pixelMaxPerc * Screen.height / 100;
            }
            else if (Input.GetMouseButton(0) && mouseMovementStarted && swipingTimer < maxSwipingTimer)
            {
                swipingTimer += Time.deltaTime;
                if (Input.mousePosition.y > maxPosY.y)
                {
                    throwingPowerSlider.value = Mathf.InverseLerp(mouseStartPosition.y, pixelMaxY, Input.mousePosition.y);
                    maxPosY = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButtonUp(0) && throwingPowerSlider.value > PERCENTAGE_MIN_OF_POWER_SLIDER_TO_THWOW_BALL || swipingTimer >= maxSwipingTimer)
            {
                mouseMovementStarted = false;
                StartCoroutine(ThrowingBall(throwingPowerSlider.value, isFireBonusActive));
            }
        }
    }
}
