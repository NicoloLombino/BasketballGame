using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    [Header("reference")]
    [SerializeField]
    protected GameManager gameManager;

    [Header("Ball components")]
    [SerializeField]
    protected Transform ball;
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
    protected Transform throwEndPosition;
    [SerializeField]
    protected Transform[] playerPositions;

    protected int currentPlayerPosition = 0;
    protected float throwingTimer;
    public Transform throwEndPositionWithRandomError;
    protected bool isThrowingBall;

    internal bool ignoreInputs;
    protected bool makePoints;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isThrowingBall && !ignoreInputs)
        {
            ball.position = dribblePosition.position + Vector3.up * 0.5f + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * dribbleHeight));
        }
    }
}
