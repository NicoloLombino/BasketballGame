using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : PlayerBase
{
    [Header("AI components")]
    [SerializeField, Tooltip("the time the AI wait before the next throw after all tasks")]
    private float timerToThrow;
    [SerializeField, Range(0, 10)]
    private int accuracy;

    private float timer;

    protected override void Start()
    {
        base.Start();

        timerToThrow += (10 / saveDataScriptableObject.AILevel) / 3;

        // set the materials to AI player and his Ball
        SetMaterialToBallAndAI(saveDataScriptableObject.playerMaterials, gameObject);
        SetMaterialToBallAndAI(saveDataScriptableObject.ballMaterials, ball.gameObject);

    }

    protected override void Update()
    {
        base.Update();

        if (gameManager.isInGame && !ignoreInputs)
        {
            timer += Time.deltaTime;
            if (timer >= timerToThrow)
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
        if (gameManager.isBackBoardBonusActive)
        {
            // aim to backboard
            throwValue = (gameManager.valueToBackboardAndPointsMin / 10);
        }
        else
        {
            // aim to basket to get 3 points
            throwValue = (gameManager.valueTo3PointsMin + gameManager.valueTo3PointsMax) / 20;
        }

        AIThrowingBall(throwValue);
    }

    /// <summary>
    /// throw the ball and set his movement
    /// </summary>
    /// <param name="throwPower"> It's the calculated value for the throw, as if it had its own slider</param>
    /// <param name="hasFireBonus"> if the fire bonus is active</param>
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

        CheckThrowingResult(throwPower, hasFireBonus);

        // the sound of throwing ball
        audioSource.Play();

        float throwingPercent = 0;
        while (throwingPercent < 1)
        {
            throwingTimer += Time.deltaTime;
            throwingPercent = throwingTimer / throwingDuration;
            ball.transform.position = Vector3.Lerp(throwStartPosition.position, throwEndPosition.position, throwingPercent)
                + Vector3.up * 5 * Mathf.Sin(throwingPercent * Mathf.PI);
            yield return null;
        }

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

        if (makePoints)
        {
            gameManager.AddAIPoints(pointsEarned, doBackboardShot, hasFireBonus);
        }
        gameManager.DoRandomBackboardBonus();

        ResetShot(-1);
    }

    /// <summary>
    ///  Set the materials to AI player and his ball, excluding those used by the player
    /// </summary>
    /// <param name="materials"> the materials array, (using the SAVE DATA) </param>
    /// <param name="obj"> the object to change the color of </param>
    private void SetMaterialToBallAndAI(Material[] materials, GameObject obj)
    {
        int rndMaterial = Random.Range(0, materials.Length);
        if (materials[rndMaterial] != saveDataScriptableObject.playerChosenMaterial)
        {
            obj.GetComponent<MeshRenderer>().material = materials[rndMaterial];
        }
        else
        {
            if (rndMaterial != materials.Length - 1)
            {
                rndMaterial = rndMaterial + 1;
            }
            else
            {
                rndMaterial = rndMaterial - 1;
            }

            obj.GetComponent<MeshRenderer>().material = materials[rndMaterial];
        }
    }
}