using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// used to exit from the game in a PC platform
/// </summary>
public class ExitOnPcGame : MonoBehaviour
{
    [SerializeField]
    private GameObject exitGameUI;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            exitGameUI.SetActive(!exitGameUI.activeInHierarchy);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
