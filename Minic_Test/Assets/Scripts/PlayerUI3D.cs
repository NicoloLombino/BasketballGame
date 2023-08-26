using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A player used solely for display in the UI
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

    void Start()
    {
        // set the materials to player and his Ball for 3D UI
        SetMaterialOnPlayer(PlayerPrefs.GetInt("PlayerMaterialIndex"));
        SetMaterialOnBall(PlayerPrefs.GetInt("BallMaterialIndex"));
    }

    void Update()
    {
        ball.position = dribblePosition.position + Vector3.up * 0.5f + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * dribbleHeight));
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
}
