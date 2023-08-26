using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotatePlayerOnUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private PlayerUI3D playerUI;

    public void OnPointerEnter(PointerEventData eventData)
    {
        playerUI.canRotate = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        playerUI.canRotate = false;
    }
}
