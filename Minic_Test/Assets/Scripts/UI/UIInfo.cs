using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new UIINfo", menuName = "Assets/UIInfo")]
public class UIInfo : ScriptableObject
{
    public UIInfor[] m_HorizontalInfor;
    public UIInfor[] m_VerticalInfor;


    public UIInfor Func_GetUIInfor(in string _name, in bool _IsUIHorizsontal)
    {
        if (_IsUIHorizsontal)
        {
            for (int i = 0; i < m_HorizontalInfor.Length; i++)
            {
                if (m_HorizontalInfor[i].m_Name == _name)
                {
                    return m_HorizontalInfor[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < m_VerticalInfor.Length; i++)
            {
                if (m_VerticalInfor[i].m_Name == _name)
                {
                    return m_VerticalInfor[i];
                }
            }
        }

        return new UIInfor();
    }

    [System.Serializable]
    public struct UIInfor
    {
        public string m_Name;
        [Tooltip("Set up by Recttransform.anchoredPosition")]

        public Vector2 m_UpInfor;
        [Tooltip("Set up by Recttransform.sizeDelta")]

        public Vector2 m_DownInfor;

        [Tooltip("Achor Preset")]
        public AnchorInfor m_AnchorInfor;

        [System.Serializable]
        public struct AnchorInfor
        {
            public Vector2 m_Min;
            public Vector2 m_Max;
        }
    }

}
