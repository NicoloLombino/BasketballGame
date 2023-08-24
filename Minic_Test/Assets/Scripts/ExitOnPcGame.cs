using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
