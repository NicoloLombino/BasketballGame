using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    [Header("AI components")]
    [SerializeField]
    private float timerToThrow;
    [SerializeField]
    private float accuracy;

    private float timer;

    void Start()
    {
        
    }

    void Update()
    {
        if(!isThrowingBall)
        {
            //ball.position = dribblePosition.position + Vector3.up * 0.5f + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * dribbleHeight));
        }
    }
}
