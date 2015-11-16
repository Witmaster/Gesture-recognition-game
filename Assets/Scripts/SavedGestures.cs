using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

[Serializable]
class SavedGestures
{
    private List<Vector2[]> gestures = new List<Vector2[]>();
    public int highScore = 0;
    private System.Random rand = new System.Random();
    private static SavedGestures instance = null;
    private SavedGestures()
        {
        }
    public static void initialize() // in case something happens to savefile, it would be reinitialized to this
    {
        instance = new SavedGestures();
        instance.highScore = 0;
        Vector3[] square = new Vector3[5] { new Vector3(5, 5, 0), new Vector3(5, -5, 0), new Vector3(-5, -5, 0), new Vector3(-5, 5, 0), new Vector3(5, 5, 0) };
        instance.gestures.Add(RecognitionScript.ProcessNewTemplate(square));
        Vector3[] triangle = new Vector3[4] { new Vector3(0, 4, 0), new Vector3(2, 0, 0), new Vector3(-2, 0, 0), new Vector3(0, 4, 0) };
        instance.gestures.Add(RecognitionScript.ProcessNewTemplate(triangle));
        Vector3[] star = new Vector3[6] { new Vector3(0, 2, 0), new Vector3(2, -2, 0), new Vector3(-2, 1, 0), new Vector3(2, 1, 0), new Vector3(-2, -2, 0), new Vector3(0, 2, 0) };
        instance.gestures.Add(RecognitionScript.ProcessNewTemplate(star));
        instance.Save();
        instance.Load();
    }
    public static SavedGestures GetInstance()
        {
            if (instance == null)
            {
                instance = new SavedGestures();
                instance.Load();
            }
            return instance;
        }
    public void Add(Vector2[] newGesture)
    {
        float maxResemblance = 0.0f;
        if (gestures.Count > 0)
        {
            foreach (Vector2[] item in gestures)
            {
                float thisResemblance = RecognitionScript.Match(item, newGesture);
                if (thisResemblance > maxResemblance)
                    maxResemblance = thisResemblance;
            }
            if (maxResemblance < 0.9f)
            {
                gestures.Add(newGesture);
                Save();
            }
        }
    }
    public int Count()
        {
          return gestures.Count;
        }
    public void Save()
        {
        SavedData save = new SavedData(gestures, highScore);
        save.Save();
        }
    public void Load()
    {
        try
        {
            SavedData save = SavedData.Load();
            highScore = save.HighScore;
            foreach (SerializableVector2[] item in save.storedGestures)
            {
                Vector2[] tempArray = new Vector2[64];
                for (int index = 0; index < item.Length; index++)
                {
                    tempArray[index] = new Vector2(item[index].x, item[index].y);
                }
                instance.gestures.Add(tempArray);
            }
        }
        catch(Exception)
        {
            SavedGestures.initialize();
        }
    }
    public Vector2[] GetRandomGesture()
        {
        if (gestures.Count == 0)
            instance.Load();
        return gestures[rand.Next(0, gestures.Count)];
        }
}

[Serializable]
struct SerializableVector2
{
    public float x;
    public float y;
    public SerializableVector2(Vector2 vector)
    {
        x = vector.x;
        y = vector.y;
    }
}
[Serializable]
struct SavedData
{
    public List<SerializableVector2[]> storedGestures;
    public int HighScore;
    public SavedData(List<Vector2[]> gestures, int highScore)
    {
        storedGestures = new List<SerializableVector2[]>();
        HighScore = highScore;
        foreach (Vector2[] item in gestures)
        {
            SerializableVector2[] tempArray = new SerializableVector2[64];
            for (int index = 0; index < item.Length; index++)
            {
                tempArray[index] = new SerializableVector2(item[index]);
            }
            storedGestures.Add(tempArray);
        }
    }
    public void Save()
    {
        using (FileStream stream = File.Open(Application.persistentDataPath + "/save.dat", FileMode.OpenOrCreate))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, this);
        }
    }
    public static SavedData Load()
    {
        try
        {
            FileStream stream = File.Open(Application.persistentDataPath + "/save.dat", FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            return (SavedData)bf.Deserialize(stream);
        }
        catch (Exception)
        {
            throw;
        }
    }
}

