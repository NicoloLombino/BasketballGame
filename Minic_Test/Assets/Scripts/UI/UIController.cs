using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public UIInfo m_UIInfor;
    public bool m_IsHorizontal;

    [Header("UI Element")]
    public RectTransform m_UI_1;
    public RectTransform m_UI_2;
    public RectTransform m_UI_3;
    public RectTransform m_UI_4;

    public void Func_SetUpUI()
    {
        UIInfo.UIInfor _temp = new UIInfo.UIInfor();
        if (m_IsHorizontal)
        {
            _temp = m_UIInfor.m_HorizontalInfor[0];
            Func_SetUI_1(_temp);
            _temp = m_UIInfor.m_HorizontalInfor[1];
            Func_SetUI_2(_temp);
        }
        else
        {
            _temp = m_UIInfor.m_VerticalInfor[0];
            Func_SetUI_1(_temp);
            _temp = m_UIInfor.m_VerticalInfor[1];
            Func_SetUI_2(_temp);
        }
    }
    private void Func_SetUI_1(UIInfo.UIInfor _UIInfor)
    {
        m_UI_1.anchorMin = _UIInfor.m_AnchorInfor.m_Min;
        m_UI_1.anchorMax = _UIInfor.m_AnchorInfor.m_Max;
        m_UI_1.anchoredPosition = _UIInfor.m_UpInfor;
        m_UI_1.sizeDelta = _UIInfor.m_DownInfor;
    }
    private void Func_SetUI_2(UIInfo.UIInfor _UIInfor)
    {
        m_UI_2.anchorMin = _UIInfor.m_AnchorInfor.m_Min;
        m_UI_2.anchorMax = _UIInfor.m_AnchorInfor.m_Max;

        m_UI_2.anchoredPosition = new Vector2(m_UI_2.anchoredPosition.x, _UIInfor.m_UpInfor.y);
        m_UI_2.sizeDelta = new Vector2(m_UI_2.sizeDelta.x, _UIInfor.m_DownInfor.y);

        m_UI_2.offsetMin = new Vector2(_UIInfor.m_UpInfor.x, m_UI_2.offsetMin.y);
        m_UI_2.offsetMax = new Vector2(_UIInfor.m_DownInfor.x * -1, m_UI_2.offsetMax.y);
    }
}
