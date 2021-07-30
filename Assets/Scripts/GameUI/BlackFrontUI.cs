using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackFrontUI : IGameUISystem
{
    [SerializeField]
    private Image m_BlackImage = default;
    [SerializeField]
    private float m_DefaultTime = 1f;

    public bool IsDark { get { return m_BlackImage.raycastTarget; } }

    public void SetToDark()
    {
        StartCoroutine(ToDark());
    }

    public void SetToDarkBack()
    {
        StartCoroutine(ToDark(true));
    }

    public void SetToLight()
    {
        StartCoroutine(ToLight());
    }

    public void SetDarkValue(float value)
    {
        if (value > 0)
        {
            Color color = Color.black;
            color.a = value;
            m_BlackImage.raycastTarget = true;
            m_BlackImage.color = color;
        }
        else
        {
            m_BlackImage.raycastTarget = false;
            m_BlackImage.color = Color.clear;
        }
    }

    private IEnumerator ToDark(bool toLight = false)
    {
        float time = 0;
        Color color = Color.black;
        m_BlackImage.raycastTarget = true;
        while (time < m_DefaultTime)
        {
            time += Time.deltaTime;
            color.a = time / m_DefaultTime;
            m_BlackImage.color = color;
            yield return null;
        }
        if (toLight)
        {
            SetToLight();
        }
        else
        {
            m_BlackImage.raycastTarget = false;
            m_BlackImage.color = Color.clear; ;
        }
    }

    private IEnumerator ToLight()
    {
        float time = m_DefaultTime;
        Color color = Color.black;
        m_BlackImage.raycastTarget = true;
        while (time > 0)
        {
            time -= Time.deltaTime;
            color.a = time / m_DefaultTime;
            m_BlackImage.color = color;
            yield return null;
        }
        m_BlackImage.raycastTarget = false;
        m_BlackImage.color = Color.clear; ;
    }
}
