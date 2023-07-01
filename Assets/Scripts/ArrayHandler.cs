using UnityEngine;

public class ArrayHandler
{
    public object[] MixArray(object[] mixedArray)
    {
        for (int i = 0; i < mixedArray.Length; i++)
        {
            object currentValue = mixedArray[i];
            int randomValue = Random.Range(i, mixedArray.Length);

            mixedArray[i] = mixedArray[randomValue];
            mixedArray[randomValue] = currentValue;
        }
        return mixedArray;
    }
}
