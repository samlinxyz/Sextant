using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Levels : MonoBehaviour
{
    [SerializeField, Range(0f, 90f)]
    float diffractionAngle;
    public float DiffractionAngle { get { return diffractionAngle; } }

    public static Levels instance;

    public ConstellationLines[] levels;

    void Awake ()
    {
        instance = this;
        levels = GetComponentsInChildren<ConstellationLines>();
    }

    public void SaveProgress()
    {
        SaveSystem.SaveProgress(this);
    }

    public void LoadProgress()
    {
        ProgressData data = SaveSystem.LoadProgress();

        SetProgress(data.progress);
    }

    //  Note: resetprogress only clears all completion in the game, and not in the save file.
    //  eraseprogress calls resetprogress and saves.
    public void EraseProgress()
    {
        Debug.Log("LOL You just erased all your progress.");

        ResetProgress();

        SaveSystem.SaveProgress(this);
    }

    public bool[][] GetProgress()
    {
        int levelCount = levels.Length;
        bool[][] progress = new bool[levelCount][];
        for (int i = 0; i < levelCount; i++) progress[i] = levels[i].GetProgress();
        return progress;
    }

    public void SetProgress(bool[][] progress)
    {
        string everything = "";
        for (int i = 0; i < progress.Length; i++)
        {
            for (int j = 0; j < progress[i].Length; j++)
            {
                everything += progress[i][j].ToString();
            }
            everything += "\r\n";
        }
        Debug.Log(everything);
        for (int i = 0; i < levels.Length; i++)
            levels[i].SetProgress(progress[i]);
    }

    //  Note: resetprogress only clears all completion in the game, and not in the save file.
    //  eraseprogress calls resetprogress and saves.
    public void ResetProgress()
    {
        bool[] progress = new bool[50];
        for (int i = 0; i < 50; i++) progress[i] = false;
        for (int i = 0; i < levels.Length; i++) levels[i].SetProgress(progress);
    }
}
