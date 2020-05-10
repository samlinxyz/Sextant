using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProgressData
{

    public bool[][] progress;

    public ProgressData(Levels levels)
    {
        progress = levels.GetProgress();
    }

}
