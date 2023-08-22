using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    protected AudioSource audioSource;

    [Header("reference")]
    [SerializeField]
    protected GameManager gameManager;

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

    [SerializeField]
    protected Transform throwEndPosition;
    protected Transform throwEndPositionChild;

    protected int currentPlayerPosition = 0;
    protected float throwingTimer;
    protected bool isThrowingBall;

    protected bool doBackboardShot;
    protected int pointsEarned;

    internal bool ignoreInputs;
    protected bool makePoints;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    protected virtual void Start()
    {
        currentPlayerPosition = Random.Range(0, playerPositions.Length);
        transform.position = playerPositions[currentPlayerPosition].position;
        transform.eulerAngles = playerPositions[currentPlayerPosition].eulerAngles;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isThrowingBall && !ignoreInputs)
        {
            ball.transform.position = dribblePosition.position + Vector3.up * 0.5f + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * dribbleHeight));
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
            movingPercent = movingTimer / 0.5f;
            transform.position = Vector3.Lerp(startPosition, playerPositions[currentPlayerPosition].position, movingPercent);
            transform.eulerAngles = Vector3.Lerp(startRotation, playerPositions[currentPlayerPosition].eulerAngles, movingPercent);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);
        ignoreInputs = false;
    }
}
