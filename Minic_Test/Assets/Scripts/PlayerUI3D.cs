using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A player used only for display in the UI
/// </summary>
public class PlayerUI3D : MonoBehaviour
{
    [SerializeField]
    private SaveData SaveDataScriptableObject;

    [Header("Ball components"), Tooltip("use the same components of Player on Game scene")]
    [SerializeField]
    protected Transform ball;
    [SerializeField]
    protected Transform dribblePosition;
    [SerializeField]
    protected float dribbleHeight;

    // components for rotation on UI
    internal bool canRotate;
    private Vector2 rotationStartPos;

    void Start()
    {
        // set the materials to player and his Ball for 3D UI
        SetMaterialOnPlayer(PlayerPrefs.GetInt("PlayerMaterialIndex"));
        SetMaterialOnBall(PlayerPrefs.GetInt("BallMaterialIndex"));
    }

    void Update()
    {
        ball.position = dribblePosition.position + Vector3.up * 0.5f + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * dribbleHeight));

        if(canRotate)
        {
            ReadInputToRotationOnUI();
        }
    }

    public void SetMaterialOnPlayer(int materialIndex)
    {
        gameObject.GetComponent<MeshRenderer>().material = SaveDataScriptableObject.playerMaterials[materialIndex];
        SaveDataScriptableObject.SetPlayerMaterial(materialIndex);
    }

    public void SetMaterialOnBall(int materialIndex)
    {
        ball.gameObject.GetComponent<MeshRenderer>().material = SaveDataScriptableObject.ballMaterials[materialIndex];
        SaveDataScriptableObject.SetBallMaterial(materialIndex);
    }

    public void SetRotationToZero()
    {
        transform.eulerAngles = Vector3.zero;
    }

    /// <summary>
    /// rotate the player used on UI 3D
    /// </summary>
    private void ReadInputToRotationOnUI()
    {
        if (SaveDataScriptableObject.isMobileGame && Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    rotationStartPos = touch.position;
                    break;

                case TouchPhase.Moved:
                    Vector2 touchDelta = touch.position - rotationStartPos;
                    float rotationAmount = touchDelta.x * 0.2f;
                    transform.Rotate(Vector3.up, rotationAmount);
                    rotationStartPos = touch.position;
                    break;
            }
        }
        else if (!SaveDataScriptableObject.isMobileGame && Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 360 * Time.deltaTime);
        }
    }
}
