using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour
{
    public GameObject elementPrefab;
    public RectTransform elementList;

    public void AddElement(int id, string text)
    {
        GameObject temp = Instantiate(elementPrefab, elementList);
        temp.name = id.ToString();
        temp.transform.Find("ElementText").GetComponent<Text>().text = text;
    }

    public void ClearElement()
    {
        for (int i = 0; i < elementList.transform.childCount; i++)
        {
            Destroy(elementList.transform.GetChild(i).gameObject);
        }
    }
}
