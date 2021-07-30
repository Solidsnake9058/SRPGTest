using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIBlock : MonoBehaviour, IPointerClickHandler
{
    private UnityAction<PointerEventData> m_PointerEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        m_PointerEvent?.Invoke(eventData);
    }

    public void SetPointEvent(UnityAction<PointerEventData> action)
    {
        m_PointerEvent = action;
    }
}
