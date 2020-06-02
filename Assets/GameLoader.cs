using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoader : MonoBehaviour
{
    [SerializeField]
    private Slider loadingBar = null;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadAsynchronously());
    }

    IEnumerator LoadAsynchronously()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(1);  // The main scene has an index of 1.
        while (!operation.isDone)
        {
            loadingBar.value = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }
    }
}
