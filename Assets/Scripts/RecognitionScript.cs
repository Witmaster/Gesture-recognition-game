using System.Collections;
using UnityEngine;

public class RecognitionScript
{
    RecognitionScript() { }
    public static float Match(Vector2[] template, Vector3[] gesture)
    {
        Vector2[] processedGesture = NormalizeGesture(gesture);
        ScaleGesture(processedGesture);
        return getMatchRate(template, processedGesture);
    }
    public static float Match(Vector2[] template, Vector2[] gesture)
    {
        return getMatchRate(template, gesture);
    }
    public static Vector2[] ProcessNewTemplate(Vector3[] gesture)
    {
        Vector2[] result = NormalizeGesture(gesture);
        ScaleGesture(result);
        return result;
    }
    private static float getMatchRate(Vector2[] template, Vector2[] gesture)
    {
        float sumOfDistances = 0.0f;
        for (int index = 0; index < gesture.Length; index++)
        {
            sumOfDistances += distance(template[index], gesture[index]);
        }
        sumOfDistances /= 64; // get average distance
        //convert distance to [0..1] score and return
        //actual formula in parentheses is Sqrt(pow(size,2) + pow(size,2)) / 2, but we have fixed size of 10 so 200
        return 1 - (sumOfDistances / (Mathf.Sqrt(200) / 2));
    }
    private static void ScaleGesture(Vector2[] gesture)
    {
        Rect boundingBox = GetBoundingBox(gesture); //Rectangle encasing the gesture
        for (int index = 0; index < gesture.Length; index++)
        {//scale gesture size to fit gamearea size (is set to 10 Unity units) and translate it to be centered at [0,0]
            gesture[index].x -= boundingBox.center.x;
            gesture[index].y -= boundingBox.center.y;
            gesture[index].x *= 10 / boundingBox.width;
            gesture[index].y *= 10 / boundingBox.height;
        }
    }
    private static Rect GetBoundingBox(Vector2[] gesture)
    {
        Vector2 min = new Vector2(0, 0);
        Vector2 max = new Vector2(0, 0);
        foreach (Vector2 item in gesture)
        {
            if (item.x < min.x)
                min.x = item.x;
            if (item.y < min.y)
                min.y = item.y;
            if (item.x > max.x)
                max.x = item.x;
            if (item.y > max.y)
                max.y = item.y;
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        
    }
    private static Vector2[] NormalizeGesture(Vector3[] gesture)
    {
        float avgLength = NormalizedLength(gesture); 
        Vector2[] result = new Vector2[64];
        result[0] = gesture[0];
        int resultIndex = 0;
        float cumulativeDistance = 0.0f;
        float lastDistance = 0.0f;
        Vector2 prevPoint;
        for (int index = 1; index < gesture.Length; index++)
        {
            if (cumulativeDistance == 0)
            {
                prevPoint = result[resultIndex];
            }
            else
            {
                prevPoint = gesture[index - 1];
            }
            lastDistance = distance(gesture[index], prevPoint);
            cumulativeDistance += lastDistance;
            if (cumulativeDistance >= avgLength)
            {
                result[++resultIndex] = new Vector2(
                    prevPoint.x + ((avgLength - cumulativeDistance + lastDistance) / lastDistance) * (gesture[index].x - prevPoint.x),
                    prevPoint.y + ((avgLength - cumulativeDistance + lastDistance) / lastDistance) * (gesture[index].y - prevPoint.y));
                    cumulativeDistance = 0.0f;
                index--;
            }
        }
        result[63] = gesture[gesture.Length - 1];
        return result;
    }
    private static float NormalizedLength(Vector3[] gesture)
    {
        float result = 0.0f;
        if (gesture.Length > 1)
        {
            for (int index = 1; index < gesture.Length; index++)
            {
                result += distance(gesture[index], gesture[index - 1]);
            }
        }
        return result / 63.0f;
    }
    public static float distance(Vector2 first, Vector2 second)
    {
        return Mathf.Sqrt(Mathf.Pow(first.x - second.x, 2) + Mathf.Pow(first.y - second.y, 2));
    }
}