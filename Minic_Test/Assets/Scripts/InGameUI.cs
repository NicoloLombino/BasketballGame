using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUI : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField]
    private float gameTime;
    private float timer;


    [SerializeField]
    private TextMeshProUGUI playerPointsText;
    [SerializeField]
    private TextMeshProUGUI aiPointsText;
    [SerializeField]
    private Image timerImage;

    [SerializeField]
    [Range(500, 700)]
    public float sliderValuePerfectShot;
    [SerializeField]
    [Range(700, 900)]
    public float sliderValueBackboardShot;

    void Start()
    {
        timer = gameTime;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        timerImage.fillAmount = timer / gameTime;
    }

    public float GetSliderValuePerfectShot()
    {
        return sliderValuePerfectShot;
    }
    public float GetSliderValuebackboardShot()
    {
        return sliderValueBackboardShot;
    }
}
