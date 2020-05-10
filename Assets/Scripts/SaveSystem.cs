using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveProgress(Levels levels)
    {
        Debug.Log("saving progress");
        string path = Path.Combine(Application.persistentDataPath, "progress.lol");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        ProgressData data = new ProgressData(levels);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static ProgressData LoadProgress()
    {
        Debug.Log("Loading Progress");
        string path = Path.Combine(Application.persistentDataPath, "progress.lol");

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            ProgressData data = formatter.Deserialize(stream) as ProgressData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }
}
