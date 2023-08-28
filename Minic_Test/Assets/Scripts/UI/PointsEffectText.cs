using UnityEngine;
using TMPro;

public class PointsEffectText : MonoBehaviour
{
    public float lifeTime;

    [Header("Components")]
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI upText;

    [Header("Animation")]
    [SerializeField]
    private bool isAnimated;
    [SerializeField]
    private float movementSpeed;
    [SerializeField, Tooltip("need to be less of of text size")]
    private float fontIncrement;

    private float startFontSizePoints;
    private float startFontSizeUp;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        if(isAnimated)
        {
            startFontSizePoints = pointsText.fontSize;
            startFontSizeUp = upText.fontSize;
        }
    }
    void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);

        if(isAnimated)
        {
            transform.position += Vector3.up * movementSpeed * Time.deltaTime;
            float sizeTimer = Mathf.PingPong(Time.time, 1);
            pointsText.fontSize = Mathf.Lerp(startFontSizePoints - fontIncrement, startFontSizePoints + fontIncrement, sizeTimer);
            upText.fontSize = Mathf.Lerp(startFontSizeUp - fontIncrement, startFontSizeUp + fontIncrement, sizeTimer);
        }
    }
}
