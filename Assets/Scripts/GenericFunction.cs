using System.Collections;
using System.Collections.Generic;

public class GenericFunction
{
    public static void Swap<T>(ref T item1, ref T item2)
    {
        T temp = item1;
        item1 = item2;
        item2 = temp;
    }

    public static List<T> Shuffle<T>(IEnumerable<T> values)
    {
        List<T> list = new List<T>(values);
        T tmp;
        int iS;
        for (int N1 = 0; N1 < list.Count; N1++)
        {
            iS = UnityEngine.Random.Range(0, list.Count);
            tmp = list[N1];
            list[N1] = list[iS];
            list[iS] = tmp;
        }
        return list;
    }

}
